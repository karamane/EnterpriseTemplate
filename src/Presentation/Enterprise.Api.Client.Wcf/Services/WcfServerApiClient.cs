using System.Net.Http.Json;
using System.Text.Json;
using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Core.Application.Interfaces.Logging;

namespace Enterprise.Api.Client.Wcf.Services;

/// <summary>
/// WCF Client için Server API client implementasyonu
/// </summary>
public class WcfServerApiClient : IWcfServerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ICorrelationContext _correlationContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<WcfServerApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WcfServerApiClient(
        HttpClient httpClient,
        ICorrelationContext correlationContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<WcfServerApiClient> logger)
    {
        _httpClient = httpClient;
        _correlationContext = correlationContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<LoginWcfResponse?> AuthenticateAsync(LoginWcfRequest request, CancellationToken cancellationToken = default)
    {
        PrepareRequest(skipAuth: true);

        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request, _jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Authentication failed for user {Username}", request.Username);
            return null;
        }

        var serverResponse = await response.Content.ReadFromJsonAsync<ServerAuthResponse>(_jsonOptions, cancellationToken);
        
        if (serverResponse?.Success != true || serverResponse.Data == null)
        {
            return null;
        }

        var data = serverResponse.Data;
        return new LoginWcfResponse(
            data.AccessToken,
            data.RefreshToken,
            data.ExpiresAt,
            data.User?.Username ?? request.Username,
            data.User?.Roles.ToArray() ?? Array.Empty<string>());
    }

    public async Task<LoginWcfResponse?> RefreshTokenAsync(RefreshTokenWcfRequest request, CancellationToken cancellationToken = default)
    {
        PrepareRequest(skipAuth: true);

        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/refresh", request, _jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var serverResponse = await response.Content.ReadFromJsonAsync<ServerAuthResponse>(_jsonOptions, cancellationToken);
        
        if (serverResponse?.Success != true || serverResponse.Data == null)
        {
            return null;
        }

        var data = serverResponse.Data;
        return new LoginWcfResponse(
            data.AccessToken,
            data.RefreshToken,
            data.ExpiresAt,
            data.User?.Username ?? string.Empty,
            data.User?.Roles.ToArray() ?? Array.Empty<string>());
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        PrepareRequest();

        var response = await _httpClient.GetAsync(endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[{CorrelationId}] Server API GET {Endpoint} returned {StatusCode}",
                _correlationContext.CorrelationId, endpoint, response.StatusCode);
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        PrepareRequest();

        var response = await _httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[{CorrelationId}] Server API POST {Endpoint} returned {StatusCode}",
                _correlationContext.CorrelationId, endpoint, response.StatusCode);
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        PrepareRequest();

        var response = await _httpClient.PutAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[{CorrelationId}] Server API PUT {Endpoint} returned {StatusCode}",
                _correlationContext.CorrelationId, endpoint, response.StatusCode);
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        PrepareRequest();

        var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private void PrepareRequest(bool skipAuth = false)
    {
        // Correlation ID propagation
        if (!string.IsNullOrEmpty(_correlationContext.CorrelationId))
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", _correlationContext.CorrelationId);
        }

        // Authorization header propagation (Bearer token)
        if (!skipAuth)
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
            }
        }
    }
}


