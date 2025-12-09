using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Auth;
using Enterprise.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.Infrastructure.Persistence.Repositories.EfCore;

/// <summary>
/// RefreshToken repository EF Core implementasyonu
/// </summary>
public class RefreshTokenRepository : EfCoreRepository<RefreshToken, long>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(long userId, string? revokedByIp = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        var activeTokens = await DbSet
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(revokedByIp, reason ?? "Logout from all devices");
        }
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await DbSet
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow.AddDays(-30)) // 30 günden eski
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(expiredTokens);
        return expiredTokens.Count;
    }
}


