using AutoMapper;
using Enterprise.Core.Application.Behaviors;
using Enterprise.Core.Application.Mapping;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Core.Application.Extensions;

/// <summary>
/// Application katmanı yapılandırma sınıfı
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterApplicationConfiguration
{
    /// <summary>
    /// Application katmanı servislerini register eder
    /// </summary>
    /// <example>
    /// services.RegisterApplication();
    /// </example>
    public static IServiceCollection RegisterApplication(this IServiceCollection services)
    {
        var assembly = typeof(RegisterApplicationConfiguration).Assembly;

        // AutoMapper - Application katmanı mapping profili
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ApplicationMappingProfile>();
        }, assembly);

        // FluentValidation - Application katmanı validator'ları
        services.AddValidatorsFromAssembly(assembly);

        // MediatR Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        return services;
    }
}

