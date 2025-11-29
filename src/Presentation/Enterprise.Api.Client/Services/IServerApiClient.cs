namespace Enterprise.Api.Client.Services;

/// <summary>
/// Server API ile iletişim interface'i
/// Client API tamamen izole - generic HTTP client
/// </summary>
public interface IServerApiClient
{
    /// <summary>
    /// GET isteği gönderir
    /// </summary>
    Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// POST isteği gönderir
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// PUT isteği gönderir
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// DELETE isteği gönderir
    /// </summary>
    Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}

