using Enterprise.Core.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Wcf.Controllers;

/// <summary>
/// WCF Client API temel controller
/// </summary>
[ApiController]
[Route("api/wcf/[controller]")]
[Produces("application/json")]
[Authorize] // Tüm endpoint'ler için authentication gerekli
public abstract class BaseWcfApiController : ControllerBase
{
    protected readonly ICorrelationContext CorrelationContext;

    protected BaseWcfApiController(ICorrelationContext correlationContext)
    {
        CorrelationContext = correlationContext;
    }

    /// <summary>
    /// Başarılı yanıt döner
    /// </summary>
    protected ActionResult<WcfApiResponse<T>> Success<T>(T data, string? message = null)
    {
        return Ok(new WcfApiResponse<T>
        {
            Success = true,
            Message = message ?? "İşlem başarılı",
            Data = data,
            CorrelationId = CorrelationContext.CorrelationId
        });
    }

    /// <summary>
    /// Oluşturma başarılı yanıtı döner
    /// </summary>
    protected ActionResult<WcfApiResponse<T>> Created<T>(T data, string? message = null)
    {
        return StatusCode(201, new WcfApiResponse<T>
        {
            Success = true,
            Message = message ?? "Kayıt oluşturuldu",
            Data = data,
            CorrelationId = CorrelationContext.CorrelationId
        });
    }

    /// <summary>
    /// Silme başarılı yanıtı döner
    /// </summary>
    protected ActionResult<WcfApiResponse<object>> Deleted(string? message = null)
    {
        return Ok(new WcfApiResponse<object>
        {
            Success = true,
            Message = message ?? "Kayıt silindi",
            CorrelationId = CorrelationContext.CorrelationId
        });
    }

    /// <summary>
    /// Hata yanıtı döner
    /// </summary>
    protected WcfApiResponse<T> Error<T>(string message, string? errorCode = null)
    {
        return new WcfApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode ?? "ERROR",
            CorrelationId = CorrelationContext.CorrelationId
        };
    }
}

/// <summary>
/// WCF API Response wrapper
/// </summary>
public class WcfApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? CorrelationId { get; set; }
    public string? ErrorCode { get; set; }
}

