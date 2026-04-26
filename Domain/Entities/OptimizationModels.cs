using data.Models.Base;

namespace Domain.Entities;

public enum OptimizationObjective
{
    Cost = 0,
    Co2 = 1,
    Hybrid = 2
}

public enum ElectricityPriceSource
{
    Database = 0,
    Api = 1
}

public sealed record MarketPoint(
    DateTime TimeFromUtc,
    decimal HeatDemandMWh,
    decimal ElectricityPriceDkkPerMWh);

public sealed record MaintenanceWindow(
    int ProductionUnitId,
    DateTime StartUtc,
    DateTime EndUtc);

public sealed record OptimizationSettings(
    OptimizationObjective Objective = OptimizationObjective.Cost,
    decimal CostWeight = 1m,
    decimal Co2Weight = 0m);

public sealed record OptimizationInput(
    IReadOnlyList<MarketPoint> Market,
    IReadOnlyList<(int Id, ProductionUnitBase Unit, bool Enabled)> Units,
    IReadOnlyList<MaintenanceWindow>? Maintenance = null,
    OptimizationSettings? Settings = null);

public sealed record UnitDispatchResult(
    int ProductionUnitId,
    string UnitName,
    decimal HeatProducedMWh,
    decimal ElectricityMWh,
    decimal NetCostDkk,
    decimal Co2Kg,
    decimal ScorePerMWh);

public sealed record HourDispatchResult(
    DateTime TimeFromUtc,
    decimal HeatDemandMWh,
    decimal ElectricityPriceDkkPerMWh,
    decimal HeatSuppliedMWh,
    decimal TotalNetCostDkk,
    decimal TotalCo2Kg,
    decimal ElectricityCashflowDkk,
    IReadOnlyList<UnitDispatchResult> UnitResults);

public sealed record OptimizationResult(
    OptimizationSettings Settings,
    decimal TotalNetCostDkk,
    decimal TotalCo2Kg,
    decimal TotalElectricityCashflowDkk,
    IReadOnlyList<HourDispatchResult> Hours);

public sealed class InfeasibleSolutionException(string message) : Exception(message);

public sealed class TimeSeriesAlignmentException(string message) : Exception(message);
