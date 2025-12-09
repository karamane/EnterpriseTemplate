using Enterprise.Api.Server.DTOs;
using Enterprise.Infrastructure.CrossCutting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Server.Controllers;

/// <summary>
/// Authentication API'si
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı girişi yapar ve JWT token döner
    /// </summary>
    /// <remarks>
    /// Demo kullanıcılar:
    /// - admin / Admin123! (Admin, User rolleri)
    /// - user / User123! (User rolü)
    /// </remarks>
    /// <param name="request">Login bilgileri</param>
    /// <returns>JWT token ve kullanıcı bilgileri</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.AuthenticateAsync(request.Username, request.Password);

        if (!result.Success)
        {
            return Unauthorized(new ApiErrorResponse
            {
                Success = false,
                ErrorCode = "AUTH_FAILED",
                Message = result.ErrorMessage ?? "Giriş başarısız"
            });
        }

        var response = new LoginResponse(
            result.AccessToken!,
            result.RefreshToken!,
            result.ExpiresAt!.Value,
            new UserInfoResponse(
                result.User!.UserId,
                result.User.Username,
                result.User.Email,
                result.User.FullName,
                result.User.Roles));

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Giriş başarılı",
            Data = response
        });
    }

    /// <summary>
    /// Refresh token ile yeni access token alır
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Yeni JWT token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
        {
            return Unauthorized(new ApiErrorResponse
            {
                Success = false,
                ErrorCode = "REFRESH_FAILED",
                Message = result.ErrorMessage ?? "Token yenileme başarısız"
            });
        }

        var response = new LoginResponse(
            result.AccessToken!,
            result.RefreshToken!,
            result.ExpiresAt!.Value,
            new UserInfoResponse(
                result.User!.UserId,
                result.User.Username,
                result.User.Email,
                result.User.FullName,
                result.User.Roles));

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Token yenilendi",
            Data = response
        });
    }

    /// <summary>
    /// Kullanıcı çıkışı yapar (token'ı geçersiz kılar)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        
        await _authService.RevokeTokenAsync(token);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Çıkış başarılı"
        });
    }

    /// <summary>
    /// Mevcut kullanıcı bilgilerini döner
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                     ?? User.FindFirst("sub")?.Value;
        var username = User.Identity?.Name 
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                       ?? User.FindFirst("unique_name")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value;
        var fullName = User.FindFirst("fullName")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        var userInfo = new UserInfoResponse(
            userId ?? string.Empty,
            username ?? string.Empty,
            email ?? string.Empty,
            fullName,
            roles);

        return Ok(new ApiResponse<UserInfoResponse>
        {
            Success = true,
            Data = userInfo
        });
    }
}


