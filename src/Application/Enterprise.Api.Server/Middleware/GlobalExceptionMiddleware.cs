using System.Net;
using System.Text.Json;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Models.Logging;
using Enterprise.Core.Shared.Constants;
using Enterprise.Core.Shared.Exceptions;

namespace Enterprise.Api.Server.Middleware;

/// <summary>
/// Global exception handling middleware
/// Tüm unhandled exception'ları yakalar ve loglar
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICorrelationContext correlationContext,
        ILogService logService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, correlationContext, logService);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ICorrelationContext correlationContext,
        ILogService logService)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                validationEx.ErrorCode,
                validationEx.Message),

            BusinessException businessEx => (
                HttpStatusCode.UnprocessableEntity,
                businessEx.ErrorCode,
                businessEx.UserFriendlyMessage ?? businessEx.Message),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.ErrorCode,
                notFoundEx.Message),

            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                unauthorizedEx.ErrorCode,
                unauthorizedEx.Message),

            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                forbiddenEx.ErrorCode,
                forbiddenEx.Message),

            ExternalServiceException externalEx => (
                HttpStatusCode.BadGateway,
                externalEx.ErrorCode,
                "Dış servis hatası oluştu."),

            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "Beklenmeyen bir hata oluştu.")
        };

        // Exception log
        if (exception is BusinessException businessException)
        {
            var businessLog = BusinessExceptionLogEntry.FromBusinessException(
                businessException,
                correlationContext.CorrelationId,
                LogConstants.Layers.ServerApi);

            businessLog.RequestPath = context.Request.Path;
            businessLog.HttpMethod = context.Request.Method;

            await logService.LogBusinessExceptionAsync(businessLog);
        }
        else
        {
            var exceptionLog = ExceptionLogEntry.FromException(
                exception,
                LogConstants.Layers.ServerApi,
                correlationContext.CorrelationId);

            exceptionLog.RequestPath = context.Request.Path;
            exceptionLog.HttpMethod = context.Request.Method;
            exceptionLog.IsHandled = true;

            await logService.LogExceptionAsync(exceptionLog);
        }

        _logger.LogError(exception,
            "[{CorrelationId}] {ErrorCode}: {Message}",
            correlationContext.CorrelationId, errorCode, message);

        // Response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            CorrelationId = correlationContext.CorrelationId,
            Timestamp = DateTime.UtcNow
        };

        // Validation errors ekle
        if (exception is ValidationException validationException)
        {
            response.Errors = validationException.Errors;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// API error response modeli
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}

