using data.Entities;
using data.Seed;
using Microsoft.EntityFrameworkCore;

namespace data;

// I configure the database shape and seed data here so every migrated database gets the same baseline setup.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Sample> Samples { get; set; }
    // I keep HeatPricePoints as its own table because it represents the CSV-based hourly heat/electricity dataset.
    public DbSet<HeatPricePoint> HeatPricePoints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HeatPricePoint>(entity =>
        {
            // I keep decimals precise and enforce a unique season+time window to avoid duplicate logical rows.
            entity.Property(x => x.HeatDemandMWh).HasPrecision(18, 4);
            entity.Property(x => x.ElectricityPriceDkkPerMWh).HasPrecision(18, 4);
            entity.HasIndex(x => new { x.Season, x.TimeFrom, x.TimeTo }).IsUnique();
            // I seed from HeatPricePointSeed so every database instance gets the same baseline data via migrations.
            entity.HasData(HeatPricePointSeed.Rows);
        });
    }
}
