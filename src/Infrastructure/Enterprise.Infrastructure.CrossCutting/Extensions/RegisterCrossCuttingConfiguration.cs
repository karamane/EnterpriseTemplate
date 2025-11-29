using Enterprise.Infrastructure.CrossCutting.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Infrastructure.CrossCutting.Extensions;

/// <summary>
/// Cross-cutting servisleri yapılandırma sınıfı
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterCrossCuttingConfiguration
{
    /// <summary>
    /// Cross-cutting servislerini DI container'a register eder
    /// </summary>
    /// <example>
    /// services.RegisterCrossCutting(configuration);
    /// </example>
    public static IServiceCollection RegisterCrossCutting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Input sanitizer
        services.AddSingleton<IInputSanitizer, InputSanitizer>();

        return services;
    }
}

