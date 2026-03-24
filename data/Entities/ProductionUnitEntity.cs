using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace data.Entities;

public class ProductionUnitEntity
{
    [Key]
    public int Id { get; set; }

    public ProductionUnitType Type { get; set; }

    [Column(TypeName = "jsonb")]
    public string DataJson { get; set; } = "{}";

    public bool OnMaintenance { get; set; } = false;
    public bool IsConnectedToGrid { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
}
