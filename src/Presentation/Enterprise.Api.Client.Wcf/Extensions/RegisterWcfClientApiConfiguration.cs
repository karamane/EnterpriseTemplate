using AutoMapper;
using Enterprise.Api.Client.Wcf.Mapping;
using Enterprise.Api.Client.Wcf.Middleware;
using Enterprise.Api.Client.Wcf.Services;
using Enterprise.Api.Client.Wcf.Services.Contracts;
using Enterprise.Infrastructure.CrossCutting.Extensions;
using Enterprise.Infrastructure.Logging.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Polly;
using Polly.Extensions.Http;

namespace Enterprise.Api.Client.Wcf.Extensions;

/// <summary>
/// WCF Client API yapılandırma sınıfı
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterWcfClientApiConfiguration
{
    /// <summary>
    /// WCF Client API servislerini DI container'a register eder
    /// </summary>
    /// <example>
    /// services.RegisterWcfClientApi(configuration);
    /// </example>
    public static IServiceCollection RegisterWcfClientApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HttpContextAccessor (Authorization header propagation için)
        services.AddHttpContextAccessor();

        // AutoMapper
        services.AddAutoMapper(typeof(WcfClientMappingProfile));

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<WcfClientMappingProfile>();

        // Server API Client with Resilience (DMZ kuralı: Sadece Server API tüketilir!)
        services.RegisterServerApiClient(configuration);

        // CoreWCF Services (SOAP endpoints)
        services.RegisterWcfServices(configuration);

        // Logging
        services.RegisterLogging(configuration);

        // JWT Authentication (CrossCutting'den)
        services.RegisterCrossCutting(configuration);

        // Health Checks
        services.AddHealthChecks();

        return services;
    }

    /// <summary>
    /// CoreWCF SOAP servislerini register eder
    /// </summary>
    private static IServiceCollection RegisterWcfServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serverApiUrl = configuration["ServerApi:BaseUrl"] ?? "https://localhost:5101";

        // Retry policy for WCF services
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Circuit breaker policy
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        // WcfCustomerService with HttpClient
        services.AddHttpClient<WcfCustomerService>(client =>
        {
            client.BaseAddress = new Uri(serverApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        // WcfAuthService with HttpClient
        services.AddHttpClient<WcfAuthService>(client =>
        {
            client.BaseAddress = new Uri(serverApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }

    /// <summary>
    /// Server API client'ı resilience ile register eder
    /// </summary>
    private static IServiceCollection RegisterServerApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serverApiUrl = configuration["ServerApi:BaseUrl"] ?? "https://localhost:5101";

        // Retry policy
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Circuit breaker policy
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IWcfServerApiClient, WcfServerApiClient>(client =>
        {
            client.BaseAddress = new Uri(serverApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }

    /// <summary>
    /// WCF Client API middleware'lerini pipeline'a register eder
    /// </summary>
    public static IApplicationBuilder UseWcfClientApi(this IApplicationBuilder app)
    {
        // Basic Auth to Bearer conversion (JWT authentication'dan önce)
        app.UseBasicToBearer();

        // Logging middleware
        app.UseLogging();

        return app;
    }
}
