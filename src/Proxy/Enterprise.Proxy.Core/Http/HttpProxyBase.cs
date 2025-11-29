using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Models.Logging;
using Enterprise.Core.Shared.ErrorCodes;
using Enterprise.Core.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Enterprise.Proxy.Core.Http;

/// <summary>
/// HTTP Proxy base sınıfı
/// Otomatik loglama ve exception handling sağlar
/// </summary>
public abstract class HttpProxyBase
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly ICorrelationContext CorrelationContext;
    protected readonly ILogService LogService;
    protected readonly string ServiceName;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected HttpProxyBase(
        HttpClient httpClient,
        ILogger logger,
        ICorrelationContext correlationContext,
        ILogService logService,
        string serviceName)
    {
        HttpClient = httpClient;
        Logger = logger;
        CorrelationContext = correlationContext;
        LogService = logService;
        ServiceName = serviceName;

        // Correlation ID'yi her request'e ekle
        PrepareClient();
    }

    private void PrepareClient()
    {
        HttpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
        if (!string.IsNullOrEmpty(CorrelationContext.CorrelationId))
        {
            HttpClient.DefaultRequestHeaders.Add("X-Correlation-ID", CorrelationContext.CorrelationId);
        }
    }

    /// <summary>
    /// GET isteği gönderir
    /// </summary>
    protected async Task<TResponse?> GetAsync<TResponse>(
        string endpoint,
        ErrorCode? customErrorCode = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<TResponse>(
            async () =>
            {
                var response = await HttpClient.GetAsync(endpoint, cancellationToken);
                await EnsureSuccessAsync(response, endpoint, "GET");
                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
            },
            "GET",
            endpoint,
            customErrorCode);
    }

    /// <summary>
    /// POST isteği gönderir
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        ErrorCode? customErrorCode = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<TResponse>(
            async () =>
            {
                var response = await HttpClient.PostAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);
                await EnsureSuccessAsync(response, endpoint, "POST");
                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
            },
            "POST",
            endpoint,
            customErrorCode,
            request);
    }

    /// <summary>
    /// PUT isteği gönderir
    /// </summary>
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        ErrorCode? customErrorCode = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<TResponse>(
            async () =>
            {
                var response = await HttpClient.PutAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);
                await EnsureSuccessAsync(response, endpoint, "PUT");
                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
            },
            "PUT",
            endpoint,
            customErrorCode,
            request);
    }

    /// <summary>
    /// DELETE isteği gönderir
    /// </summary>
    protected async Task<bool> DeleteAsync(
        string endpoint,
        ErrorCode? customErrorCode = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<bool>(
            async () =>
            {
                var response = await HttpClient.DeleteAsync(endpoint, cancellationToken);
                await EnsureSuccessAsync(response, endpoint, "DELETE");
                return response.IsSuccessStatusCode;
            },
            "DELETE",
            endpoint,
            customErrorCode);
    }

    /// <summary>
    /// HTTP operasyonunu güvenli şekilde çalıştırır
    /// Exception'ları loglar ve BusinessException'a çevirir
    /// </summary>
    private async Task<TResult?> ExecuteAsync<TResult>(
        Func<Task<TResult?>> operation,
        string httpMethod,
        string endpoint,
        ErrorCode? customErrorCode = null,
        object? requestBody = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Logger.LogDebug(
                "[{CorrelationId}] HTTP {Method} Start: {ServiceName}{Endpoint}",
                CorrelationContext.CorrelationId,
                httpMethod,
                ServiceName,
                endpoint);

            var result = await operation();

            stopwatch.Stop();

            Logger.LogInformation(
                "[{CorrelationId}] HTTP {Method} Success: {ServiceName}{Endpoint} | Duration: {Duration}ms",
                CorrelationContext.CorrelationId,
                httpMethod,
                ServiceName,
                endpoint,
                stopwatch.ElapsedMilliseconds);

            // Performance log
            await LogPerformanceAsync(httpMethod, endpoint, stopwatch.ElapsedMilliseconds, true);

            return result;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            await HandleExceptionAsync(ex, httpMethod, endpoint, stopwatch.ElapsedMilliseconds,
                customErrorCode ?? CommonErrorCodes.ExternalServiceError, requestBody);
            throw; // BusinessException throw edildi
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();
            await HandleExceptionAsync(ex, httpMethod, endpoint, stopwatch.ElapsedMilliseconds,
                CommonErrorCodes.ExternalServiceTimeout, requestBody);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await HandleExceptionAsync(ex, httpMethod, endpoint, stopwatch.ElapsedMilliseconds,
                customErrorCode ?? CommonErrorCodes.ExternalServiceError, requestBody);
            throw;
        }
    }

    /// <summary>
    /// Response status kontrolü
    /// </summary>
    private async Task EnsureSuccessAsync(HttpResponseMessage response, string endpoint, string method)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            Logger.LogWarning(
                "[{CorrelationId}] HTTP {Method} Failed: {ServiceName}{Endpoint} | Status: {StatusCode} | Response: {Response}",
                CorrelationContext.CorrelationId,
                method,
                ServiceName,
                endpoint,
                (int)response.StatusCode,
                content);

            throw new HttpRequestException(
                $"HTTP {method} {endpoint} failed with status {(int)response.StatusCode}: {content}");
        }
    }

    /// <summary>
    /// Exception'ı loglar ve BusinessException'a çevirir
    /// Orijinal hatayı KAYBETMEZ
    /// </summary>
    private async Task HandleExceptionAsync(
        Exception ex,
        string httpMethod,
        string endpoint,
        long durationMs,
        ErrorCode errorCode,
        object? requestBody)
    {
        // 1. Önce mevcut hatayı logla (KRİTİK - hata kaybedilmemeli)
        Logger.LogError(ex,
            "[{CorrelationId}] HTTP {Method} Failed: {ServiceName}{Endpoint} | Duration: {Duration}ms | Error: {ErrorMessage}",
            CorrelationContext.CorrelationId,
            httpMethod,
            ServiceName,
            endpoint,
            durationMs,
            ex.Message);

        // 2. Exception log entry oluştur
        await LogService.LogExceptionAsync(new ExceptionLogEntry
        {
            ExceptionType = ex.GetType().FullName,
            ExceptionMessage = ex.Message,
            StackTrace = ex.StackTrace,
            InnerExceptionMessage = ex.InnerException?.Message,
            InnerExceptionType = ex.InnerException?.GetType().FullName,
            MethodName = $"{ServiceName} {httpMethod} {endpoint}",
            LayerName = "Proxy",
            Severity = errorCode.Severity.ToString(),
            ExceptionCategory = "ExternalService",
            RequestPath = endpoint,
            HttpMethod = httpMethod,
            RequestBody = requestBody != null ? JsonSerializer.Serialize(requestBody) : null,
            AdditionalData = new Dictionary<string, object>
            {
                ["ServiceName"] = ServiceName,
                ["Endpoint"] = endpoint,
                ["HttpMethod"] = httpMethod,
                ["DurationMs"] = durationMs,
                ["ErrorCode"] = errorCode.Code,
                ["CorrelationId"] = CorrelationContext.CorrelationId ?? ""
            }
        });

        // 3. Performance log (başarısız)
        await LogPerformanceAsync(httpMethod, endpoint, durationMs, false);

        // 4. BusinessException fırlat (orijinal exception'ı koru)
        throw new BusinessException(
            errorCode,
            ex,
            new Dictionary<string, object>
            {
                ["ServiceName"] = ServiceName,
                ["Endpoint"] = endpoint,
                ["HttpMethod"] = httpMethod,
                ["OriginalError"] = ex.Message
            });
    }

    private async Task LogPerformanceAsync(string httpMethod, string endpoint, long durationMs, bool success)
    {
        try
        {
            await LogService.LogPerformanceAsync(new PerformanceLogEntry
            {
                OperationName = $"{ServiceName} {httpMethod} {endpoint}",
                OperationType = "HTTP",
                DurationMs = durationMs,
                Success = success,
                Metadata = new Dictionary<string, object>
                {
                    ["ServiceName"] = ServiceName,
                    ["HttpMethod"] = httpMethod,
                    ["Endpoint"] = endpoint
                }
            });
        }
        catch
        {
            // Loglama hatası ana akışı etkilememeli
        }
    }
}

