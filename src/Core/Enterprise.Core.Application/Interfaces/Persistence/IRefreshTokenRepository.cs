using Enterprise.Core.Domain.Entities.Auth;

namespace Enterprise.Core.Application.Interfaces.Persistence;

/// <summary>
/// RefreshToken repository interface
/// Token yönetimi işlemleri için özelleştirilmiş metodlar
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken, long>
{
    /// <summary>
    /// Token değerine göre refresh token getirir
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının tüm aktif refresh token'larını getirir
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının tüm refresh token'larını iptal eder
    /// </summary>
    Task RevokeAllUserTokensAsync(long userId, string? revokedByIp = null, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Süresi dolmuş token'ları temizler
    /// </summary>
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}


