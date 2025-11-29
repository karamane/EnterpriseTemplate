using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Proxy.Core.Base;
using Microsoft.Extensions.Logging;

namespace Enterprise.Proxy.ExternalService;

/// <summary>
/// Örnek dış servis proxy implementasyonu
/// Geliştiriciler için referans
/// </summary>
public interface ISampleExternalServiceProxy
{
    /// <summary>
    /// Kullanıcı bilgisi getirir
    /// </summary>
    Task<ExternalUserDto?> GetUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcı oluşturur
    /// </summary>
    Task<ExternalUserDto?> CreateUserAsync(CreateExternalUserRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sample external service proxy implementasyonu
/// </summary>
public class SampleExternalServiceProxy : BaseHttpProxy<SampleExternalServiceProxy>, ISampleExternalServiceProxy
{
    protected override string ServiceName => "SampleExternalService";

    public SampleExternalServiceProxy(
        HttpClient httpClient,
        ILogger<SampleExternalServiceProxy> logger,
        ILogService logService,
        ICorrelationContext correlationContext)
        : base(httpClient, logger, logService, correlationContext)
    {
    }

    public async Task<ExternalUserDto?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<ExternalUserDto>($"/users/{userId}", cancellationToken);
    }

    public async Task<ExternalUserDto?> CreateUserAsync(
        CreateExternalUserRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<CreateExternalUserRequest, ExternalUserDto>("/users", request, cancellationToken);
    }
}

#region DTOs

/// <summary>
/// External user DTO
/// </summary>
public record ExternalUserDto(
    int Id,
    string Name,
    string Email,
    string Phone);

/// <summary>
/// Create external user request
/// </summary>
public record CreateExternalUserRequest(
    string Name,
    string Email,
    string Phone);

#endregion

