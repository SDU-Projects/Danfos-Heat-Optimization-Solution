using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace desktop.app.Services;

public class ChatTurn
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class AiChatRequest
{
    [JsonPropertyName("messages")]
    public List<ChatTurn> Messages { get; set; } = new();
}

public class UnitDataDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("max_heat_mw")]
    public double MaxHeatMW { get; set; }

    [JsonPropertyName("production_cost_per_mwh")]
    public double ProductionCostPerMWh { get; set; }

    [JsonPropertyName("co2_kg_per_mwh")]
    public double CO2KgPerMWh { get; set; }

    [JsonPropertyName("electricity_produced_mw")]
    public double ElectricityProducedMW { get; set; }

    [JsonPropertyName("electricity_consumed_mw")]
    public double ElectricityConsumedMW { get; set; }

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;
}

public class AiChatResponse
{
    [JsonPropertyName("reply")]
    public string Reply { get; set; } = string.Empty;

    [JsonPropertyName("unit_data")]
    public UnitDataDto? UnitData { get; set; }
}

public class AiChatService
{
    private const string BaseUrl = "http://localhost:8000";

    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromSeconds(60)
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _http.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AiChatResponse> SendAsync(List<ChatTurn> history)
    {
        var request = new AiChatRequest { Messages = history };
        var response = await _http.PostAsJsonAsync("/chat", request, _jsonOptions);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AiChatResponse>(_jsonOptions);
        return result ?? new AiChatResponse { Reply = "(empty response)" };
    }
}
