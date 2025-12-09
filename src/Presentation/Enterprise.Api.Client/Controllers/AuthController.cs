using Enterprise.Api.Client.DTOs;
using Enterprise.Api.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Controllers;

/// <summary>
/// Client API Authentication Controller
/// Server API'ye proxy yapar - DMZ'de çalışır
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IServerApiClient _serverApiClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IServerApiClient serverApiClient,
        ILogger<AuthController> logger)
    {
        _serverApiClient = serverApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı girişi yapar
    /// </summary>
    /// <remarks>
    /// Demo kullanıcılar:
    /// - admin / Admin123! (Admin, User rolleri)
    /// - user / User123! (User rolü)
    /// </remarks>
    /// <param name="request">Kullanıcı adı ve şifre</param>
    /// <returns>JWT token bilgileri</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginClientRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { error = "Kullanıcı adı ve şifre gereklidir" });
        }

        var serverResponse = await _serverApiClient.PostAsync<LoginClientRequest, ServerAuthResponse>(
            "api/v1/auth/login",
            request);

        if (serverResponse == null || !serverResponse.Success || serverResponse.Data == null)
        {
            _logger.LogWarning("Login failed for user {Username}", request.Username);
            return Unauthorized(new { error = "Kullanıcı adı veya şifre hatalı" });
        }

        var data = serverResponse.Data;
        var response = new LoginClientResponse(
            data.AccessToken,
            data.RefreshToken,
            data.ExpiresAt,
            data.User?.Username ?? request.Username,
            data.User?.Roles.ToArray() ?? Array.Empty<string>());

        return Ok(response);
    }

    /// <summary>
    /// Token yenileme
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Yeni JWT token bilgileri</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenClientRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token gereklidir" });
        }

        var serverResponse = await _serverApiClient.PostAsync<RefreshTokenClientRequest, ServerAuthResponse>(
            "api/v1/auth/refresh",
            request);

        if (serverResponse == null || !serverResponse.Success || serverResponse.Data == null)
        {
            return Unauthorized(new { error = "Geçersiz veya süresi dolmuş refresh token" });
        }

        var data = serverResponse.Data;
        var response = new LoginClientResponse(
            data.AccessToken,
            data.RefreshToken,
            data.ExpiresAt,
            data.User?.Username ?? string.Empty,
            data.User?.Roles.ToArray() ?? Array.Empty<string>());

        return Ok(response);
    }

    /// <summary>
    /// Çıkış yapar - token'ı iptal eder
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        // Authorization header'dan token'ı al ve Server API'ye ilet
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        
        if (!string.IsNullOrEmpty(token))
        {
            // Server API'ye logout isteği gönder
            await _serverApiClient.PostAsync<object, object>(
                "api/v1/auth/logout",
                new { });
        }

        return Ok(new { message = "Çıkış başarılı" });
    }
}
