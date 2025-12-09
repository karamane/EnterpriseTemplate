using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Auth;
using Enterprise.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.Infrastructure.Persistence.Repositories.EfCore;

/// <summary>
/// User repository EF Core implementasyonu
/// </summary>
public class UserRepository : EfCoreRepository<User, long>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetWithRefreshTokensAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
    }
}


