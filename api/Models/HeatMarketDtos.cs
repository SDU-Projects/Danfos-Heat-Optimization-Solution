// Heat Market Data Transfer Objects

namespace api.Models;

public class HeatMarketPointDto
{
    public string Season { get; set; } = string.Empty;
    public DateTime TimeFromUtc { get; set; }
    public DateTime TimeToUtc { get; set; }
    public decimal HeatDemandKWh { get; set; }
    public decimal ElectricityPriceDkkPerMWh { get; set; }
}

public class SaveHeatMarketPointRequest
{
    public string? Season { get; set; }
    public DateTime TimeFromUtc { get; set; }
    public DateTime TimeToUtc { get; set; }
    public decimal HeatDemandKWh { get; set; }
    public decimal ElectricityPriceDkkPerMWh { get; set; }
}