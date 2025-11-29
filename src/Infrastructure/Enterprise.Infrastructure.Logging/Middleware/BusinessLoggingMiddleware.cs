using System.Diagnostics;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Models.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enterprise.Infrastructure.Logging.Middleware;

/// <summary>
/// MediatR pipeline için otomatik loglama behavior
/// Her Command/Query otomatik loglanır - handler'larda log yazılmasına gerek kalmaz
/// </summary>
public class AutoLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AutoLoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogService _logService;

    public AutoLoggingBehavior(
        ILogger<AutoLoggingBehavior<TRequest, TResponse>> logger,
        ICorrelationContext correlationContext,
        ILogService logService)
    {
        _logger = logger;
        _correlationContext = correlationContext;
        _logService = logService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        // Request başlangıcını logla
        _logger.LogInformation(
            "[{CorrelationId}] MediatR START: {RequestName}",
            _correlationContext.CorrelationId,
            requestName);

        try
        {
            var response = await next();

            stopwatch.Stop();

            // Başarılı response'u logla
            _logger.LogInformation(
                "[{CorrelationId}] MediatR END: {RequestName} | Duration: {Duration}ms | Success",
                _correlationContext.CorrelationId,
                requestName,
                stopwatch.ElapsedMilliseconds);

            // Performance log (async - fire and forget)
            _ = LogPerformanceAsync(requestName, stopwatch.ElapsedMilliseconds, true, null);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Hata logla
            _logger.LogError(ex,
                "[{CorrelationId}] MediatR ERROR: {RequestName} | Duration: {Duration}ms | Error: {ErrorMessage}",
                _correlationContext.CorrelationId,
                requestName,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            // Performance ve Exception logları (async)
            _ = LogPerformanceAsync(requestName, stopwatch.ElapsedMilliseconds, false, ex.Message);
            _ = LogExceptionAsync(requestName, ex);

            throw;
        }
    }

    private async Task LogPerformanceAsync(string operationName, long durationMs, bool success, string? errorMessage)
    {
        try
        {
            await _logService.LogPerformanceAsync(new PerformanceLogEntry
            {
                OperationName = operationName,
                DurationMs = durationMs,
                OperationType = "MediatR",
                Success = success,
                Metadata = errorMessage != null
                    ? new Dictionary<string, object> { ["ErrorMessage"] = errorMessage }
                    : null
            });
        }
        catch
        {
            // Log hatası ana akışı etkilememeli
        }
    }

    private async Task LogExceptionAsync(string methodName, Exception ex)
    {
        try
        {
            await _logService.LogExceptionAsync(new ExceptionLogEntry
            {
                ExceptionType = ex.GetType().FullName ?? "Unknown",
                ExceptionMessage = ex.Message,
                StackTrace = ex.StackTrace,
                InnerExceptionMessage = ex.InnerException?.Message,
                MethodName = methodName,
                LayerName = "Business",
                Severity = "Error"
            });
        }
        catch
        {
            // Log hatası ana akışı etkilememeli
        }
    }
}

