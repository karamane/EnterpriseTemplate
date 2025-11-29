using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Proxy.Core.Extensions;

/// <summary>
/// Proxy Core katmanı yapılandırma sınıfı
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterProxyCoreConfiguration
{
    /// <summary>
    /// Proxy Core servislerini register eder
    /// </summary>
    /// <example>
    /// services.RegisterProxyCore();
    /// </example>
    public static IServiceCollection RegisterProxyCore(this IServiceCollection services)
    {
        // Base proxy servisleri burada register edilebilir
        return services;
    }
}

