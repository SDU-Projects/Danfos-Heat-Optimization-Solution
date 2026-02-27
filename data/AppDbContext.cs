using data.Entities;
using Microsoft.EntityFrameworkCore;

namespace data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Sample> Samples { get; set; }
}
