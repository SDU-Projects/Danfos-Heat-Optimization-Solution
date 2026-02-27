using System.ComponentModel.DataAnnotations;

namespace data.Entities;

public class Sample
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } =  string.Empty;
}
