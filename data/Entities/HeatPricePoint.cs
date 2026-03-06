using System.ComponentModel.DataAnnotations;

namespace data.Entities;

// I use this model to store one heat/electricity datapoint for a specific season and hour.
public class HeatPricePoint
{
    [Key]
    public int Id { get; set; }

    [MaxLength(16)]
    public string Season { get; set; } = string.Empty;

    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }

    public decimal HeatDemandMWh { get; set; }
    public decimal ElectricityPriceDkkPerMWh { get; set; }
}
