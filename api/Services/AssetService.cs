using data.Models;
using data.Models.Base;

namespace data.Services;
public class AssetService
{
    private const string ImagesFolder = "AssetManager/Images";

    private readonly HeatingGrid _heatingGrid;
    private readonly List<ProductionUnitBase> _productionUnits;

    public AssetService()
    {
        _heatingGrid = CreateHeatingGrid();
        _productionUnits = CreateProductionUnits();
    }


    public HeatingGrid GetHeatingGrid()
    {
        return _heatingGrid;
    }


    public List<ProductionUnitBase> GetProductionUnits()
    {
        return _productionUnits;
    }

    public ProductionUnitBase? GetProductionUnitByName(string name)
    {
        return _productionUnits.FirstOrDefault(u => u.Name == name);
    }

    public List<ProductionUnitBase> GetAvailableUnits()
    {
        return _productionUnits.Where(u => u.IsAvailable).ToList();
    }

    public List<ProductionUnitBase> GetAvailableUnitsAt(DateTime time)
    {
        return _productionUnits
            .Where(u => u.IsAvailable && !u.IsUnderMaintenance(time))
            .ToList();
    }

    private static HeatingGrid CreateHeatingGrid()
    {
        return new HeatingGrid
        {
            Name = "Sample Heating Grid",
            ImagePath = string.Empty
        };
    }

    private static List<ProductionUnitBase> CreateProductionUnits()
    {
        return new List<ProductionUnitBase>
        {
            new GasBoiler
            {
                Name = "GB1",
                MaxHeatMW = 3.0,
                ProductionCostPerMWh = 510,
                CO2KgPerMWh = 132,
                GasConsumption = 1.05,
                ImageUrl = Path.Combine(ImagesFolder, "GB1.png")
            },
            new GasBoiler
            {
                Name = "GB2",
                MaxHeatMW = 2.0,
                ProductionCostPerMWh = 540,
                CO2KgPerMWh = 134,
                GasConsumption = 1.08,
                ImageUrl = Path.Combine(ImagesFolder, "GB2.png")
            },
            new GasBoiler
            {
                Name = "GB3",
                MaxHeatMW = 4.0,
                ProductionCostPerMWh = 580,
                CO2KgPerMWh = 136,
                GasConsumption = 1.09,
                ImageUrl = Path.Combine(ImagesFolder, "GB3.png")
            },
            new OilBoiler
            {
                Name = "OB1",
                MaxHeatMW = 6.0,
                ProductionCostPerMWh = 690,
                CO2KgPerMWh = 147,
                OilConsumption = 1.18,
                ImageUrl = Path.Combine(ImagesFolder, "OB1.jpg")
            },
            new GasMotor
            {
                Name = "GM1",
                MaxHeatMW = 5.3,
                ProductionCostPerMWh = 975,
                CO2KgPerMWh = 227,
                GasConsumption = 1.82,
                ElectricityProducedMW = 3.9,
                ImageUrl = Path.Combine(ImagesFolder, "GM1.png")
            },
            new ElectricBoiler
            {
                Name = "EB1",
                MaxHeatMW = 6.0,
                ProductionCostPerMWh = 15,
                CO2KgPerMWh = 0,
                ElectricityConsumedMW = 6.0,
                ImageUrl = Path.Combine(ImagesFolder, "EB1.jpg")
            }
        };
    }
}
