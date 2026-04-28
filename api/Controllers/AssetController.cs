using data;
using data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using api.Services;
using Domain.Entities;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpPost("/api/optimization/run")]
        public async Task<ActionResult<OptimizationRunDto>> RunOptimization(
            [FromBody] OptimizationRunRequest request,
            [FromServices] HeatOptimizer heatOptimizer,
            [FromServices] ElectricityPriceSeriesService priceSeriesService,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            DateTime nowHourUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc);

            DateTime? requestFrom = request.TimeFromUtc?.ToUniversalTime();
            DateTime? requestTo = request.TimeToUtc?.ToUniversalTime();

            string? season = string.IsNullOrWhiteSpace(request.Season) ? null : request.Season.Trim();

            IQueryable<HeatPricePoint> marketQuery = _context.HeatPricePoints.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(season))
            {
                marketQuery = marketQuery.Where(x => x.Season == season);
            }

            if (requestFrom.HasValue)
            {
                DateTime fromUtc = TruncateToHourUtc(requestFrom.Value);
                marketQuery = marketQuery.Where(x => x.TimeFrom >= fromUtc);
            }

            if (requestTo.HasValue)
            {
                DateTime toUtc = TruncateToHourUtc(requestTo.Value);
                marketQuery = marketQuery.Where(x => x.TimeFrom < toUtc);
            }

            List<HeatPricePoint> marketRows = await marketQuery
                .OrderBy(x => x.TimeFrom)
                .ToListAsync(cancellationToken);

            if (marketRows.Count == 0)
            {
                return NotFound("No market points found for the selected time range/season.");
            }

            DateTime timeFromUtc = EnsureUtc(marketRows.First().TimeFrom);
            DateTime timeToUtcExclusive = EnsureUtc(marketRows.Last().TimeFrom).AddHours(1);

            ElectricityPriceSourceDto chosenSource = request.ElectricityPriceSource;
            Dictionary<DateTime, decimal>? apiPrices = null;

            if (chosenSource == ElectricityPriceSourceDto.Api)
            {
                try
                {
                    apiPrices = await priceSeriesService.GetPricesAsync(timeFromUtc, timeToUtcExclusive, cancellationToken);
                }
                catch
                {
                    apiPrices = null;
                }

                if (apiPrices == null || apiPrices.Count == 0)
                {
                    chosenSource = ElectricityPriceSourceDto.Database;
                }
            }

            List<MarketPoint> market = new(marketRows.Count);
            foreach (HeatPricePoint row in marketRows)
            {
                DateTime hourUtc = EnsureUtc(row.TimeFrom);
                decimal electricityPrice = row.ElectricityPriceDkkPerMWh;
                if (chosenSource == ElectricityPriceSourceDto.Api && apiPrices != null && apiPrices.TryGetValue(hourUtc, out decimal apiPrice))
                {
                    electricityPrice = apiPrice;
                }

                market.Add(new MarketPoint(hourUtc, row.HeatDemandMWh, electricityPrice));
            }

            List<ProductionUnit> units = await _context.ProductionUnits
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            HashSet<int>? enabledSet = request.EnabledProductionUnitIds != null && request.EnabledProductionUnitIds.Count > 0
                ? new HashSet<int>(request.EnabledProductionUnitIds)
                : null;

            var optimizerUnits = units
                .Select(u => (u.Id, u.Data, Enabled: enabledSet == null || enabledSet.Contains(u.Id)))
                .ToList();

            var maintenance = request.Maintenance?
                .Select(x => new MaintenanceWindow(
                    x.ProductionUnitId,
                    TruncateToHourUtc(x.StartUtc),
                    TruncateToHourUtc(x.EndUtc)))
                .ToList();

            var settings = new OptimizationSettings(
                Objective: (OptimizationObjective)request.Objective,
                CostWeight: request.CostWeight,
                Co2Weight: request.Co2Weight);

            OptimizationResult optimization;
            try
            {
                optimization = heatOptimizer.Optimize(new OptimizationInput(
                    market,
                    optimizerUnits,
                    maintenance,
                    settings));
            }
            catch (InfeasibleSolutionException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (TimeSeriesAlignmentException ex)
            {
                return BadRequest(ex.Message);
            }

            OptimizationRun runEntity = new()
            {
                CreatedAtUtc = nowHourUtc,
                Objective = settings.Objective.ToString(),
                CostWeight = settings.CostWeight,
                Co2Weight = settings.Co2Weight,
                ElectricityPriceSource = chosenSource.ToString(),
                TimeFromUtc = timeFromUtc,
                TimeToUtc = timeToUtcExclusive,
                TotalNetCostDkk = optimization.TotalNetCostDkk,
                TotalCo2Kg = optimization.TotalCo2Kg,
                TotalElectricityCashflowDkk = optimization.TotalElectricityCashflowDkk,
                Hours = optimization.Hours.Select(h => new OptimizationHourResult
                {
                    TimeFromUtc = h.TimeFromUtc,
                    HeatDemandMWh = h.HeatDemandMWh,
                    ElectricityPriceDkkPerMWh = h.ElectricityPriceDkkPerMWh,
                    HeatSuppliedMWh = h.HeatSuppliedMWh,
                    TotalNetCostDkk = h.TotalNetCostDkk,
                    TotalCo2Kg = h.TotalCo2Kg,
                    ElectricityCashflowDkk = h.ElectricityCashflowDkk,
                    UnitResults = h.UnitResults.Select(u => new OptimizationUnitHourResult
                    {
                        ProductionUnitId = u.ProductionUnitId,
                        UnitName = u.UnitName,
                        HeatProducedMWh = u.HeatProducedMWh,
                        ElectricityMWh = u.ElectricityMWh,
                        NetCostDkk = u.NetCostDkk,
                        Co2Kg = u.Co2Kg,
                        ScorePerMWh = u.ScorePerMWh
                    }).ToList()
                }).ToList()
            };

            _context.OptimizationRuns.Add(runEntity);
            await _context.SaveChangesAsync(cancellationToken);

            OptimizationRunDto dto = MapRunToDto(runEntity);
            return CreatedAtAction(nameof(GetOptimizationRun), new { id = runEntity.Id }, dto);
        }

        [HttpGet("/api/optimization/runs")]
        public async Task<ActionResult<IEnumerable<OptimizationRunDto>>> GetOptimizationRuns(CancellationToken cancellationToken)
        {
            var runs = await _context.OptimizationRuns
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(r => new OptimizationRunDto
                {
                    Id = r.Id,
                    CreatedAtUtc = r.CreatedAtUtc,
                    Objective = r.Objective,
                    CostWeight = r.CostWeight,
                    Co2Weight = r.Co2Weight,
                    ElectricityPriceSource = r.ElectricityPriceSource,
                    TimeFromUtc = r.TimeFromUtc,
                    TimeToUtc = r.TimeToUtc,
                    TotalNetCostDkk = r.TotalNetCostDkk,
                    TotalCo2Kg = r.TotalCo2Kg,
                    TotalElectricityCashflowDkk = r.TotalElectricityCashflowDkk,
                    Hours = new List<OptimizationHourResultDto>()
                })
                .ToListAsync(cancellationToken);

            return Ok(runs);
        }

        [HttpGet("/api/optimization/runs/{id:int}")]
        public async Task<ActionResult<OptimizationRunDto>> GetOptimizationRun(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest("Run ID must be a positive integer.");
            }

            OptimizationRun? run = await _context.OptimizationRuns
                .AsNoTracking()
                .Include(x => x.Hours)
                .ThenInclude(x => x.UnitResults)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (run == null)
            {
                return NotFound($"Optimization run with ID {id} not found.");
            }

            return Ok(MapRunToDto(run));
        }

        private static DateTime TruncateToHourUtc(DateTime utcDateTime)
        {
            DateTime dt = utcDateTime.Kind == DateTimeKind.Utc
                ? utcDateTime
                : utcDateTime.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc)
                    : utcDateTime.ToUniversalTime();
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc);
        }

        private static DateTime EnsureUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : dateTime.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                    : dateTime.ToUniversalTime();
        }

        private static OptimizationRunDto MapRunToDto(OptimizationRun run)
        {
            return new OptimizationRunDto
            {
                Id = run.Id,
                CreatedAtUtc = EnsureUtc(run.CreatedAtUtc),
                Objective = run.Objective,
                CostWeight = run.CostWeight,
                Co2Weight = run.Co2Weight,
                ElectricityPriceSource = run.ElectricityPriceSource,
                TimeFromUtc = EnsureUtc(run.TimeFromUtc),
                TimeToUtc = EnsureUtc(run.TimeToUtc),
                TotalNetCostDkk = run.TotalNetCostDkk,
                TotalCo2Kg = run.TotalCo2Kg,
                TotalElectricityCashflowDkk = run.TotalElectricityCashflowDkk,
                Hours = run.Hours
                    .OrderBy(x => x.TimeFromUtc)
                    .Select(h => new OptimizationHourResultDto
                    {
                        TimeFromUtc = EnsureUtc(h.TimeFromUtc),
                        HeatDemandMWh = h.HeatDemandMWh,
                        ElectricityPriceDkkPerMWh = h.ElectricityPriceDkkPerMWh,
                        HeatSuppliedMWh = h.HeatSuppliedMWh,
                        TotalNetCostDkk = h.TotalNetCostDkk,
                        TotalCo2Kg = h.TotalCo2Kg,
                        ElectricityCashflowDkk = h.ElectricityCashflowDkk,
                        UnitResults = h.UnitResults
                            .OrderBy(x => x.ProductionUnitId)
                            .Select(u => new OptimizationUnitHourResultDto
                            {
                                ProductionUnitId = u.ProductionUnitId,
                                UnitName = u.UnitName,
                                HeatProducedMWh = u.HeatProducedMWh,
                                ElectricityMWh = u.ElectricityMWh,
                                NetCostDkk = u.NetCostDkk,
                                Co2Kg = u.Co2Kg,
                                ScorePerMWh = u.ScorePerMWh
                            }).ToList()
                    }).ToList()
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionUnit>>> GetAllAssets()
        {
            var assets = await _context.ProductionUnits
                .AsNoTracking()
                .ToListAsync();

            return Ok(assets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductionUnit>> GetAssetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Asset ID must be a positive integer.");
            }

            var asset = await _context.ProductionUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            return Ok(asset);
        }

        [HttpPost]
        public async Task<ActionResult<ProductionUnit>> CreateAsset([FromBody] ProductionUnit asset)
        {
            if (asset == null)
            {
                return BadRequest("Asset data is required.");
            }

            if (asset.Data == null)
            {
                return BadRequest("Asset data properties are required.");
            }

            if (string.IsNullOrWhiteSpace(asset.Data.Name))
            {
                return BadRequest("Asset name is required.");
            }

            if (string.IsNullOrWhiteSpace(asset.Data.ImageUrl))
            {
                return BadRequest("Asset image URL is required.");
            }

            _context.ProductionUnits.Add(asset);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssetById), new { id = asset.Id }, asset);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] ProductionUnit asset)
        {
            if (id <= 0)
            {
                return BadRequest("Asset ID must be a positive integer.");
            }

            if (asset == null)
            {
                return BadRequest("Asset data is required.");
            }

            if (asset.Data == null)
            {
                return BadRequest("Asset data properties are required.");
            }

            if (string.IsNullOrWhiteSpace(asset.Data.Name))
            {
                return BadRequest("Asset name is required.");
            }

            if (string.IsNullOrWhiteSpace(asset.Data.ImageUrl))
            {
                return BadRequest("Asset image URL is required.");
            }

            var existingAsset = await _context.ProductionUnits.FindAsync(id);

            if (existingAsset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            existingAsset.Data = asset.Data;
            existingAsset.Type = asset.Type;

            _context.ProductionUnits.Update(existingAsset);
            await _context.SaveChangesAsync();

            return Ok(existingAsset);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Asset ID must be a positive integer.");
            }

            var asset = await _context.ProductionUnits.FindAsync(id);

            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            _context.ProductionUnits.Remove(asset);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}