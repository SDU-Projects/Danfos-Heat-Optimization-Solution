using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.Migrations
{
    /// <inheritdoc />
    public partial class fix_seed_isavailable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 1,
                column: "JsonData",
                value: "{\"$type\":\"GasBoiler\",\"GasConsumption\":1.05,\"Name\":\"GB1\",\"ImageUrl\":\"GB1.png\",\"MaxHeatMW\":3,\"ProductionCostPerMWh\":510,\"CO2KgPerMWh\":132,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":0,\"IsAvailable\":true,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 2,
                column: "JsonData",
                value: "{\"$type\":\"GasBoiler\",\"GasConsumption\":1.08,\"Name\":\"GB2\",\"ImageUrl\":\"GB2.png\",\"MaxHeatMW\":2,\"ProductionCostPerMWh\":540,\"CO2KgPerMWh\":134,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":0,\"IsAvailable\":true,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 3,
                column: "JsonData",
                value: "{\"$type\":\"GasBoiler\",\"GasConsumption\":1.09,\"Name\":\"GB3\",\"ImageUrl\":\"GB3.png\",\"MaxHeatMW\":4,\"ProductionCostPerMWh\":580,\"CO2KgPerMWh\":136,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":0,\"IsAvailable\":true,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 4,
                column: "JsonData",
                value: "{\"$type\":\"OilBoiler\",\"OilConsumption\":1.18,\"Name\":\"OB1\",\"ImageUrl\":\"OB1.jpg\",\"MaxHeatMW\":6,\"ProductionCostPerMWh\":690,\"CO2KgPerMWh\":147,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":0,\"IsAvailable\":true,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 5,
                column: "JsonData",
                value: "{\"$type\":\"GasMotor\",\"GasConsumption\":1.82,\"Name\":\"GM1\",\"ImageUrl\":\"GM1.png\",\"MaxHeatMW\":5.3,\"ProductionCostPerMWh\":975,\"CO2KgPerMWh\":227,\"ElectricityProducedMW\":3.9,\"ElectricityConsumedMW\":0,\"IsAvailable\":true,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 6,
                column: "JsonData",
                value: "{\"$type\":\"ElectricBoiler\",\"Name\":\"EB1\",\"ImageUrl\":\"EB1.jpg\",\"MaxHeatMW\":6,\"ProductionCostPerMWh\":15,\"CO2KgPerMWh\":0,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":6,\"IsAvailable\":true,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 1,
                column: "JsonData",
                value: "{\"$type\":\"GasBoiler\",\"GasConsumption\":1.05,\"Name\":\"GB1\",\"ImageUrl\":\"GB1.png\",\"MaxHeatMW\":3,\"ProductionCostPerMWh\":510,\"CO2KgPerMWh\":132,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":0,\"IsAvailable\":false,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 2,
                column: "JsonData",
                value: "{\"$type\":\"GasBoiler\",\"GasConsumption\":1.08,\"Name\":\"GB2\",\"ImageUrl\":\"GB2.png\",\"MaxHeatMW\":2,\"ProductionCostPerMWh\":540,\"CO2KgPerMWh\":134,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":0,\"IsAvailable\":false,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 3,
                column: "JsonData",
                value: "{\"$type\":\"GasBoiler\",\"GasConsumption\":1.09,\"Name\":\"GB3\",\"ImageUrl\":\"GB3.png\",\"MaxHeatMW\":4,\"ProductionCostPerMWh\":580,\"CO2KgPerMWh\":136,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":0,\"IsAvailable\":false,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 4,
                column: "JsonData",
                value: "{\"$type\":\"OilBoiler\",\"OilConsumption\":1.18,\"Name\":\"OB1\",\"ImageUrl\":\"OB1.jpg\",\"MaxHeatMW\":6,\"ProductionCostPerMWh\":690,\"CO2KgPerMWh\":147,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":0,\"IsAvailable\":false,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 5,
                column: "JsonData",
                value: "{\"$type\":\"GasMotor\",\"GasConsumption\":1.82,\"Name\":\"GM1\",\"ImageUrl\":\"GM1.png\",\"MaxHeatMW\":5.3,\"ProductionCostPerMWh\":975,\"CO2KgPerMWh\":227,\"ElectricityProducedMW\":3.9,\"ElectricityConsumedMW\":0,\"IsAvailable\":false,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");

            migrationBuilder.UpdateData(
                table: "ProductionUnits",
                keyColumn: "Id",
                keyValue: 6,
                column: "JsonData",
                value: "{\"$type\":\"ElectricBoiler\",\"Name\":\"EB1\",\"ImageUrl\":\"EB1.jpg\",\"MaxHeatMW\":6,\"ProductionCostPerMWh\":15,\"CO2KgPerMWh\":0,\"ElectricityProducedMW\":0,\"ElectricityConsumedMW\":6,\"IsAvailable\":false,\"OnMaintainance\":false,\"IsConnectedToGrid\":false}");
        }
    }
}
