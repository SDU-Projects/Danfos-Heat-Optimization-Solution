using data.Models.Base;

namespace data.Models;

// consumes electricity to produce heat
// formula: ProductionCostPerMWh + (ElectricityConsumedMW * electricityPrice)
public class ElectricBoiler : ProductionUnitBase
{
    public override double CalculateNetProductionCost(double electricityPrice)
    {
        if (MaxHeatMW <= 0)
        {
            return ProductionCostPerMWh;
        }

        double electricityMWhPerMWhHeat = ElectricityConsumedMW / MaxHeatMW;
        return ProductionCostPerMWh + (electricityMWhPerMWhHeat * electricityPrice);
    }
}
