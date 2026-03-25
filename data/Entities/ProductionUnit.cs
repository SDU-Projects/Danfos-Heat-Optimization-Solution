using System.ComponentModel.DataAnnotations;
using data.Models.Base;

namespace data.Entities;

public class ProductionUnit
{
    [Key]
    public int Id { get; set; }
    public ProductionUnitBase Data { get; set; } = null!;
    public ProductionUnitType Type { get; set; }
}

public enum ProductionUnitType
{
    ElectricBoiler = 0,
    GasBoiler = 1,
    GasMotor = 2,
    OilBoiler = 3,
}
