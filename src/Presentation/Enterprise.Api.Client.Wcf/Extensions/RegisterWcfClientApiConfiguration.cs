using AutoMapper;
using Enterprise.Api.Client.Wcf.Mapping;
using Enterprise.Api.Client.Wcf.Services;
using Enterprise.Infrastructure.Logging.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;

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
        // Options
        services.Configure<WcfClientOptions>(configuration.GetSection(WcfClientOptions.SectionName));

        // AutoMapper
        services.AddAutoMapper(typeof(WcfClientMappingProfile));

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<WcfClientMappingProfile>();

        // WCF Clients
        services.AddScoped<ICustomerWcfClient, CustomerWcfClient>();
        services.AddScoped<IOrderWcfClient, OrderWcfClient>();

        // Logging
        services.RegisterLogging(configuration);

        return services;
    }

    /// <summary>
    /// WCF Client API middleware'lerini pipeline'a register eder
    /// </summary>
    public static IApplicationBuilder UseWcfClientApi(this IApplicationBuilder app)
    {
        // Logging middleware
        app.UseLogging();

        return app;
    }
}

