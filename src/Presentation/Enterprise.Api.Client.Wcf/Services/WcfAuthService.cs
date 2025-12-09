using System.Net.Http.Json;
using System.Text.Json;
using Enterprise.Api.Client.Wcf.Services.Contracts;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Infrastructure.CrossCutting.Services;
using Microsoft.AspNetCore.Http;

namespace Enterprise.Api.Client.Wcf.Services;

/// <summary>
/// WCF Authentication Service Implementation
/// IAuthenticationService kullanarak token işlemlerini gerçekleştirir
/// </summary>
public class WcfAuthService : IWcfAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogger<WcfAuthService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WcfAuthService(
        HttpClient httpClient,
        IAuthenticationService authService,
        IHttpContextAccessor httpContextAccessor,
        ICorrelationContext correlationContext,
        ILogger<WcfAuthService> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
        _correlationContext = correlationContext;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<WcfLoginResponse> LoginAsync(WcfLoginRequest request)
    {
        try
        {
            _logger.LogInformation("[{CorrelationId}] WCF Login attempt for user {Username}",
                _correlationContext.CorrelationId, request.Username);

            var result = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (!result.Success)
            {
                _logger.LogWarning("[{CorrelationId}] WCF Login failed for user {Username}",
                    _correlationContext.CorrelationId, request.Username);

                return new WcfLoginResponse
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage ?? "Giriş başarısız"
                };
            }

            _logger.LogInformation("[{CorrelationId}] WCF Login successful for user {Username}",
                _correlationContext.CorrelationId, request.Username);

            return new WcfLoginResponse
            {
                Success = true,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt,
                Username = result.User?.Username,
                Roles = result.User?.Roles?.ToArray()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] WCF Login error for user {Username}",
                _correlationContext.CorrelationId, request.Username);

            return new WcfLoginResponse
            {
                Success = false,
                ErrorMessage = "Beklenmeyen bir hata oluştu"
            };
        }
    }

    public async Task<WcfLoginResponse> RefreshTokenAsync(WcfRefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!result.Success)
            {
                return new WcfLoginResponse
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage ?? "Token yenileme başarısız"
                };
            }

            return new WcfLoginResponse
            {
                Success = true,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt,
                Username = result.User?.Username,
                Roles = result.User?.Roles?.ToArray()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] WCF RefreshToken error", _correlationContext.CorrelationId);

            return new WcfLoginResponse
            {
                Success = false,
                ErrorMessage = "Beklenmeyen bir hata oluştu"
            };
        }
    }

    public async Task<WcfLogoutResponse> LogoutAsync(WcfLogoutRequest request)
    {
        try
        {
            await _authService.RevokeTokenAsync(request.AccessToken);

            return new WcfLogoutResponse
            {
                Success = true,
                Message = "Çıkış başarılı"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] WCF Logout error", _correlationContext.CorrelationId);

            return new WcfLogoutResponse
            {
                Success = false,
                Message = "Çıkış sırasında hata oluştu"
            };
        }
    }

    public async Task<WcfValidateTokenResponse> ValidateTokenAsync(WcfValidateTokenRequest request)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(request.AccessToken);

            if (!isValid)
            {
                return new WcfValidateTokenResponse
                {
                    IsValid = false
                };
            }

            // Token'dan user bilgilerini çıkar
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(request.AccessToken);

            var username = token.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
            var roles = token.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();

            return new WcfValidateTokenResponse
            {
                IsValid = true,
                Username = username,
                Roles = roles
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] WCF ValidateToken error", _correlationContext.CorrelationId);

            return new WcfValidateTokenResponse
            {
                IsValid = false
            };
        }
    }
}

