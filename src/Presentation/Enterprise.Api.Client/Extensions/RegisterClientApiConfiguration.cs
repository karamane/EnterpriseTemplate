using AspNetCoreRateLimit;
using AutoMapper;
using Enterprise.Api.Client.Mapping;
using Enterprise.Api.Client.Services;
using Enterprise.Infrastructure.CrossCutting.Extensions;
using Enterprise.Infrastructure.Logging.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Enterprise.Api.Client.Extensions;

/// <summary>
/// Client API katmanı yapılandırma sınıfı
/// Client API tamamen izole - hiçbir internal API referansı yok
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterClientApiConfiguration
{
    /// <summary>
    /// Client API katmanı servislerini register eder (sadece API katmanı)
    /// </summary>
    /// <example>
    /// services.RegisterClientApi();
    /// </example>
    public static IServiceCollection RegisterClientApi(this IServiceCollection services)
    {
        var assembly = typeof(RegisterClientApiConfiguration).Assembly;

        // AutoMapper - Client API katmanı mapping profili
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ClientApiMappingProfile>();
        }, assembly);

        // FluentValidation - Client API katmanı validator'ları
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    /// <summary>
    /// Client API'yi tüm bağımlılıklarıyla register eder
    /// Tek satırda tüm altyapıyı dahil eder
    /// Client API tamamen izole - Server API referansı yok
    /// </summary>
    /// <example>
    /// services.RegisterEnterpriseClientApi(configuration);
    /// </example>
    public static IServiceCollection RegisterEnterpriseClientApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure katmanları (sadece gerekli olanlar)
        services.RegisterLogging(configuration);
        services.RegisterCrossCutting(configuration);

        // Rate Limiting (DDoS Protection)
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        // Server API Client with Resilience
        services.RegisterServerApiClient(configuration);

        // Client API katmanı
        services.RegisterClientApi();

        return services;
    }

    /// <summary>
    /// Server API client'ı resilience ile register eder
    /// </summary>
    private static IServiceCollection RegisterServerApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serverApiUrl = configuration["ServerApi:BaseUrl"] ?? "https://localhost:7001";

        // Retry policy
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Circuit breaker policy
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IServerApiClient, ServerApiClient>(client =>
        {
            client.BaseAddress = new Uri(serverApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}

