using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.Infrastructure.Persistence.Repositories.EfCore;

/// <summary>
/// Order repository EF Core implementasyonu
/// </summary>
public class OrderRepository : EfCoreRepository<Order, long>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<Order?> GetWithDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Order>> GetByCustomerIdWithDetailsAsync(
        long customerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId && !o.IsDeleted)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }
}

