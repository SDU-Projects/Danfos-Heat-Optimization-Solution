namespace data.AssetManager;

// produces both heat and electricity
// net cost is reduced by revenue from electricity produced
// formula: ProductionCostPerMWh - (ElectricityProducedMW * electricityPrice)
public class GasMotor : ProductionUnit
{
    public double GasConsumption { get; set; }

    public GasMotor()
    {
        ElectricityConsumedMW = 0;
    }

    public override double CalculateNetProductionCost(double electricityPrice)
    {
        return ProductionCostPerMWh - (ElectricityProducedMW * electricityPrice);
    }
}
