using System.Linq.Expressions;

namespace Enterprise.Core.Application.Interfaces.Persistence;

/// <summary>
/// Generic repository interface
/// Domain katmanı ile Persistence katmanı arasında soyutlama sağlar
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">ID tipi</typeparam>
public interface IRepository<TEntity, TId> where TEntity : class
{
    /// <summary>
    /// ID ile entity getirir
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tüm entity'leri getirir
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula göre entity'leri getirir
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula göre tek entity getirir
    /// </summary>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sayfalanmış sonuç getirir
    /// </summary>
    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Entity ekler
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Birden fazla entity ekler
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Entity günceller
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Birden fazla entity günceller
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Entity siler
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// ID ile entity siler
    /// </summary>
    Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula göre entity var mı kontrol eder
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula göre sayı döndürür
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Guid ID'li entity'ler için repository
/// </summary>
public interface IRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : class
{
}

