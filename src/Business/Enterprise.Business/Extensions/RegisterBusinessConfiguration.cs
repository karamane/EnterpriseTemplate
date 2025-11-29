using AutoMapper;
using Enterprise.Business.Mapping;
using Enterprise.Core.Application.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Business.Extensions;

/// <summary>
/// Business katmanı yapılandırma sınıfı
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterBusinessConfiguration
{
    /// <summary>
    /// Business katmanı servislerini register eder
    /// </summary>
    /// <example>
    /// services.RegisterBusiness();
    /// </example>
    public static IServiceCollection RegisterBusiness(this IServiceCollection services)
    {
        var assembly = typeof(RegisterBusinessConfiguration).Assembly;

        // MediatR - Business handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // AutoMapper - Business katmanı mapping profili
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<BusinessMappingProfile>();
        }, assembly);

        // FluentValidation - Business katmanı validator'ları
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }

    /// <summary>
    /// Business katmanını tüm bağımlılıklarıyla register eder
    /// Application katmanını da dahil eder
    /// </summary>
    /// <example>
    /// services.RegisterEnterpriseBusiness();
    /// </example>
    public static IServiceCollection RegisterEnterpriseBusiness(this IServiceCollection services)
    {
        // Application katmanı (bağımlılık)
        services.RegisterApplication();

        // Business katmanı
        services.RegisterBusiness();

        return services;
    }
}

