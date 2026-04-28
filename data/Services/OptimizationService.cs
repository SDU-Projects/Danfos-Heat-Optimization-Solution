using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace data.Services;

// ── DTOs (mirrors api.Models) ──────────────────────────────────────────────

public sealed class OptimizationRunRequest
{
    public DateTime? TimeFromUtc { get; set; }
    public DateTime? TimeToUtc { get; set; }
    public string? Season { get; set; }
    public List<int>? EnabledProductionUnitIds { get; set; }
    public List<MaintenanceWindowRequest>? Maintenance { get; set; }
    public int Objective { get; set; } = 0; // 0=Cost
    public decimal CostWeight { get; set; } = 1m;
    public decimal Co2Weight { get; set; } = 0m;
    public int ElectricityPriceSource { get; set; } = 0; // 0=Database
}

public sealed class MaintenanceWindowRequest
{
    public int ProductionUnitId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}

public sealed class OptimizationUnitHourResultDto
{
    public int ProductionUnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal HeatProducedMWh { get; set; }
    public decimal ElectricityMWh { get; set; }
    public decimal NetCostDkk { get; set; }
    public decimal Co2Kg { get; set; }
    public decimal ScorePerMWh { get; set; }
}

public sealed class OptimizationHourResultDto
{
    public DateTime TimeFromUtc { get; set; }
    public decimal HeatDemandMWh { get; set; }
    public decimal ElectricityPriceDkkPerMWh { get; set; }
    public decimal HeatSuppliedMWh { get; set; }
    public decimal TotalNetCostDkk { get; set; }
    public decimal TotalCo2Kg { get; set; }
    public decimal ElectricityCashflowDkk { get; set; }
    public List<OptimizationUnitHourResultDto> UnitResults { get; set; } = new();
}

public sealed class OptimizationRunDto
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Objective { get; set; } = string.Empty;
    public decimal CostWeight { get; set; }
    public decimal Co2Weight { get; set; }
    public string ElectricityPriceSource { get; set; } = string.Empty;
    public DateTime TimeFromUtc { get; set; }
    public DateTime TimeToUtc { get; set; }
    public decimal TotalNetCostDkk { get; set; }
    public decimal TotalCo2Kg { get; set; }
    public decimal TotalElectricityCashflowDkk { get; set; }
    public List<OptimizationHourResultDto> Hours { get; set; } = new();
}

// ── Service ────────────────────────────────────────────────────────────────

public class OptimizationService
{
    private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:5113";

    public OptimizationService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
    }

    public async Task<OptimizationRunDto> RunOptimizationAsync(OptimizationRunRequest request)
    {
        string json = JsonSerializer.Serialize(request, JsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/optimization/run", content);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Optimization failed ({response.StatusCode}): {body}");
        }

        string resultJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OptimizationRunDto>(resultJson, JsonOpts)
               ?? throw new InvalidOperationException("Empty response from optimization endpoint.");
    }

    public async Task<List<OptimizationRunDto>> GetRunsAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/optimization/runs");
        if (!response.IsSuccessStatusCode) return new List<OptimizationRunDto>();
        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<OptimizationRunDto>>(json, JsonOpts) ?? new List<OptimizationRunDto>();
    }

    public async Task<OptimizationRunDto?> GetRunAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/optimization/runs/{id}");
        if (!response.IsSuccessStatusCode) return null;
        string resultJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OptimizationRunDto>(resultJson, JsonOpts);
    }
}
