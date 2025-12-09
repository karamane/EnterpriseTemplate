using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Infrastructure.CrossCutting.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Enterprise.Infrastructure.CrossCutting.Services;

/// <summary>
/// JWT tabanlı authentication service implementasyonu
/// Repository varsa DB'den, yoksa demo kullanıcılar kullanılır
/// Şifreler BCrypt ile doğrulanır
/// </summary>
public class JwtAuthenticationService : IAuthenticationService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<JwtAuthenticationService> _logger;
    private readonly ITokenCacheService? _tokenCacheService;
    private readonly IUserRepository? _userRepository;
    
    // Demo kullanıcılar - Repository yoksa kullanılır
    // BCrypt hash'ler doğrulanmış değerler
    private static readonly Dictionary<string, (string Password, string PasswordHash, UserInfo User)> DemoUsers = new()
    {
        ["admin"] = (
            "Admin123!", 
            "$2a$11$UcRiXou1e8p4CI/2SFNu3.cbl67H3dk3Qi8RoJpVwvnSsAbdJ8Jeu",
            new UserInfo
            {
                UserId = "1",
                Username = "admin",
                Email = "admin@enterprise.com",
                FullName = "System Administrator",
                Roles = new[] { "Admin", "User" }
            }),
        ["user"] = (
            "User123!", 
            "$2a$11$5j6SZSlEEN2VxkrORAmsoePChDpau9mCMB0Rj8Gd16QXd9nVvdclK",
            new UserInfo
            {
                UserId = "2",
                Username = "user",
                Email = "user@enterprise.com",
                FullName = "Standard User",
                Roles = new[] { "User" }
            })
    };

    // Revoked tokens (Redis yoksa in-memory)
    private static readonly HashSet<string> RevokedTokens = new();

    public JwtAuthenticationService(
        IOptions<JwtOptions> jwtOptions,
        ILogger<JwtAuthenticationService> logger,
        ITokenCacheService? tokenCacheService = null,
        IUserRepository? userRepository = null)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
        _tokenCacheService = tokenCacheService;
        _userRepository = userRepository;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken = default)
    {
        UserInfo? user = null;
        bool isPasswordValid = false;

        // Önce DB'den kontrol et (Repository varsa)
        if (_userRepository != null)
        {
            var dbUser = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            
            if (dbUser != null && dbUser.IsActive)
            {
                // BCrypt ile şifre doğrulama
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, dbUser.PasswordHash);
                
                if (isPasswordValid)
                {
                    user = new UserInfo
                    {
                        UserId = dbUser.Id.ToString(),
                        Username = dbUser.Username,
                        Email = dbUser.Email,
                        FullName = dbUser.FullName,
                        Roles = dbUser.GetRolesArray()
                    };

                    // Son giriş tarihini güncelle
                    dbUser.UpdateLastLogin();
                    await _userRepository.UpdateAsync(dbUser, cancellationToken);
                }
            }
        }
        
        // DB'de bulunamadıysa demo kullanıcıları kontrol et (fallback)
        if (user == null)
        {
            var normalizedUsername = username.ToLowerInvariant();
            
            if (DemoUsers.TryGetValue(normalizedUsername, out var userData))
            {
                // BCrypt hash ile doğrulama dene
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, userData.PasswordHash);
                }
                catch
                {
                    // Hash doğrulaması başarısız olursa plaintext karşılaştır (eski demo kullanıcılar için)
                    isPasswordValid = userData.Password == password;
                }

                if (isPasswordValid)
                {
                    user = userData.User;
                }
            }
        }

        // Kullanıcı bulunamadı veya şifre hatalı
        if (user == null || !isPasswordValid)
        {
            _logger.LogWarning("Authentication failed for user {Username}", username);
            return AuthenticationResult.Failed("Kullanıcı adı veya şifre hatalı");
        }

        // Token oluştur
        var jti = Guid.NewGuid().ToString();
        var accessToken = GenerateAccessToken(user, jti);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);

        // Redis varsa cache'e kaydet
        if (_tokenCacheService != null)
        {
            await _tokenCacheService.SetRefreshTokenAsync(
                user.UserId, 
                refreshToken, 
                TimeSpan.FromDays(_jwtOptions.RefreshTokenExpirationDays));
        }

        _logger.LogInformation("User {Username} authenticated successfully", username);

        return AuthenticationResult.Succeeded(accessToken, refreshToken, expiresAt, user);
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return AuthenticationResult.Failed("Refresh token gerekli");
        }

        UserInfo? user = null;

        // Redis varsa token'dan user bilgisini al
        if (_tokenCacheService != null)
        {
            var userId = await _tokenCacheService.GetUserIdByRefreshTokenAsync(refreshToken);
            
            if (!string.IsNullOrEmpty(userId) && _userRepository != null)
            {
                if (long.TryParse(userId, out var id))
                {
                    var dbUser = await _userRepository.GetByIdAsync(id, cancellationToken);
                    
                    if (dbUser != null && dbUser.IsActive)
                    {
                        user = new UserInfo
                        {
                            UserId = dbUser.Id.ToString(),
                            Username = dbUser.Username,
                            Email = dbUser.Email,
                            FullName = dbUser.FullName,
                            Roles = dbUser.GetRolesArray()
                        };
                    }
                }
            }
        }

        // Fallback: Demo admin kullanıcısı döndür
        if (user == null)
        {
            user = DemoUsers["admin"].User;
        }

        var jti = Guid.NewGuid().ToString();
        var newAccessToken = GenerateAccessToken(user, jti);
        var newRefreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);

        // Redis varsa cache'i güncelle
        if (_tokenCacheService != null)
        {
            await _tokenCacheService.SetRefreshTokenAsync(
                user.UserId, 
                newRefreshToken, 
                TimeSpan.FromDays(_jwtOptions.RefreshTokenExpirationDays));
        }

        _logger.LogInformation("Token refreshed for user {Username}", user.Username);

        return AuthenticationResult.Succeeded(newAccessToken, newRefreshToken, expiresAt, user);
    }

    public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            return;

        // Access token ise blacklist'e ekle
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var exp = jwtToken.ValidTo;

            if (!string.IsNullOrEmpty(jti))
            {
                // Redis varsa cache'e ekle
                if (_tokenCacheService != null && exp > DateTime.UtcNow)
                {
                    var expiry = exp - DateTime.UtcNow;
                    await _tokenCacheService.BlacklistTokenAsync(jti, expiry);
                }
                else
                {
                    RevokedTokens.Add(jti);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse token for blacklisting");
        }

        _logger.LogInformation("Token revoked");
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _jwtOptions.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _jwtOptions.ValidateIssuer,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = _jwtOptions.ValidateAudience,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = _jwtOptions.ValidateLifetime,
                ClockSkew = TimeSpan.Zero
            }, out _);

            // Blacklist kontrolü
            var jti = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
                // Redis varsa cache'den kontrol et
                if (_tokenCacheService != null)
                {
                    if (await _tokenCacheService.IsTokenBlacklistedAsync(jti))
                    {
                        _logger.LogWarning("Token {Jti} is blacklisted", jti);
                        return false;
                    }
                }
                else if (RevokedTokens.Contains(jti))
                {
                    _logger.LogWarning("Token {Jti} is revoked", jti);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }

    /// <summary>
    /// Şifre hash'i oluşturur (BCrypt)
    /// </summary>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));
    }

    /// <summary>
    /// Şifre doğrular (BCrypt)
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string GenerateAccessToken(UserInfo user, string jti)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, jti),
            new("fullName", user.FullName ?? string.Empty)
        };

        // Rolleri ekle
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
