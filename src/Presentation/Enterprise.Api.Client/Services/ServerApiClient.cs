using System.Net.Http.Json;
using System.Text.Json;
using Enterprise.Core.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Enterprise.Api.Client.Services;

/// <summary>
/// Server API HTTP client implementasyonu
/// Correlation ID ve Authorization header propagation dahil
/// </summary>
public class ServerApiClient : IServerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ICorrelationContext _correlationContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ServerApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ServerApiClient(
        HttpClient httpClient,
        ICorrelationContext correlationContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ServerApiClient> logger)
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

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        PrepareRequest();

        var response = await _httpClient.GetAsync(endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "[{CorrelationId}] Server API GET {Endpoint} returned {StatusCode}",
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
            _logger.LogWarning(
                "[{CorrelationId}] Server API POST {Endpoint} returned {StatusCode}",
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
            _logger.LogWarning(
                "[{CorrelationId}] Server API PUT {Endpoint} returned {StatusCode}",
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

    private void PrepareRequest()
    {
        // Correlation ID propagation
        if (!string.IsNullOrEmpty(_correlationContext.CorrelationId))
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", _correlationContext.CorrelationId);
        }

        // Authorization header propagation (Bearer token)
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
        }
    }
}
