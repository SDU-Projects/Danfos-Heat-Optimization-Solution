using data.Entities;

namespace data.Seed;

public static class ProductionUnitSeed
{
    public static readonly ProductionUnitEntity[] Rows =
    [
        new ProductionUnitEntity
        {
            Id = 1,
            Type = ProductionUnitType.GasBoiler,
            DataJson = """{"name":"GB1","imagePath":"AssetManager/Images/GB1.png","maxHeatMW":3.0,"costDkkPerMWh":510,"co2KgPerMWh":132,"gasConsumptionMWhPerMWh":1.05}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 2,
            Type = ProductionUnitType.GasBoiler,
            DataJson = """{"name":"GB2","imagePath":"AssetManager/Images/GB2.png","maxHeatMW":2.0,"costDkkPerMWh":540,"co2KgPerMWh":134,"gasConsumptionMWhPerMWh":1.08}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 3,
            Type = ProductionUnitType.GasBoiler,
            DataJson = """{"name":"GB3","imagePath":"AssetManager/Images/GB3.png","maxHeatMW":4.0,"costDkkPerMWh":580,"co2KgPerMWh":136,"gasConsumptionMWhPerMWh":1.09}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 4,
            Type = ProductionUnitType.OilBoiler,
            DataJson = """{"name":"OB1","imagePath":"AssetManager/Images/OB1.jpg","maxHeatMW":6.0,"costDkkPerMWh":690,"co2KgPerMWh":147,"oilConsumptionMWhPerMWh":1.18}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 5,
            Type = ProductionUnitType.GasMotor,
            DataJson = """{"name":"GM1","imagePath":"AssetManager/Images/GM1.png","maxHeatMW":5.3,"maxElectricityMW":3.9,"costDkkPerMWh":975,"co2KgPerMWh":227,"gasConsumptionMWhPerMWh":1.82}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 6,
            Type = ProductionUnitType.ElectricBoiler,
            DataJson = """{"name":"EB1","imagePath":"AssetManager/Images/EB1.jpg","maxHeatMW":6.0,"electricityConsumedMW":6.0,"costDkkPerMWh":15,"co2KgPerMWh":0}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        }
    ];
}
