namespace data.AssetManager;
public class AssetManager
{
    private const string ImagesFolder = "AssetManager/Images";

    private readonly HeatingGrid _heatingGrid;
    private readonly List<ProductionUnit> _productionUnits;

    public AssetManager()
    {
        _heatingGrid = CreateHeatingGrid();
        _productionUnits = CreateProductionUnits();
    }


    public HeatingGrid GetHeatingGrid()
    {
        return _heatingGrid;
    }


    public List<ProductionUnit> GetProductionUnits()
    {
        return _productionUnits;
    }

    public ProductionUnit? GetProductionUnitByName(string name)
    {
        return _productionUnits.FirstOrDefault(u => u.Name == name);
    }

    public List<ProductionUnit> GetAvailableUnits()
    {
        return _productionUnits.Where(u => u.IsAvailable).ToList();
    }

    public List<ProductionUnit> GetAvailableUnitsAt(DateTime time)
    {
        return _productionUnits
            .Where(u => u.IsAvailable && !u.IsUnderMaintenance(time))
            .ToList();
    }

    // heat grid??

    private static HeatingGrid CreateHeatingGrid()
    {
        return new HeatingGrid
        {
            Name = "Sample Heating Grid",
            ImagePath = string.Empty
        };
    }

    private static List<ProductionUnit> CreateProductionUnits()
    {
        return new List<ProductionUnit>
        {
            new GasBoiler
            {
                Name = "GB1",
                MaxHeatMW = 3.0,
                ProductionCostPerMWh = 510,
                CO2KgPerMWh = 132,
                GasConsumption = 1.05,
                ImagePath = Path.Combine(ImagesFolder, "GB1.png")
            },
            new GasBoiler
            {
                Name = "GB2",
                MaxHeatMW = 2.0,
                ProductionCostPerMWh = 540,
                CO2KgPerMWh = 134,
                GasConsumption = 1.08,
                ImagePath = Path.Combine(ImagesFolder, "GB2.png")
            },
            new GasBoiler
            {
                Name = "GB3",
                MaxHeatMW = 4.0,
                ProductionCostPerMWh = 580,
                CO2KgPerMWh = 136,
                GasConsumption = 1.09,
                ImagePath = Path.Combine(ImagesFolder, "GB3.png")
            },
            new OilBoiler
            {
                Name = "OB1",
                MaxHeatMW = 6.0,
                ProductionCostPerMWh = 690,
                CO2KgPerMWh = 147,
                OilConsumption = 1.18,
                ImagePath = Path.Combine(ImagesFolder, "OB1.jpg")
            },
            new GasMotor
            {
                Name = "GM1",
                MaxHeatMW = 5.3,
                ProductionCostPerMWh = 975,
                CO2KgPerMWh = 227,
                GasConsumption = 1.82,
                ElectricityProducedMW = 3.9,
                ImagePath = Path.Combine(ImagesFolder, "GM1.png")
            },
            new ElectricBoiler
            {
                Name = "EB1",
                MaxHeatMW = 6.0,
                ProductionCostPerMWh = 15,
                CO2KgPerMWh = 0,
                ElectricityConsumedMW = 6.0,
                ImagePath = Path.Combine(ImagesFolder, "EB1.jpg")
            }
        };
    }
}
