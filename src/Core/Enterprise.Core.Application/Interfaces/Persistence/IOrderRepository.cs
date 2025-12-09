namespace Enterprise.Core.Application.Interfaces.Persistence;

using Enterprise.Core.Domain.Entities.Sample;

/// <summary>
/// Order repository interface - siparişe özel sorgular için
/// </summary>
public interface IOrderRepository : IRepository<Order, long>
{
    /// <summary>
    /// ID'ye göre sipariş detayları ile birlikte getirir
    /// </summary>
    Task<Order?> GetWithDetailsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Müşteri ID'sine göre siparişleri detaylarıyla birlikte getirir
    /// </summary>
    Task<IReadOnlyList<Order>> GetByCustomerIdWithDetailsAsync(
        long customerId,
        CancellationToken cancellationToken = default);
}

