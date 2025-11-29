using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Models.Logging;
using Enterprise.Core.Shared.Constants;
using Enterprise.Core.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Enterprise.Proxy.Core.Base;

/// <summary>
/// Tüm HTTP proxy'lerin base sınıfı
/// Logging, resilience ve correlation ID propagation sağlar
/// </summary>
/// <typeparam name="TProxy">Proxy tipi (loglama için)</typeparam>
public abstract class BaseHttpProxy<TProxy>
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger<TProxy> Logger;
    protected readonly ILogService LogService;
    protected readonly ICorrelationContext CorrelationContext;

    protected abstract string ServiceName { get; }

    protected BaseHttpProxy(
        HttpClient httpClient,
        ILogger<TProxy> logger,
        ILogService logService,
        ICorrelationContext correlationContext)
    {
        HttpClient = httpClient;
        Logger = logger;
        LogService = logService;
        CorrelationContext = correlationContext;
    }

    /// <summary>
    /// GET request gönderir
    /// </summary>
    protected async Task<TResponse?> GetAsync<TResponse>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<object, TResponse>(
            endpoint,
            HttpMethod.Get,
            default,
            cancellationToken);
    }

    /// <summary>
    /// POST request gönderir
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<TRequest, TResponse>(
            endpoint,
            HttpMethod.Post,
            request,
            cancellationToken);
    }

    /// <summary>
    /// PUT request gönderir
    /// </summary>
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<TRequest, TResponse>(
            endpoint,
            HttpMethod.Put,
            request,
            cancellationToken);
    }

    /// <summary>
    /// DELETE request gönderir
    /// </summary>
    protected async Task DeleteAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync<object, object>(
            endpoint,
            HttpMethod.Delete,
            default,
            cancellationToken);
    }

    /// <summary>
    /// HTTP request'i yürütür - logging ve error handling ile
    /// </summary>
    private async Task<TResponse?> ExecuteAsync<TRequest, TResponse>(
        string endpoint,
        HttpMethod method,
        TRequest? request,
        CancellationToken cancellationToken)
    {
        var fullUrl = $"{HttpClient.BaseAddress}{endpoint}";

        // Request log
        var requestLog = new RequestLogEntry
        {
            CorrelationId = CorrelationContext.CorrelationId,
            Layer = LogConstants.Layers.Proxy,
            ClassName = typeof(TProxy).Name,
            MethodName = endpoint,
            HttpMethod = method.Method,
            RequestPath = fullUrl,
            RequestBody = request != null ? JsonSerializer.Serialize(request) : null
        };

        await LogService.LogRequestAsync(requestLog, cancellationToken);

        var sw = Stopwatch.StartNew();

        try
        {
            using var httpRequest = new HttpRequestMessage(method, endpoint);

            // Correlation ID header ekle
            httpRequest.Headers.Add(LogConstants.Headers.CorrelationId, CorrelationContext.CorrelationId);

            // Request body ekle
            if (request != null)
            {
                httpRequest.Content = JsonContent.Create(request);
            }

            Logger.LogDebug(
                "[{CorrelationId}] Calling external service {ServiceName}: {Method} {Url}",
                CorrelationContext.CorrelationId, ServiceName, method, fullUrl);

            var response = await HttpClient.SendAsync(httpRequest, cancellationToken);
            sw.Stop();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            // Response log
            var responseLog = new ResponseLogEntry
            {
                CorrelationId = CorrelationContext.CorrelationId,
                Layer = LogConstants.Layers.Proxy,
                RequestLogId = requestLog.LogId,
                StatusCode = (int)response.StatusCode,
                DurationMs = sw.ElapsedMilliseconds,
                ResponseBody = responseBody.Length > 4000 ? responseBody[..4000] : responseBody
            };

            await LogService.LogResponseAsync(responseLog, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "[{CorrelationId}] External service {ServiceName} returned {StatusCode}: {Response}",
                    CorrelationContext.CorrelationId, ServiceName, response.StatusCode, responseBody);

                throw new ExternalServiceException(ServiceName, (int)response.StatusCode, responseBody);
            }

            if (string.IsNullOrEmpty(responseBody))
            {
                return default;
            }

            return JsonSerializer.Deserialize<TResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();

            Logger.LogError(ex,
                "[{CorrelationId}] HTTP error calling {ServiceName}: {Message}",
                CorrelationContext.CorrelationId, ServiceName, ex.Message);

            // Exception log
            var exceptionLog = ExceptionLogEntry.FromException(
                ex,
                LogConstants.Layers.Proxy,
                CorrelationContext.CorrelationId);

            exceptionLog.ClassName = typeof(TProxy).Name;
            exceptionLog.MethodName = endpoint;
            exceptionLog.RequestPath = fullUrl;
            exceptionLog.HttpMethod = method.Method;
            exceptionLog.IsTransient = true;

            await LogService.LogExceptionAsync(exceptionLog, cancellationToken);

            throw new ExternalServiceException(ServiceName, ex.Message);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            sw.Stop();

            Logger.LogError(
                "[{CorrelationId}] Timeout calling {ServiceName} after {Duration}ms",
                CorrelationContext.CorrelationId, ServiceName, sw.ElapsedMilliseconds);

            throw new ExternalServiceException(ServiceName, $"Request timed out after {sw.ElapsedMilliseconds}ms");
        }
    }
}

