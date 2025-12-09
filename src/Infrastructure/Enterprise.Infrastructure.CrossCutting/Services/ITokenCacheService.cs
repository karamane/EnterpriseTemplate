namespace Enterprise.Infrastructure.CrossCutting.Services;

/// <summary>
/// Token cache servisi interface
/// Redis veya memory-based cache kullanabilir
/// </summary>
public interface ITokenCacheService
{
    /// <summary>
    /// Access token'ı blacklist'e ekler (logout/revoke için)
    /// </summary>
    /// <param name="jti">JWT Token ID (jti claim)</param>
    /// <param name="expiry">Token'ın kalan ömrü</param>
    Task BlacklistTokenAsync(string jti, TimeSpan expiry);

    /// <summary>
    /// Token blacklist'te mi kontrol eder
    /// </summary>
    /// <param name="jti">JWT Token ID</param>
    Task<bool> IsTokenBlacklistedAsync(string jti);

    /// <summary>
    /// Kullanıcının aktif refresh token'ını cache'e kaydeder
    /// </summary>
    Task SetRefreshTokenAsync(string userId, string token, TimeSpan expiry);

    /// <summary>
    /// Kullanıcının cache'deki refresh token'ını alır
    /// </summary>
    Task<string?> GetRefreshTokenAsync(string userId);

    /// <summary>
    /// Kullanıcının refresh token'ını cache'den siler
    /// </summary>
    Task RemoveRefreshTokenAsync(string userId);

    /// <summary>
    /// Kullanıcının tüm token'larını iptal eder (logout all devices)
    /// </summary>
    Task RevokeAllUserTokensAsync(string userId);

    /// <summary>
    /// Refresh token'dan kullanıcı ID'sini alır
    /// </summary>
    Task<string?> GetUserIdByRefreshTokenAsync(string refreshToken);
}


