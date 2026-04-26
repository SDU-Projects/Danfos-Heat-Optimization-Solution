using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using data.Entities;
using data.Models.Base;
using data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace data;

// I configure the database shape and seed data here so every migrated database gets the same baseline setup.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ProductionUnit> ProductionUnits { get; set; }
    public DbSet<HeatPricePoint> HeatPricePoints { get; set; }
    public DbSet<OptimizationRun> OptimizationRuns { get; set; }
    public DbSet<OptimizationHourResult> OptimizationHourResults { get; set; }
    public DbSet<OptimizationUnitHourResult> OptimizationUnitHourResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        modelBuilder.Entity<ProductionUnit>(entity =>
        {
            entity.Property(x => x.Type)
                .HasConversion<string>();

            var converter = new ValueConverter<ProductionUnitBase, string>(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<ProductionUnitBase>(v, jsonOptions)!
            );

            var comparer = new ValueComparer<ProductionUnitBase>(
                (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                v => JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                v => JsonSerializer.Deserialize<ProductionUnitBase>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)!
            );

            entity.Property(x => x.Data)
                .HasColumnName("JsonData")
                .HasConversion(converter, comparer);

            entity.HasData(ProductionUnitSeed.Rows);
        });

        modelBuilder.Entity<HeatPricePoint>(entity =>
        {
            // I keep decimals precise and enforce a unique season+time window to avoid duplicate logical rows.
            entity.Property(x => x.HeatDemandMWh).HasPrecision(18, 4);
            entity.Property(x => x.ElectricityPriceDkkPerMWh).HasPrecision(18, 4);
            entity.HasIndex(x => new { x.Season, x.TimeFrom, x.TimeTo }).IsUnique();
            // I seed from HeatPricePointSeed so every database instance gets the same baseline data via migrations.
            entity.HasData(HeatPricePointSeed.Rows);
        });

        modelBuilder.Entity<OptimizationRun>(entity =>
        {
            entity.Property(x => x.TotalNetCostDkk).HasPrecision(18, 4);
            entity.Property(x => x.TotalCo2Kg).HasPrecision(18, 4);
            entity.Property(x => x.TotalElectricityCashflowDkk).HasPrecision(18, 4);
        });

        modelBuilder.Entity<OptimizationHourResult>(entity =>
        {
            entity.Property(x => x.HeatDemandMWh).HasPrecision(18, 4);
            entity.Property(x => x.ElectricityPriceDkkPerMWh).HasPrecision(18, 4);
            entity.Property(x => x.HeatSuppliedMWh).HasPrecision(18, 4);
            entity.Property(x => x.TotalNetCostDkk).HasPrecision(18, 4);
            entity.Property(x => x.TotalCo2Kg).HasPrecision(18, 4);
            entity.Property(x => x.ElectricityCashflowDkk).HasPrecision(18, 4);

            entity.HasIndex(x => new { x.OptimizationRunId, x.TimeFromUtc }).IsUnique();
        });

        modelBuilder.Entity<OptimizationUnitHourResult>(entity =>
        {
            entity.Property(x => x.HeatProducedMWh).HasPrecision(18, 4);
            entity.Property(x => x.ElectricityMWh).HasPrecision(18, 4);
            entity.Property(x => x.NetCostDkk).HasPrecision(18, 4);
            entity.Property(x => x.Co2Kg).HasPrecision(18, 4);
            entity.Property(x => x.ScorePerMWh).HasPrecision(18, 6);

            entity.HasIndex(x => new { x.OptimizationHourResultId, x.ProductionUnitId }).IsUnique();
        });
    }
}
