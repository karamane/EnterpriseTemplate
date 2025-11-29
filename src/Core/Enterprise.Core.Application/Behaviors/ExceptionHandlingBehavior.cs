using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Models.Logging;
using Enterprise.Core.Shared.Constants;
using Enterprise.Core.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enterprise.Core.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior - Exception handling ve logging
/// Tüm exception'ları yakalar ve loglar
/// </summary>
/// <typeparam name="TRequest">Request tipi</typeparam>
/// <typeparam name="TResponse">Response tipi</typeparam>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogService _logService;

    public ExceptionHandlingBehavior(
        ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger,
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
        try
        {
            return await next();
        }
        catch (BusinessException ex)
        {
            // Business exception'ları ayrı logla
            await LogBusinessExceptionAsync(ex, request, cancellationToken);
            throw;
        }
        catch (ValidationException ex)
        {
            // Validation exception'ları ayrı logla
            await LogValidationExceptionAsync(ex, request, cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            // Sistem hatalarını logla
            await LogExceptionAsync(ex, request, cancellationToken);
            throw;
        }
    }

    private async Task LogBusinessExceptionAsync(
        BusinessException ex,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogWarning(ex,
            "[{CorrelationId}] Business exception in {RequestName}: {ErrorCode} - {Message}",
            _correlationContext.CorrelationId, requestName, ex.ErrorCode, ex.Message);

        var logEntry = BusinessExceptionLogEntry.FromBusinessException(
            ex,
            _correlationContext.CorrelationId,
            LogConstants.Layers.Business);

        logEntry.ClassName = requestName;
        logEntry.BusinessOperation = requestName;

        await _logService.LogBusinessExceptionAsync(logEntry, cancellationToken);
    }

    private async Task LogValidationExceptionAsync(
        ValidationException ex,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogWarning(ex,
            "[{CorrelationId}] Validation exception in {RequestName}: {Errors}",
            _correlationContext.CorrelationId, requestName, ex.Errors);

        var logEntry = new BusinessExceptionLogEntry
        {
            CorrelationId = _correlationContext.CorrelationId,
            Layer = LogConstants.Layers.Business,
            ClassName = requestName,
            BusinessOperation = requestName,
            BusinessErrorCode = ex.ErrorCode,
            BusinessErrorMessage = ex.Message,
            ValidationErrors = ex.Errors,
            ExceptionType = ex.GetType().FullName,
            ExceptionMessage = ex.Message,
            ExceptionCategory = LogConstants.ExceptionCategories.Validation
        };

        await _logService.LogBusinessExceptionAsync(logEntry, cancellationToken);
    }

    private async Task LogExceptionAsync(
        Exception ex,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogError(ex,
            "[{CorrelationId}] Unhandled exception in {RequestName}: {Message}",
            _correlationContext.CorrelationId, requestName, ex.Message);

        var logEntry = ExceptionLogEntry.FromException(
            ex,
            LogConstants.Layers.Business,
            _correlationContext.CorrelationId);

        logEntry.ClassName = requestName;
        logEntry.IsHandled = false;

        await _logService.LogExceptionAsync(logEntry, cancellationToken);
    }
}

