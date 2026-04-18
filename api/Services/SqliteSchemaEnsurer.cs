using System.Data;
using data;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public static class SqliteSchemaEnsurer
{
    public static async Task EnsureOptimizationTablesExistAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (context.Database.ProviderName == null || !context.Database.ProviderName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        // I create the tables with IF NOT EXISTS so an existing database file can be upgraded without migrations.
        string[] commands =
        [
            "CREATE TABLE IF NOT EXISTS OptimizationRuns (Id INTEGER PRIMARY KEY AUTOINCREMENT, CreatedAtUtc TEXT NOT NULL, Objective TEXT NOT NULL, CostWeight TEXT NOT NULL, Co2Weight TEXT NOT NULL, ElectricityPriceSource TEXT NOT NULL, TimeFromUtc TEXT NOT NULL, TimeToUtc TEXT NOT NULL, TotalNetCostDkk TEXT NOT NULL, TotalCo2Kg TEXT NOT NULL, TotalElectricityCashflowDkk TEXT NOT NULL);",
            "CREATE TABLE IF NOT EXISTS OptimizationHourResults (Id INTEGER PRIMARY KEY AUTOINCREMENT, OptimizationRunId INTEGER NOT NULL, TimeFromUtc TEXT NOT NULL, HeatDemandMWh TEXT NOT NULL, ElectricityPriceDkkPerMWh TEXT NOT NULL, HeatSuppliedMWh TEXT NOT NULL, TotalNetCostDkk TEXT NOT NULL, TotalCo2Kg TEXT NOT NULL, ElectricityCashflowDkk TEXT NOT NULL, FOREIGN KEY(OptimizationRunId) REFERENCES OptimizationRuns(Id) ON DELETE CASCADE);",
            "CREATE TABLE IF NOT EXISTS OptimizationUnitHourResults (Id INTEGER PRIMARY KEY AUTOINCREMENT, OptimizationHourResultId INTEGER NOT NULL, ProductionUnitId INTEGER NOT NULL, UnitName TEXT NOT NULL, HeatProducedMWh TEXT NOT NULL, ElectricityMWh TEXT NOT NULL, NetCostDkk TEXT NOT NULL, Co2Kg TEXT NOT NULL, ScorePerMWh TEXT NOT NULL, FOREIGN KEY(OptimizationHourResultId) REFERENCES OptimizationHourResults(Id) ON DELETE CASCADE);",
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_OptimizationHourResults_Run_TimeFromUtc ON OptimizationHourResults(OptimizationRunId, TimeFromUtc);",
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_OptimizationUnitHourResults_Hour_Unit ON OptimizationUnitHourResults(OptimizationHourResultId, ProductionUnitId);"
        ];

        foreach (string sql in commands)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
