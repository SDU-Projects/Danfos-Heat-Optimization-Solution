using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using data.Entities;

namespace data.Services;

public class AssetService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:5113/api/asset";

    public AssetService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<ProductionUnit>> GetAllAssetsAsync()
    {
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();

        var text = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<ProductionUnit>>(text, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        return result ?? new List<ProductionUnit>();
    }

    public async Task<ProductionUnit?> CreateAssetAsync(ProductionUnit asset)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        JsonObject requestPayload = asset.Data switch
        {
            data.Models.GasBoiler gb => new JsonObject
            {
                ["data"] = new JsonObject
                {
                    ["$type"] = "GasBoiler",
                    ["name"] = gb.Name,
                    ["imageUrl"] = gb.ImageUrl,
                    ["maxHeatMW"] = gb.MaxHeatMW,
                    ["productionCostPerMWh"] = gb.ProductionCostPerMWh,
                    ["cO2KgPerMWh"] = gb.CO2KgPerMWh,
                    ["electricityProducedMW"] = gb.ElectricityProducedMW,
                    ["electricityConsumedMW"] = gb.ElectricityConsumedMW,
                    ["isAvailable"] = gb.IsAvailable,
                    ["onMaintainance"] = gb.OnMaintainance,
                    ["isConnectedToGrid"] = gb.IsConnectedToGrid,
                    ["gasConsumption"] = gb.GasConsumption
                },
                ["type"] = JsonValue.Create(asset.Type)
            },
            data.Models.GasMotor gm => new JsonObject
            {
                ["data"] = new JsonObject
                {
                    ["$type"] = "GasMotor",
                    ["name"] = gm.Name,
                    ["imageUrl"] = gm.ImageUrl,
                    ["maxHeatMW"] = gm.MaxHeatMW,
                    ["productionCostPerMWh"] = gm.ProductionCostPerMWh,
                    ["cO2KgPerMWh"] = gm.CO2KgPerMWh,
                    ["electricityProducedMW"] = gm.ElectricityProducedMW,
                    ["electricityConsumedMW"] = gm.ElectricityConsumedMW,
                    ["isAvailable"] = gm.IsAvailable,
                    ["onMaintainance"] = gm.OnMaintainance,
                    ["isConnectedToGrid"] = gm.IsConnectedToGrid,
                    ["gasConsumption"] = gm.GasConsumption
                },
                ["type"] = JsonValue.Create(asset.Type)
            },
            data.Models.OilBoiler ob => new JsonObject
            {
                ["data"] = new JsonObject
                {
                    ["$type"] = "OilBoiler",
                    ["name"] = ob.Name,
                    ["imageUrl"] = ob.ImageUrl,
                    ["maxHeatMW"] = ob.MaxHeatMW,
                    ["productionCostPerMWh"] = ob.ProductionCostPerMWh,
                    ["cO2KgPerMWh"] = ob.CO2KgPerMWh,
                    ["electricityProducedMW"] = ob.ElectricityProducedMW,
                    ["electricityConsumedMW"] = ob.ElectricityConsumedMW,
                    ["isAvailable"] = ob.IsAvailable,
                    ["onMaintainance"] = ob.OnMaintainance,
                    ["isConnectedToGrid"] = ob.IsConnectedToGrid,
                    ["oilConsumption"] = ob.OilConsumption
                },
                ["type"] = JsonValue.Create(asset.Type)
            },
            data.Models.ElectricBoiler eb => new JsonObject
            {
                ["data"] = new JsonObject
                {
                    ["$type"] = "ElectricBoiler",
                    ["name"] = eb.Name,
                    ["imageUrl"] = eb.ImageUrl,
                    ["maxHeatMW"] = eb.MaxHeatMW,
                    ["productionCostPerMWh"] = eb.ProductionCostPerMWh,
                    ["cO2KgPerMWh"] = eb.CO2KgPerMWh,
                    ["electricityProducedMW"] = eb.ElectricityProducedMW,
                    ["electricityConsumedMW"] = eb.ElectricityConsumedMW,
                    ["isAvailable"] = eb.IsAvailable,
                    ["onMaintainance"] = eb.OnMaintainance,
                    ["isConnectedToGrid"] = eb.IsConnectedToGrid
                },
                ["type"] = JsonValue.Create(asset.Type)
            },
            _ => new JsonObject
            {
                ["data"] = new JsonObject
                {
                    ["$type"] = asset.Data.GetType().Name,
                    ["name"] = asset.Data.Name,
                    ["imageUrl"] = asset.Data.ImageUrl,
                    ["maxHeatMW"] = asset.Data.MaxHeatMW,
                    ["productionCostPerMWh"] = asset.Data.ProductionCostPerMWh,
                    ["cO2KgPerMWh"] = asset.Data.CO2KgPerMWh,
                    ["electricityProducedMW"] = asset.Data.ElectricityProducedMW,
                    ["electricityConsumedMW"] = asset.Data.ElectricityConsumedMW,
                    ["isAvailable"] = asset.Data.IsAvailable,
                    ["onMaintainance"] = asset.Data.OnMaintainance,
                    ["isConnectedToGrid"] = asset.Data.IsConnectedToGrid
                },
                ["type"] = JsonValue.Create(asset.Type)
            }
        };

        var json = requestPayload.ToJsonString(options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_baseUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode} ({response.ReasonPhrase}). Body: {errorBody}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProductionUnit>(responseContent, options);
    }

    public async Task<bool> UpdateAssetAsync(int id, ProductionUnit asset)
    {
        var json = JsonSerializer.Serialize(asset, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        var response = await _httpClient.PutAsync($"{_baseUrl}/{id}", new StringContent(json, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAssetAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}
