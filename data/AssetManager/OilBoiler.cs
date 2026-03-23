namespace data.AssetManager;
// produces heat only
public class OilBoiler : ProductionUnit
{
    public double OilConsumption { get; set; }

    public OilBoiler()
 {
        ElectricityProducedMW = 0;
        ElectricityConsumedMW = 0;
    }

    public override double CalculateNetProductionCost(double electricityPrice)
    {
        return ProductionCostPerMWh;
    }
}
