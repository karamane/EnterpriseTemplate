using Enterprise.Api.Client.Wcf.DTOs;

namespace Enterprise.Api.Client.Wcf.Services;

/// <summary>
/// WCF Client için Server API client interface
/// </summary>
public interface IWcfServerApiClient
{
    /// <summary>
    /// Server API'den authentication yapar ve token alır
    /// </summary>
    Task<LoginWcfResponse?> AuthenticateAsync(LoginWcfRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh token ile yeni token alır
    /// </summary>
    Task<LoginWcfResponse?> RefreshTokenAsync(RefreshTokenWcfRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// GET isteği yapar
    /// </summary>
    Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// POST isteği yapar
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// PUT isteği yapar
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// DELETE isteği yapar
    /// </summary>
    Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}


