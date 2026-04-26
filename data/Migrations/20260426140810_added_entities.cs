using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace data.Migrations
{
    /// <inheritdoc />
    public partial class added_entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OptimizationRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Objective = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CostWeight = table.Column<decimal>(type: "numeric", nullable: false),
                    Co2Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    ElectricityPriceSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TimeFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalNetCostDkk = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCo2Kg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalElectricityCashflowDkk = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationHourResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptimizationRunId = table.Column<int>(type: "integer", nullable: false),
                    TimeFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HeatDemandMWh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ElectricityPriceDkkPerMWh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    HeatSuppliedMWh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalNetCostDkk = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCo2Kg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ElectricityCashflowDkk = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationHourResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptimizationHourResults_OptimizationRuns_OptimizationRunId",
                        column: x => x.OptimizationRunId,
                        principalTable: "OptimizationRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationUnitHourResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptimizationHourResultId = table.Column<int>(type: "integer", nullable: false),
                    ProductionUnitId = table.Column<int>(type: "integer", nullable: false),
                    UnitName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HeatProducedMWh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ElectricityMWh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    NetCostDkk = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Co2Kg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ScorePerMWh = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationUnitHourResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptimizationUnitHourResults_OptimizationHourResults_Optimiz~",
                        column: x => x.OptimizationHourResultId,
                        principalTable: "OptimizationHourResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationHourResults_OptimizationRunId_TimeFromUtc",
                table: "OptimizationHourResults",
                columns: new[] { "OptimizationRunId", "TimeFromUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationUnitHourResults_OptimizationHourResultId_Produc~",
                table: "OptimizationUnitHourResults",
                columns: new[] { "OptimizationHourResultId", "ProductionUnitId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptimizationUnitHourResults");

            migrationBuilder.DropTable(
                name: "OptimizationHourResults");

            migrationBuilder.DropTable(
                name: "OptimizationRuns");
        }
    }
}
