using Enterprise.Core.Domain.Entities.Auth;
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

    // Order entities
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Auth entities
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());

        // Order configurations
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());

        // Auth configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // Global query filters (soft delete)
        // modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}
