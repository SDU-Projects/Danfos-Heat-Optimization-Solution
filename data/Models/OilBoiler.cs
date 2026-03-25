using data.Models.Base;

namespace data.Models;
// produces heat only
public class OilBoiler : ProductionUnitBase
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
