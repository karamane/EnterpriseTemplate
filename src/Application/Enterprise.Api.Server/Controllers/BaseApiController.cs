using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Server.Controllers;

/// <summary>
/// Tüm API controller'ların base sınıfı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    private ICorrelationContext? _correlationContext;

    /// <summary>
    /// MediatR sender
    /// </summary>
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Correlation context
    /// </summary>
    protected ICorrelationContext CorrelationContext =>
        _correlationContext ??= HttpContext.RequestServices.GetRequiredService<ICorrelationContext>();

    /// <summary>
    /// Result'ı HTTP response'a çevirir
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data,
                CorrelationId = CorrelationContext.CorrelationId
            });
        }

        return BadRequest(new ApiResponse<T>
        {
            Success = false,
            Message = result.Message,
            ErrorCode = result.ErrorCode,
            Errors = result.Errors,
            CorrelationId = CorrelationContext.CorrelationId
        });
    }

    /// <summary>
    /// Created response döner
    /// </summary>
    protected IActionResult HandleCreated<T>(Result<T> result, string location)
    {
        if (result.IsSuccess)
        {
            return Created(location, new ApiResponse<T>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data,
                CorrelationId = CorrelationContext.CorrelationId
            });
        }

        return BadRequest(new ApiResponse<T>
        {
            Success = false,
            Message = result.Message,
            ErrorCode = result.ErrorCode,
            CorrelationId = CorrelationContext.CorrelationId
        });
    }
}

/// <summary>
/// Standart API response modeli
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// API hata response modeli
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public List<string>? Errors { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

