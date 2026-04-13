using System.Text.Json;
using api.Models;
using data;
using data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeatMarketController : ControllerBase
{
    private const string EnergiDataServiceUrl = "https://api.energidataservice.dk/dataset/Elspotprices?limit=1&sort=HourUTC%20DESC&filter={\"PriceArea\":[\"DK1\"]}";
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public HeatMarketController(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HeatMarketPointDto>>> GetAll()
    {
        List<HeatPricePoint> rows = await _context.HeatPricePoints
            .AsNoTracking()
            .OrderBy(x => x.TimeFrom)
            .ToListAsync();

        List<HeatMarketPointDto> response = rows
            .Select(MapEntityToDto)
            .ToList();

        return Ok(response);
    }

    [HttpGet("current")]
    public async Task<ActionResult<HeatMarketPointDto>> GetCurrent()
    {
        DateTime timeFromUtc = TruncateToHour(DateTime.UtcNow);
        DateTime timeToUtc = timeFromUtc.AddHours(1);

        decimal electricityPrice = await GetCurrentElectricityPriceAsync();
        decimal heatDemandKwh = GetRandomHeatDemandKwh();

        HeatMarketPointDto point = new HeatMarketPointDto
        {
            Season = ResolveSeason(timeFromUtc),
            TimeFromUtc = timeFromUtc,
            TimeToUtc = timeToUtc,
            HeatDemandKWh = heatDemandKwh,
            ElectricityPriceDkkPerMWh = electricityPrice
        };

        return Ok(point);
    }

    [HttpPost("current")]
    public async Task<ActionResult<HeatMarketPointDto>> SaveCurrent([FromBody] SaveHeatMarketPointRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        if (request.TimeToUtc <= request.TimeFromUtc)
        {
            return BadRequest("TimeToUtc must be after TimeFromUtc.");
        }

        if (request.HeatDemandKWh < 5m || request.HeatDemandKWh > 15m)
        {
            return BadRequest("HeatDemandKWh must be between 5 and 15.");
        }

        if (request.ElectricityPriceDkkPerMWh < 0m)
        {
            return BadRequest("ElectricityPriceDkkPerMWh must be non-negative.");
        }

        DateTime normalizedFrom = TruncateToHour(request.TimeFromUtc.ToUniversalTime());
        DateTime normalizedTo = TruncateToHour(request.TimeToUtc.ToUniversalTime());

        string season = string.IsNullOrWhiteSpace(request.Season)
            ? ResolveSeason(normalizedFrom)
            : request.Season.Trim();

        HeatPricePoint entity = new HeatPricePoint
        {
            Season = season,
            TimeFrom = normalizedFrom,
            TimeTo = normalizedTo,
            HeatDemandMWh = request.HeatDemandKWh / 1000m,
            ElectricityPriceDkkPerMWh = request.ElectricityPriceDkkPerMWh
        };

        _context.HeatPricePoints.Add(entity);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("A point already exists for the same season and time interval.");
        }

        HeatMarketPointDto response = MapEntityToDto(entity);

        return CreatedAtAction(nameof(GetCurrent), response);
    }

    private static HeatMarketPointDto MapEntityToDto(HeatPricePoint entity)
    {
        return new HeatMarketPointDto
        {
            Season = entity.Season,
            TimeFromUtc = entity.TimeFrom,
            TimeToUtc = entity.TimeTo,
            HeatDemandKWh = decimal.Round(entity.HeatDemandMWh * 1000m, 2),
            ElectricityPriceDkkPerMWh = entity.ElectricityPriceDkkPerMWh
        };
    }

    private async Task<decimal> GetCurrentElectricityPriceAsync()
    {
        HttpClient client = _httpClientFactory.CreateClient();

        try
        {
            using HttpResponseMessage response = await client.GetAsync(EnergiDataServiceUrl);
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            using JsonDocument document = await JsonDocument.ParseAsync(stream);

            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("records", out JsonElement records) &&
                !root.TryGetProperty("recors", out records))
            {
                return await GetLatestKnownOrFallbackPriceAsync();
            }

            if (records.ValueKind != JsonValueKind.Array || records.GetArrayLength() == 0)
            {
                return await GetLatestKnownOrFallbackPriceAsync();
            }

            JsonElement firstRecord = records[0];
            if (!firstRecord.TryGetProperty("SpotPriceDKK", out JsonElement priceElement))
            {
                return await GetLatestKnownOrFallbackPriceAsync();
            }

            if (priceElement.TryGetDecimal(out decimal decimalPrice))
            {
                return decimalPrice;
            }

            if (priceElement.TryGetDouble(out double doublePrice))
            {
                return Convert.ToDecimal(doublePrice);
            }
        }
        catch
        {
            return await GetLatestKnownOrFallbackPriceAsync();
        }

        return await GetLatestKnownOrFallbackPriceAsync();
    }

    private async Task<decimal> GetLatestKnownOrFallbackPriceAsync()
    {
        decimal? latest = await _context.HeatPricePoints
            .AsNoTracking()
            .OrderByDescending(x => x.TimeFrom)
            .Select(x => (decimal?)x.ElectricityPriceDkkPerMWh)
            .FirstOrDefaultAsync();

        if (latest.HasValue)
        {
            return latest.Value;
        }

        int randomWholePrice = Random.Shared.Next(200, 2001);
        return randomWholePrice;
    }

    private static decimal GetRandomHeatDemandKwh()
    {
        double value = Random.Shared.NextDouble() * 10d + 5d;
        return decimal.Round(Convert.ToDecimal(value), 2);
    }

    private static DateTime TruncateToHour(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, DateTimeKind.Utc);
    }

    private static string ResolveSeason(DateTime utcDate)
    {
        int month = utcDate.Month;

        if (month == 12 || month == 1 || month == 2)
        {
            return "Winter";
        }

        if (month >= 3 && month <= 5)
        {
            return "Spring";
        }

        if (month >= 6 && month <= 8)
        {
            return "Summer";
        }

        return "Autumn";
    }
}