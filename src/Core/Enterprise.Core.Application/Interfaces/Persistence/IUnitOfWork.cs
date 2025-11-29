namespace Enterprise.Core.Application.Interfaces.Persistence;

/// <summary>
/// Unit of Work pattern interface
/// Transaction yönetimi ve repository koordinasyonu sağlar
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Değişiklikleri kaydeder
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction başlatır
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction'ı commit eder
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction'ı rollback eder
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction içinde işlem yapar
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction içinde işlem yapar (return değersiz)
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);
}

