using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Api.Client.Wcf.Services;
using Enterprise.Core.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Wcf.Controllers;

/// <summary>
/// WCF Client API Authentication Controller
/// Basic Authentication ve Bearer Token desteği
/// </summary>
[ApiController]
[Route("api/wcf/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IWcfServerApiClient _serverApiClient;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IWcfServerApiClient serverApiClient,
        ICorrelationContext correlationContext,
        ILogger<AuthController> logger)
    {
        _serverApiClient = serverApiClient;
        _correlationContext = correlationContext;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı girişi yapar (Basic Auth veya JSON body)
    /// </summary>
    /// <remarks>
    /// İki yöntemle kullanılabilir:
    /// 1. JSON Body: {"username": "admin", "password": "Admin123!"}
    /// 2. Basic Auth Header: Authorization: Basic YWRtaW46QWRtaW4xMjMh
    /// 
    /// Demo kullanıcılar:
    /// - admin / Admin123! (Admin, User rolleri)
    /// - user / User123! (User rolü)
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WcfApiResponse<LoginWcfResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginWcfRequest? request = null)
    {
        // Basic Auth header'dan credentials al (body boşsa)
        if (request == null || string.IsNullOrEmpty(request.Username))
        {
            var authHeader = Request.Headers.Authorization.ToString();
            if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                    var credentialBytes = Convert.FromBase64String(encodedCredentials);
                    var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                    if (credentials.Length == 2)
                    {
                        request = new LoginWcfRequest(credentials[0], credentials[1]);
                    }
                }
                catch
                {
                    return Unauthorized(CreateErrorResponse("Invalid Basic Auth header"));
                }
            }
        }

        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(CreateErrorResponse("Kullanıcı adı ve şifre gereklidir"));
        }

        var response = await _serverApiClient.AuthenticateAsync(request);

        if (response == null)
        {
            _logger.LogWarning("Login failed for user {Username}", request.Username);
            return Unauthorized(CreateErrorResponse("Kullanıcı adı veya şifre hatalı"));
        }

        return Ok(new WcfApiResponse<LoginWcfResponse>
        {
            Success = true,
            Message = "Giriş başarılı",
            Data = response,
            CorrelationId = _correlationContext.CorrelationId
        });
    }

    /// <summary>
    /// Token yenileme
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WcfApiResponse<LoginWcfResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenWcfRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(CreateErrorResponse("Refresh token gereklidir"));
        }

        var response = await _serverApiClient.RefreshTokenAsync(request);

        if (response == null)
        {
            return Unauthorized(CreateErrorResponse("Geçersiz veya süresi dolmuş refresh token"));
        }

        return Ok(new WcfApiResponse<LoginWcfResponse>
        {
            Success = true,
            Message = "Token yenilendi",
            Data = response,
            CorrelationId = _correlationContext.CorrelationId
        });
    }

    /// <summary>
    /// Çıkış yapar
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(WcfApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        return Ok(new WcfApiResponse<object>
        {
            Success = true,
            Message = "Çıkış başarılı",
            CorrelationId = _correlationContext.CorrelationId
        });
    }

    private WcfApiResponse<object> CreateErrorResponse(string message)
    {
        return new WcfApiResponse<object>
        {
            Success = false,
            Message = message,
            ErrorCode = "AUTH_ERROR",
            CorrelationId = _correlationContext.CorrelationId
        };
    }
}


