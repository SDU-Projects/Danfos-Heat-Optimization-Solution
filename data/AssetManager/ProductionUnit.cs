namespace data.AssetManager;
public abstract class ProductionUnit
{
    public string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public double MaxHeatMW { get; set; }
    public double ProductionCostPerMWh { get; set; }
    public double CO2KgPerMWh { get; set; }
    public double ElectricityProducedMW { get; set; }
    public double ElectricityConsumedMW { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime? MaintenanceStart { get; set; }
    public DateTime? MaintenanceEnd { get; set; }

    public bool IsUnderMaintenance(DateTime time)
    {
        if (MaintenanceStart.HasValue && MaintenanceEnd.HasValue)
        {
     return time >= MaintenanceStart.Value && time <= MaintenanceEnd.Value;
        }

        return false;
    }

    // each subclass overrides this to reflect its specific cost structure
    public abstract double CalculateNetProductionCost(double electricityPrice);
}
