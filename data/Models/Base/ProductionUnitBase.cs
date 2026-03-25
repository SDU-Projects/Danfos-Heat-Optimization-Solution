using System.Text.Json.Serialization;
using data.Models;

namespace data.Models.Base;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ElectricBoiler), "ElectricBoiler")]
[JsonDerivedType(typeof(GasBoiler), "GasBoiler")]
[JsonDerivedType(typeof(GasMotor), "GasMotor")]
[JsonDerivedType(typeof(OilBoiler), "OilBoiler")]
public class ProductionUnitBase
{
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public double MaxHeatMW { get; set; }
    public double ProductionCostPerMWh { get; set; }
    public double CO2KgPerMWh { get; set; }
    public double ElectricityProducedMW { get; set; }
    public double ElectricityConsumedMW { get; set; }
    public bool IsAvailable { get; set; }
    public bool OnMaintainance { get; set; }
    public bool IsConnectedToGrid { get; set; }

    public virtual bool IsUnderMaintenance(DateTime time) => OnMaintainance;
    public virtual double CalculateNetProductionCost(double electricityPrice) => ProductionCostPerMWh;
}
