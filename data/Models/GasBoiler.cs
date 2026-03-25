using data.Models.Base;

namespace data.Models;

// produces heat only, doesnt produce or consume electricity
// uses natural gas as fuel, net cost is independent of electricity price
public class GasBoiler : ProductionUnitBase
{
    public double GasConsumption { get; set; }

    public override double CalculateNetProductionCost(double electricityPrice)
    {
     return ProductionCostPerMWh;
    }
}
