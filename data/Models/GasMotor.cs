using data.Models.Base;

namespace data.Models;

// produces both heat and electricity
// net cost is reduced by revenue from electricity produced (per MWh heat)
public class GasMotor : ProductionUnitBase
{
    public double GasConsumption { get; set; }

    public GasMotor()
    {
        ElectricityConsumedMW = 0;
    }

    public override double CalculateNetProductionCost(double electricityPrice)
    {
        if (MaxHeatMW <= 0)
        {
            return ProductionCostPerMWh;
        }

        double electricityMWhPerMWhHeat = ElectricityProducedMW / MaxHeatMW;
        return ProductionCostPerMWh - (electricityMWhPerMWhHeat * electricityPrice);
    }
}
