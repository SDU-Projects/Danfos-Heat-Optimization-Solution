using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace data.Entities;

public class OptimizationRun
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    [MaxLength(32)]
    public string Objective { get; set; } = string.Empty;

    public decimal CostWeight { get; set; }
    public decimal Co2Weight { get; set; }

    [MaxLength(32)]
    public string ElectricityPriceSource { get; set; } = string.Empty;

    public DateTime TimeFromUtc { get; set; }
    public DateTime TimeToUtc { get; set; }

    public decimal TotalNetCostDkk { get; set; }
    public decimal TotalCo2Kg { get; set; }
    public decimal TotalElectricityCashflowDkk { get; set; }

    public List<OptimizationHourResult> Hours { get; set; } = new();
}

public class OptimizationHourResult
{
    [Key]
    public int Id { get; set; }

    public int OptimizationRunId { get; set; }
    public OptimizationRun OptimizationRun { get; set; } = null!;

    public DateTime TimeFromUtc { get; set; }

    public decimal HeatDemandMWh { get; set; }
    public decimal ElectricityPriceDkkPerMWh { get; set; }

    public decimal HeatSuppliedMWh { get; set; }
    public decimal TotalNetCostDkk { get; set; }
    public decimal TotalCo2Kg { get; set; }
    public decimal ElectricityCashflowDkk { get; set; }

    public List<OptimizationUnitHourResult> UnitResults { get; set; } = new();
}

public class OptimizationUnitHourResult
{
    [Key]
    public int Id { get; set; }

    public int OptimizationHourResultId { get; set; }
    public OptimizationHourResult OptimizationHourResult { get; set; } = null!;

    public int ProductionUnitId { get; set; }

    [MaxLength(64)]
    public string UnitName { get; set; } = string.Empty;

    public decimal HeatProducedMWh { get; set; }
    public decimal ElectricityMWh { get; set; }

    public decimal NetCostDkk { get; set; }
    public decimal Co2Kg { get; set; }
    public decimal ScorePerMWh { get; set; }
}
