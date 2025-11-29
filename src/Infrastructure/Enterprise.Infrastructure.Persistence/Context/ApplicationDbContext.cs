using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.Infrastructure.Persistence.Context;

/// <summary>
/// Application DbContext
/// SqlServer ve Oracle destekli
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    #region DbSets

    public DbSet<Customer> Customers => Set<Customer>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());

        // Global query filters (soft delete)
        // modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}
