namespace data.AssetManager;

// consumes electricity to produce heat
// formula: ProductionCostPerMWh + (ElectricityConsumedMW * electricityPrice)
public class ElectricBoiler : ProductionUnit
{
    public ElectricBoiler()
 {
        ElectricityProducedMW = 0;
    }

    public override double CalculateNetProductionCost(double electricityPrice)
    {
        return ProductionCostPerMWh + (ElectricityConsumedMW * electricityPrice);
    }
}
