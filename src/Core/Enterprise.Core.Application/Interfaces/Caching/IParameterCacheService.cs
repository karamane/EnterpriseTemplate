namespace Enterprise.Core.Application.Interfaces.Caching;

/// <summary>
/// Parametre cache servisi
/// Uygulama parametrelerinin yönetimi için
/// </summary>
public interface IParameterCacheService
{
    /// <summary>
    /// Parametre değerini getirir
    /// </summary>
    Task<T?> GetParameterAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parametre değerini getirir, yoksa default değer döner
    /// </summary>
    Task<T> GetParameterAsync<T>(string key, T defaultValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parametre değerini günceller
    /// </summary>
    Task SetParameterAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tüm parametreleri yeniden yükler
    /// </summary>
    Task RefreshAllParametersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen parametreyi yeniden yükler
    /// </summary>
    Task RefreshParameterAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parametre değişikliği bildirir (Pub/Sub)
    /// Tüm sunuculara cache refresh sinyali gönderir
    /// </summary>
    Task NotifyParameterChangedAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tüm parametreleri dictionary olarak getirir
    /// </summary>
    Task<Dictionary<string, object>> GetAllParametersAsync(CancellationToken cancellationToken = default);
}

