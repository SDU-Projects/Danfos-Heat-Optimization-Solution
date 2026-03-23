namespace data.AssetManager;

// produces heat only, doesnt produce or consume electricity
// uses natural gas as fuel, net cost is independent of electricity price
public class GasBoiler : ProductionUnit
{
    public double GasConsumption { get; set; }

    public GasBoiler()
    {
   ElectricityProducedMW = 0;
        ElectricityConsumedMW = 0;
    }

    public override double CalculateNetProductionCost(double electricityPrice)
    {
     return ProductionCostPerMWh;
}
}
