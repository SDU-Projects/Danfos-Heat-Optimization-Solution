using data.Entities;
using data.Models;

namespace data.Seed;

public static class ProductionUnitSeed
{
    public static readonly ProductionUnit[] Rows = new ProductionUnit[]
    {
        new ProductionUnit
        {
            Id = 1,
            Type = ProductionUnitType.GasBoiler,
            Data = new GasBoiler
            {
                Name = "GB1",
                MaxHeatMW = 3.0,
                ProductionCostPerMWh = 510,
                CO2KgPerMWh = 132,
                GasConsumption = 1.05,
                ImageUrl = "GB1.png",
                IsAvailable = true
            }
        },
        new ProductionUnit
        {
            Id = 2,
            Type = ProductionUnitType.GasBoiler,
            Data = new GasBoiler
            {
                Name = "GB2",
                MaxHeatMW = 2.0,
                ProductionCostPerMWh = 540,
                CO2KgPerMWh = 134,
                GasConsumption = 1.08,
                ImageUrl = "GB2.png",
                IsAvailable = true
            }
        },
        new ProductionUnit
        {
            Id = 3,
            Type = ProductionUnitType.GasBoiler,
            Data = new GasBoiler
            {
                Name = "GB3",
                MaxHeatMW = 4.0,
                ProductionCostPerMWh = 580,
                CO2KgPerMWh = 136,
                GasConsumption = 1.09,
                ImageUrl = "GB3.png",
                IsAvailable = true
            }
        },
        new ProductionUnit
        {
            Id = 4,
            Type = ProductionUnitType.OilBoiler,
            Data = new OilBoiler
            {
                Name = "OB1",
                MaxHeatMW = 6.0,
                ProductionCostPerMWh = 690,
                CO2KgPerMWh = 147,
                OilConsumption = 1.18,
                ImageUrl = "OB1.jpg",
                IsAvailable = true
            }
        },
        new ProductionUnit
        {
            Id = 5,
            Type = ProductionUnitType.GasMotor,
            Data = new GasMotor
            {
                Name = "GM1",
                MaxHeatMW = 5.3,
                ProductionCostPerMWh = 975,
                CO2KgPerMWh = 227,
                GasConsumption = 1.82,
                ElectricityProducedMW = 3.9,
                ImageUrl = "GM1.png",
                IsAvailable = true
            }
        },
        new ProductionUnit
        {
            Id = 6,
            Type = ProductionUnitType.ElectricBoiler,
            Data = new ElectricBoiler
            {
                Name = "EB1",
                MaxHeatMW = 6.0,
                ProductionCostPerMWh = 15,
                CO2KgPerMWh = 0,
                ElectricityConsumedMW = 6.0,
                ImageUrl = "EB1.jpg",
                IsAvailable = true
            }
        },
    };
}
