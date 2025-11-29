namespace Enterprise.Core.Application.Interfaces.Caching;

/// <summary>
/// Cache servisi interface
/// Distributed cache operasyonları için
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Cache'den veri getirir
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache'e veri yazar
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache'den veri siler
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pattern ile eşleşen key'leri siler
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Key'in var olup olmadığını kontrol eder
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache'den veri getirir, yoksa factory ile oluşturur ve cache'e yazar
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Key'in süresini yeniler
    /// </summary>
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Birden fazla key'i tek seferde getirir
    /// </summary>
    Task<Dictionary<string, T?>> GetManyAsync<T>(
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Birden fazla key'i tek seferde yazar
    /// </summary>
    Task SetManyAsync<T>(
        Dictionary<string, T> items,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default);
}

