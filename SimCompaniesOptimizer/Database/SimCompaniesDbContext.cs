using Microsoft.EntityFrameworkCore;
using SimCompaniesOptimizer.Extensions;
using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Database;

public class SimCompaniesDbContext : DbContext
{
    public SimCompaniesDbContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "simcompanies.db");
    }

    private string DbPath { get; }

    public DbSet<Resource> Resources { get; set; }

    public DbSet<ExchangeTrackerEntry> ExchangeTrackerEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>().Property(p => p.ProducedFrom).HasJsonConversion();
        modelBuilder.Entity<Resource>().Property(p => p.PriceCard).HasJsonConversion();

        modelBuilder.Entity<ExchangeTrackerEntry>().Property(p => p.ExchangePrices).HasJsonConversion();
    }
}