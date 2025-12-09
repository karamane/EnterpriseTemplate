using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Enterprise.Infrastructure.CrossCutting.Services;

/// <summary>
/// Redis tabanlı token cache servisi
/// Token blacklist ve refresh token cache işlemleri
/// </summary>
public class RedisTokenCacheService : ITokenCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisTokenCacheService> _logger;
    private readonly IDatabase _db;

    private const string BlacklistKeyPrefix = "token:blacklist:";
    private const string RefreshTokenKeyPrefix = "token:refresh:";
    private const string RefreshTokenReverseKeyPrefix = "token:refresh_reverse:";
    private const string UserTokensKeyPrefix = "user:tokens:";

    public RedisTokenCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisTokenCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
        _db = _redis.GetDatabase();
    }

    /// <inheritdoc />
    public async Task BlacklistTokenAsync(string jti, TimeSpan expiry)
    {
        var key = $"{BlacklistKeyPrefix}{jti}";
        await _db.StringSetAsync(key, "revoked", expiry);
        _logger.LogDebug("Token {Jti} added to blacklist with expiry {Expiry}", jti, expiry);
    }

    /// <inheritdoc />
    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        var key = $"{BlacklistKeyPrefix}{jti}";
        return await _db.KeyExistsAsync(key);
    }

    /// <inheritdoc />
    public async Task SetRefreshTokenAsync(string userId, string token, TimeSpan expiry)
    {
        var key = $"{RefreshTokenKeyPrefix}{userId}";
        await _db.StringSetAsync(key, token, expiry);

        // Reverse mapping: token -> userId (for lookup by token)
        var reverseKey = $"{RefreshTokenReverseKeyPrefix}{token}";
        await _db.StringSetAsync(reverseKey, userId, expiry);

        // Kullanıcının token'ını user tokens set'ine ekle
        var userTokensKey = $"{UserTokensKeyPrefix}{userId}";
        await _db.SetAddAsync(userTokensKey, token);
        await _db.KeyExpireAsync(userTokensKey, expiry);

        _logger.LogDebug("Refresh token cached for user {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task<string?> GetRefreshTokenAsync(string userId)
    {
        var key = $"{RefreshTokenKeyPrefix}{userId}";
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    /// <inheritdoc />
    public async Task RemoveRefreshTokenAsync(string userId)
    {
        var key = $"{RefreshTokenKeyPrefix}{userId}";
        await _db.KeyDeleteAsync(key);
        _logger.LogDebug("Refresh token removed from cache for user {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(string userId)
    {
        // Refresh token cache'ini sil
        var refreshKey = $"{RefreshTokenKeyPrefix}{userId}";
        await _db.KeyDeleteAsync(refreshKey);

        // User tokens set'indeki tüm token'ları blacklist'e ekle
        var userTokensKey = $"{UserTokensKeyPrefix}{userId}";
        var tokens = await _db.SetMembersAsync(userTokensKey);

        foreach (var token in tokens)
        {
            if (token.HasValue)
            {
                var blacklistKey = $"{BlacklistKeyPrefix}{token}";
                await _db.StringSetAsync(blacklistKey, "revoked", TimeSpan.FromDays(7));
            }
        }

        // User tokens set'ini sil
        await _db.KeyDeleteAsync(userTokensKey);

        _logger.LogInformation("All tokens revoked for user {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task<string?> GetUserIdByRefreshTokenAsync(string refreshToken)
    {
        var key = $"{RefreshTokenReverseKeyPrefix}{refreshToken}";
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }
}


