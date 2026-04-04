using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
        var json = JsonSerializer.Serialize(asset, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_baseUrl, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProductionUnit>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });
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
