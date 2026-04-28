using data;
using data.Entities;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace optimizer
{
    /// <summary>
    /// Background worker that runs heat production optimization for both Scenario 1 and
    /// Scenario 2 on startup, then repeats every 6 hours.
    /// </summary>
    public class Worker(ILogger<Worker> logger, IDbContextFactory<AppDbContext> dbFactory) : BackgroundService
    {
        // Repeat interval — set short (1 h) so the worker is useful in practice.
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run immediately on startup, then on the interval.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunScenario1Async(stoppingToken);
                    await RunScenario2Async(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Optimization run failed. Will retry at next interval.");
                }

                try
                {
                    await Task.Delay(RunInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        // ── Scenario 1 ──────────────────────────────────────────────────────────────
        // Single heating network, heat demand, three gas boilers (GB1, GB2, GB3) and
        // one oil boiler (OB1). GB2 is put under maintenance for 48 hours in mid-winter.
        private async Task RunScenario1Async(CancellationToken ct)
        {
            logger.LogInformation("[S1] Starting Scenario 1 optimization (winter, 14 days).");

            await using AppDbContext db = await dbFactory.CreateDbContextAsync(ct);

            List<HeatPricePoint> market = await db.HeatPricePoints
                .AsNoTracking()
                .Where(x => x.Season == "Winter")
                .OrderBy(x => x.TimeFrom)
                .ToListAsync(ct);

            if (market.Count == 0)
            {
                logger.LogWarning("[S1] No Winter market data found — skipping.");
                return;
            }

            // Scenario 1 units: GB1 (id=1), GB2 (id=2), GB3 (id=3), OB1 (id=4).
            int[] scenario1UnitIds = [1, 2, 3, 4];

            List<data.Entities.ProductionUnit> units = await db.ProductionUnits
                .AsNoTracking()
                .Where(u => scenario1UnitIds.Contains(u.Id))
                .ToListAsync(ct);

            // GB2 is under maintenance for 48 hours starting at hour 48 (day 3).
            DateTime maintStart = EnsureUtc(market[48].TimeFrom);
            DateTime maintEnd   = EnsureUtc(market[96].TimeFrom); // 48 h later

            var maintenance = new List<MaintenanceWindow>
            {
                new MaintenanceWindow(2, maintStart, maintEnd)
            };

            logger.LogInformation("[S1] GB2 maintenance: {from} → {to}", maintStart, maintEnd);

            var optimizerUnits = units
                .Select(u => (u.Id, u.Data, Enabled: true))
                .ToList();

            var settings = new OptimizationSettings(
                Objective: OptimizationObjective.Cost,
                CostWeight: 1m,
                Co2Weight: 0m);

            var optimizer = new HeatOptimizer();
            OptimizationResult result = optimizer.Optimize(new OptimizationInput(
                BuildMarket(market),
                optimizerUnits,
                maintenance,
                settings));

            await PersistResultAsync(db, result, "Scenario1-Winter", settings, market, ct);
            logger.LogInformation("[S1] Done. Total net cost: {cost:F2} DKK, CO2: {co2:F1} kg",
                result.TotalNetCostDkk, result.TotalCo2Kg);
        }

        // ── Scenario 2 ──────────────────────────────────────────────────────────────
        // Single heating network, two gas boilers (GB1, GB2), gas motor (GM1) and
        // electric boiler (EB1). Electricity prices drive dispatch order.
        // GM1 is under maintenance for 48 hours starting at hour 48.
        private async Task RunScenario2Async(CancellationToken ct)
        {
            logger.LogInformation("[S2] Starting Scenario 2 optimization (summer, 14 days).");

            await using AppDbContext db = await dbFactory.CreateDbContextAsync(ct);

            List<HeatPricePoint> market = await db.HeatPricePoints
                .AsNoTracking()
                .Where(x => x.Season == "Summer")
                .OrderBy(x => x.TimeFrom)
                .ToListAsync(ct);

            if (market.Count == 0)
            {
                logger.LogWarning("[S2] No Summer market data found — skipping.");
                return;
            }

            // Scenario 2 units: GB1 (id=1), GB2 (id=2), GM1 (id=5), EB1 (id=6).
            int[] scenario2UnitIds = [1, 2, 5, 6];

            List<data.Entities.ProductionUnit> units = await db.ProductionUnits
                .AsNoTracking()
                .Where(u => scenario2UnitIds.Contains(u.Id))
                .ToListAsync(ct);

            // GM1 under maintenance for 48 hours starting at hour 48.
            DateTime maintStart = EnsureUtc(market[Math.Min(48, market.Count - 1)].TimeFrom);
            DateTime maintEnd   = EnsureUtc(market[Math.Min(96, market.Count - 1)].TimeFrom);

            var maintenance = new List<MaintenanceWindow>
            {
                new MaintenanceWindow(5, maintStart, maintEnd)
            };

            logger.LogInformation("[S2] GM1 maintenance: {from} → {to}", maintStart, maintEnd);

            var optimizerUnits = units
                .Select(u => (u.Id, u.Data, Enabled: true))
                .ToList();

            var settings = new OptimizationSettings(
                Objective: OptimizationObjective.Cost,
                CostWeight: 1m,
                Co2Weight: 0m);

            var optimizer = new HeatOptimizer();
            OptimizationResult result = optimizer.Optimize(new OptimizationInput(
                BuildMarket(market),
                optimizerUnits,
                maintenance,
                settings));

            await PersistResultAsync(db, result, "Scenario2-Summer", settings, market, ct);
            logger.LogInformation("[S2] Done. Total net cost: {cost:F2} DKK, CO2: {co2:F1} kg",
                result.TotalNetCostDkk, result.TotalCo2Kg);
        }

        // ── Helpers ─────────────────────────────────────────────────────────────────

        private static List<MarketPoint> BuildMarket(List<HeatPricePoint> rows)
        {
            return rows.Select(r => new MarketPoint(
                EnsureUtc(r.TimeFrom),
                r.HeatDemandMWh,
                r.ElectricityPriceDkkPerMWh)).ToList();
        }

        private static DateTime EnsureUtc(DateTime dt) =>
            dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

        private async Task PersistResultAsync(
            AppDbContext db,
            OptimizationResult result,
            string label,
            OptimizationSettings settings,
            List<HeatPricePoint> market,
            CancellationToken ct)
        {
            DateTime now = DateTime.UtcNow;
            DateTime timeFrom = EnsureUtc(market.First().TimeFrom);
            DateTime timeTo   = EnsureUtc(market.Last().TimeFrom).AddHours(1);

            var run = new OptimizationRun
            {
                CreatedAtUtc = now,
                Objective = settings.Objective.ToString(),
                CostWeight = settings.CostWeight,
                Co2Weight  = settings.Co2Weight,
                ElectricityPriceSource = $"Database ({label})",
                TimeFromUtc = timeFrom,
                TimeToUtc   = timeTo,
                TotalNetCostDkk = result.TotalNetCostDkk,
                TotalCo2Kg = result.TotalCo2Kg,
                TotalElectricityCashflowDkk = result.TotalElectricityCashflowDkk,
                Hours = result.Hours.Select(h => new OptimizationHourResult
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

            db.OptimizationRuns.Add(run);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("[Worker] Persisted run id={id} label='{label}'", run.Id, label);
        }
    }
}
