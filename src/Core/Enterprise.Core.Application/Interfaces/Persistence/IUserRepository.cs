using Enterprise.Core.Domain.Entities.Auth;

namespace Enterprise.Core.Application.Interfaces.Persistence;

/// <summary>
/// User repository interface
/// Authentication ve user management işlemleri için özelleştirilmiş metodlar
/// </summary>
public interface IUserRepository : IRepository<User, long>
{
    /// <summary>
    /// Kullanıcı adına göre kullanıcı getirir
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Email'e göre kullanıcı getirir
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcı adı kullanılıyor mu kontrol eder
    /// </summary>
    Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Email kullanılıyor mu kontrol eder
    /// </summary>
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcıyı refresh token'ları ile birlikte getirir
    /// </summary>
    Task<User?> GetWithRefreshTokensAsync(long userId, CancellationToken cancellationToken = default);
}


