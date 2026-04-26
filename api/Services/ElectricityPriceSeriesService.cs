using System.Text.Json;

namespace api.Services;

public sealed class ElectricityPriceSeriesService(IHttpClientFactory httpClientFactory)
{
    private const string DatasetUrl = "https://api.energidataservice.dk/dataset/Elspotprices";

    public async Task<Dictionary<DateTime, decimal>> GetPricesAsync(DateTime timeFromUtcInclusive, DateTime timeToUtcExclusive, CancellationToken cancellationToken)
    {
        if (timeFromUtcInclusive.Kind != DateTimeKind.Utc || timeToUtcExclusive.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.");
        }

        if (timeToUtcExclusive <= timeFromUtcInclusive)
        {
            throw new ArgumentException("TimeTo must be after TimeFrom.");
        }

        // EnergiDataService uses HourUTC. We query a bit broadly and then align on exact hours.
        string start = Uri.EscapeDataString(timeFromUtcInclusive.ToString("yyyy-MM-ddTHH:mm"));
        string end = Uri.EscapeDataString(timeToUtcExclusive.ToString("yyyy-MM-ddTHH:mm"));
        string filter = Uri.EscapeDataString("{\"PriceArea\":[\"DK1\"]}");

        string url = $"{DatasetUrl}?start={start}&end={end}&sort=HourUTC%20ASC&filter={filter}&limit=5000";

        HttpClient client = httpClientFactory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("records", out JsonElement records) || records.ValueKind != JsonValueKind.Array)
        {
            return new Dictionary<DateTime, decimal>();
        }

        Dictionary<DateTime, decimal> result = new();

        foreach (JsonElement record in records.EnumerateArray())
        {
            if (!record.TryGetProperty("HourUTC", out JsonElement hourElement))
            {
                continue;
            }

            if (!record.TryGetProperty("SpotPriceDKK", out JsonElement priceElement))
            {
                continue;
            }

            if (!DateTime.TryParse(hourElement.GetString(), out DateTime hourUtc))
            {
                continue;
            }

            hourUtc = DateTime.SpecifyKind(hourUtc, DateTimeKind.Utc);

            if (hourUtc < timeFromUtcInclusive || hourUtc >= timeToUtcExclusive)
            {
                continue;
            }

            if (priceElement.TryGetDecimal(out decimal price))
            {
                result[hourUtc] = price;
                continue;
            }

            if (priceElement.TryGetDouble(out double priceDouble))
            {
                result[hourUtc] = Convert.ToDecimal(priceDouble);
            }
        }

        return result;
    }
}
