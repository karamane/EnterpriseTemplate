using Enterprise.Proxy.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Enterprise.Proxy.ExternalService.Extensions;

/// <summary>
/// External Service Proxy yapılandırma sınıfı
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterExternalServiceProxyConfiguration
{
    /// <summary>
    /// External Service Proxy'yi register eder
    /// </summary>
    /// <example>
    /// services.RegisterExternalServiceProxy(configuration);
    /// </example>
    public static IServiceCollection RegisterExternalServiceProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Proxy Core
        services.RegisterProxyCore();

        var baseUrl = configuration["ExternalServices:SampleService:BaseUrl"] ?? "https://api.example.com";
        var timeout = configuration.GetValue<int>("ExternalServices:SampleService:TimeoutSeconds", 30);

        // Retry policy
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Circuit breaker policy
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddHttpClient<ISampleExternalServiceProxy, SampleExternalServiceProxy>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout);
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}

