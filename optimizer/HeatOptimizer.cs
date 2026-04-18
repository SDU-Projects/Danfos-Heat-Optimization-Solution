using data.Models.Base;

namespace optimizer;

public sealed class HeatOptimizer
{
    private const decimal HeatEpsilon = 0.0001m;

    public OptimizationResult Optimize(OptimizationInput input)
    {
        if (input.Market == null || input.Market.Count == 0)
        {
            throw new ArgumentException("Market time series is required.", nameof(input));
        }

        if (input.Units == null || input.Units.Count == 0)
        {
            throw new ArgumentException("At least one production unit is required.", nameof(input));
        }

        ValidateAlignedHourlySeries(input.Market);

        OptimizationSettings settings = input.Settings ?? new OptimizationSettings();

        var maintenance = input.Maintenance ?? Array.Empty<MaintenanceWindow>();

        List<HourDispatchResult> hours = new(input.Market.Count);
        decimal totalCost = 0m;
        decimal totalCo2 = 0m;
        decimal totalElectricityCashflow = 0m;

        foreach (MarketPoint point in input.Market)
        {
            decimal demand = point.HeatDemandMWh;
            decimal price = point.ElectricityPriceDkkPerMWh;

            if (demand <= HeatEpsilon)
            {
                hours.Add(new HourDispatchResult(
                    point.TimeFromUtc,
                    point.HeatDemandMWh,
                    price,
                    0m,
                    0m,
                    0m,
                    0m,
                    Array.Empty<UnitDispatchResult>()));
                continue;
            }

            var availableUnits = input.Units
                .Where(u => u.Enabled && u.Unit.IsAvailable)
                .Where(u => !u.Unit.IsUnderMaintenance(point.TimeFromUtc))
                .Where(u => !IsInMaintenance(u.Id, point.TimeFromUtc, maintenance))
                .ToList();

            decimal totalCapacity = availableUnits.Sum(u => Convert.ToDecimal(Math.Max(0d, u.Unit.MaxHeatMW)));
            if (totalCapacity + HeatEpsilon < demand)
            {
                throw new InfeasibleSolutionException($"Infeasible at {point.TimeFromUtc:O}. Demand={demand} MWh, total available capacity={totalCapacity} MWh.");
            }

            var scored = availableUnits
                .Select(u => new
                {
                    u.Id,
                    u.Unit,
                    Score = ComputeScorePerMWh(u.Unit, price, settings)
                })
                .OrderBy(x => x.Score)
                .ThenBy(x => x.Unit.Name, StringComparer.Ordinal)
                .ToList();

            decimal remaining = demand;
            List<UnitDispatchResult> unitResults = new(scored.Count);

            foreach (var s in scored)
            {
                if (remaining <= HeatEpsilon)
                {
                    break;
                }

                decimal maxHeat = Convert.ToDecimal(Math.Max(0d, s.Unit.MaxHeatMW));
                if (maxHeat <= HeatEpsilon)
                {
                    continue;
                }

                decimal heatProduced = Math.Min(maxHeat, remaining);
                if (heatProduced <= HeatEpsilon)
                {
                    continue;
                }

                decimal electricityMWh = CalculateElectricityMWhForHeat(s.Unit, heatProduced);
                decimal netCostPerMWh = Convert.ToDecimal(s.Unit.CalculateNetProductionCost((double)price));
                decimal netCost = heatProduced * netCostPerMWh;
                decimal co2 = heatProduced * Convert.ToDecimal(s.Unit.CO2KgPerMWh);

                unitResults.Add(new UnitDispatchResult(
                    s.Id,
                    s.Unit.Name,
                    heatProduced,
                    electricityMWh,
                    netCost,
                    co2,
                    s.Score));

                remaining -= heatProduced;
            }

            if (remaining > HeatEpsilon)
            {
                throw new InfeasibleSolutionException($"Infeasible at {point.TimeFromUtc:O}. Uncovered demand={remaining} MWh.");
            }

            decimal supplied = unitResults.Sum(x => x.HeatProducedMWh);
            decimal hourCost = unitResults.Sum(x => x.NetCostDkk);
            decimal hourCo2 = unitResults.Sum(x => x.Co2Kg);
            decimal hourElectricityCashflow = unitResults.Sum(x => x.ElectricityMWh * price);

            hours.Add(new HourDispatchResult(
                point.TimeFromUtc,
                demand,
                price,
                supplied,
                hourCost,
                hourCo2,
                hourElectricityCashflow,
                unitResults));

            totalCost += hourCost;
            totalCo2 += hourCo2;
            totalElectricityCashflow += hourElectricityCashflow;
        }

        return new OptimizationResult(settings, totalCost, totalCo2, totalElectricityCashflow, hours);
    }

    private static bool IsInMaintenance(int unitId, DateTime hourUtc, IReadOnlyList<MaintenanceWindow> maintenance)
    {
        for (int i = 0; i < maintenance.Count; i++)
        {
            MaintenanceWindow window = maintenance[i];
            if (window.ProductionUnitId != unitId)
            {
                continue;
            }

            if (hourUtc >= window.StartUtc && hourUtc < window.EndUtc)
            {
                return true;
            }
        }

        return false;
    }

    private static decimal ComputeScorePerMWh(ProductionUnitBase unit, decimal electricityPrice, OptimizationSettings settings)
    {
        switch (settings.Objective)
        {
            case OptimizationObjective.Cost:
                return Convert.ToDecimal(unit.CalculateNetProductionCost((double)electricityPrice));

            case OptimizationObjective.Co2:
                return Convert.ToDecimal(unit.CO2KgPerMWh);

            case OptimizationObjective.Hybrid:
                decimal cost = Convert.ToDecimal(unit.CalculateNetProductionCost((double)electricityPrice));
                decimal co2 = Convert.ToDecimal(unit.CO2KgPerMWh);
                return (settings.CostWeight * cost) + (settings.Co2Weight * co2);

            default:
                return Convert.ToDecimal(unit.CalculateNetProductionCost((double)electricityPrice));
        }
    }

    private static decimal CalculateElectricityMWhForHeat(ProductionUnitBase unit, decimal heatProducedMWh)
    {
        decimal maxHeat = Convert.ToDecimal(Math.Max(0d, unit.MaxHeatMW));
        if (maxHeat <= 0m)
        {
            return 0m;
        }

        decimal fraction = heatProducedMWh / maxHeat;
        if (fraction <= 0m)
        {
            return 0m;
        }

        if (fraction > 1m)
        {
            fraction = 1m;
        }

        decimal produced = fraction * Convert.ToDecimal(unit.ElectricityProducedMW);
        decimal consumed = fraction * Convert.ToDecimal(unit.ElectricityConsumedMW);
        return produced - consumed;
    }

    private static void ValidateAlignedHourlySeries(IReadOnlyList<MarketPoint> market)
    {
        for (int i = 0; i < market.Count; i++)
        {
            if (market[i].TimeFromUtc.Kind != DateTimeKind.Utc)
            {
                throw new TimeSeriesAlignmentException("All timestamps must be UTC.");
            }
        }

        for (int i = 1; i < market.Count; i++)
        {
            DateTime prev = market[i - 1].TimeFromUtc;
            DateTime cur = market[i].TimeFromUtc;

            if (cur <= prev)
            {
                throw new TimeSeriesAlignmentException("Timestamps must be strictly increasing.");
            }

            if (cur - prev != TimeSpan.FromHours(1))
            {
                throw new TimeSeriesAlignmentException($"Timestamps must be hourly and consecutive. Gap between {prev:O} and {cur:O}.");
            }
        }
    }
}
