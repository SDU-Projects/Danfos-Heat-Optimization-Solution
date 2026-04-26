namespace api.Models;

public enum OptimizationObjectiveDto
{
    Cost = 0,
    Co2 = 1,
    Hybrid = 2
}

public enum ElectricityPriceSourceDto
{
    Database = 0,
    Api = 1
}

public sealed class MaintenanceWindowDto
{
    public int ProductionUnitId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}

public sealed class OptimizationRunRequest
{
    public DateTime? TimeFromUtc { get; set; }
    public DateTime? TimeToUtc { get; set; }
    public string? Season { get; set; }

    public List<int>? EnabledProductionUnitIds { get; set; }

    public List<MaintenanceWindowDto>? Maintenance { get; set; }

    public OptimizationObjectiveDto Objective { get; set; } = OptimizationObjectiveDto.Cost;

    public decimal CostWeight { get; set; } = 1m;
    public decimal Co2Weight { get; set; } = 0m;

    public ElectricityPriceSourceDto ElectricityPriceSource { get; set; } = ElectricityPriceSourceDto.Database;
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
