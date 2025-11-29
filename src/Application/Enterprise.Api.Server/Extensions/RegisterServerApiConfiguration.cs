using AutoMapper;
using Enterprise.Api.Server.Mapping;
using Enterprise.Business.Extensions;
using Enterprise.Infrastructure.Caching.Extensions;
using Enterprise.Infrastructure.CrossCutting.Extensions;
using Enterprise.Infrastructure.Logging.Extensions;
using Enterprise.Infrastructure.Persistence.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Api.Server.Extensions;

/// <summary>
/// Server API katmanı yapılandırma sınıfı
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterServerApiConfiguration
{
    /// <summary>
    /// Server API katmanı servislerini register eder (sadece API katmanı)
    /// </summary>
    /// <example>
    /// services.RegisterServerApi();
    /// </example>
    public static IServiceCollection RegisterServerApi(this IServiceCollection services)
    {
        var assembly = typeof(RegisterServerApiConfiguration).Assembly;

        // AutoMapper - Server API katmanı mapping profili
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ServerApiMappingProfile>();
        }, assembly);

        // FluentValidation - Server API katmanı validator'ları
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    /// <summary>
    /// Server API'yi tüm bağımlılıklarıyla register eder
    /// Tek satırda tüm altyapıyı dahil eder
    /// </summary>
    /// <example>
    /// services.RegisterEnterpriseServerApi(configuration);
    /// </example>
    public static IServiceCollection RegisterEnterpriseServerApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure katmanları
        services.RegisterLogging(configuration);
        services.RegisterPersistence(configuration);
        services.RegisterCaching(configuration);
        services.RegisterCrossCutting(configuration);

        // Business katmanı (Application dahil)
        services.RegisterEnterpriseBusiness();

        // Server API katmanı
        services.RegisterServerApi();

        return services;
    }
}

