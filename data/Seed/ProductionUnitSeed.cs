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
            DataJson = """{"maxHeatMW":3.0,"costDkkPerMWh":510,"co2KgPerMWh":132,"gasConsumptionMWhPerMWh":1.05}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 2,
            Type = ProductionUnitType.GasBoiler,
            DataJson = """{"maxHeatMW":2.0,"costDkkPerMWh":540,"co2KgPerMWh":134,"gasConsumptionMWhPerMWh":1.08}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 3,
            Type = ProductionUnitType.GasBoiler,
            DataJson = """{"maxHeatMW":4.0,"costDkkPerMWh":580,"co2KgPerMWh":136,"gasConsumptionMWhPerMWh":1.09}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 4,
            Type = ProductionUnitType.OilBoiler,
            DataJson = """{"maxHeatMW":6.0,"costDkkPerMWh":690,"co2KgPerMWh":147,"oilConsumptionMWhPerMWh":1.18}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 5,
            Type = ProductionUnitType.GasMotor,
            DataJson = """{"maxHeatMW":5.3,"maxElectricityMW":3.9,"costDkkPerMWh":975,"co2KgPerMWh":227,"gasConsumptionMWhPerMWh":1.82}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        },
        new ProductionUnitEntity
        {
            Id = 6,
            Type = ProductionUnitType.ElectricBoiler,
            DataJson = """{"maxHeatMW":6.0,"electricityConsumedMW":6.0,"costDkkPerMWh":15,"co2KgPerMWh":0}""",
            OnMaintenance = false,
            IsConnectedToGrid = true,
            IsAvailable = true
        }
    ];
}
