using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DataJson = table.Column<string>(type: "jsonb", nullable: false),
                    OnMaintenance = table.Column<bool>(type: "boolean", nullable: false),
                    IsConnectedToGrid = table.Column<bool>(type: "boolean", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionUnits", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ProductionUnits",
                columns: new[] { "Id", "DataJson", "IsAvailable", "IsConnectedToGrid", "OnMaintenance", "Type" },
                values: new object[,]
                {
                    { 1, "{\"maxHeatMW\":3.0,\"costDkkPerMWh\":510,\"co2KgPerMWh\":132,\"gasConsumptionMWhPerMWh\":1.05}", true, true, false, "GasBoiler" },
                    { 2, "{\"maxHeatMW\":2.0,\"costDkkPerMWh\":540,\"co2KgPerMWh\":134,\"gasConsumptionMWhPerMWh\":1.08}", true, true, false, "GasBoiler" },
                    { 3, "{\"maxHeatMW\":4.0,\"costDkkPerMWh\":580,\"co2KgPerMWh\":136,\"gasConsumptionMWhPerMWh\":1.09}", true, true, false, "GasBoiler" },
                    { 4, "{\"maxHeatMW\":6.0,\"costDkkPerMWh\":690,\"co2KgPerMWh\":147,\"oilConsumptionMWhPerMWh\":1.18}", true, true, false, "OilBoiler" },
                    { 5, "{\"maxHeatMW\":5.3,\"maxElectricityMW\":3.9,\"costDkkPerMWh\":975,\"co2KgPerMWh\":227,\"gasConsumptionMWhPerMWh\":1.82}", true, true, false, "GasMotor" },
                    { 6, "{\"maxHeatMW\":6.0,\"electricityConsumedMW\":6.0,\"costDkkPerMWh\":15,\"co2KgPerMWh\":0}", true, true, false, "ElectricBoiler" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionUnits");
        }
    }
}
