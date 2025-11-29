using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Shared.Helpers;
using Enterprise.Infrastructure.Logging.Context;
using Enterprise.Infrastructure.Logging.Middleware;
using Enterprise.Infrastructure.Logging.Services;
using Enterprise.Infrastructure.Logging.Sinks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Enterprise.Infrastructure.Logging.Extensions;

/// <summary>
/// Logging servisleri yapılandırma sınıfı
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterLoggingConfiguration
{
    /// <summary>
    /// Logging servislerini DI container'a register eder
    /// </summary>
    /// <example>
    /// services.RegisterLogging(configuration);
    /// </example>
    public static IServiceCollection RegisterLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<LoggingOptions>(configuration.GetSection(LoggingOptions.SectionName));
        services.Configure<Helpers.SensitiveDataOptions>(configuration.GetSection(Helpers.SensitiveDataOptions.SectionName));

        // Sensitive data masker (konfigürasyondan okur)
        services.AddSingleton<Helpers.ISensitiveDataMasker, Helpers.SensitiveDataMasker>();

        // Correlation context (Scoped - her request için yeni instance)
        services.AddScoped<ICorrelationContext, CorrelationContext>();

        // Log sinks
        services.AddSingleton<ILogSink, DatabaseLogSink>();
        services.AddSingleton<ILogSinkManager, LogSinkManager>();

        // Log service (Scoped - CorrelationContext'e bağımlı)
        services.AddScoped<ILogService, LogService>();

        // Auto-logging behavior for MediatR
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AutoLoggingBehavior<,>));

        return services;
    }

    /// <summary>
    /// Logging middleware'lerini pipeline'a register eder
    /// Sıralama önemli: Exception -> Correlation -> Request -> Action
    /// </summary>
    public static IApplicationBuilder UseLogging(this IApplicationBuilder app)
    {
        // 1. Exception logging (en üstte - tüm hataları yakalar)
        app.UseMiddleware<ExceptionLoggingMiddleware>();

        // 2. Correlation ID (exception'dan sonra - her request'e ID atar)
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 3. Request/Response logging (body dahil)
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 4. Action logging (controller seviyesinde)
        app.UseMiddleware<ActionLoggingMiddleware>();

        return app;
    }

    /// <summary>
    /// Serilog'u yapılandırır
    /// </summary>
    public static LoggerConfiguration ConfigureSerilog(
        this LoggerConfiguration loggerConfig,
        IConfiguration configuration,
        string applicationName)
    {
        var loggingOptions = configuration.GetSection(LoggingOptions.SectionName).Get<LoggingOptions>()
            ?? new LoggingOptions();

        // Türkiye saat dilimi için Serilog yapılandırması
        var turkeyTimeZone = TimeZoneHelper.TurkeyTimeZone;

        loggerConfig
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.WithProperty("TimeZone", "Turkey Standard Time (UTC+3)");

        // Console sink - Türkiye saati formatı
        loggerConfig.WriteTo.Console(
            outputTemplate: "[{Timestamp:dd.MM.yyyy HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}",
            formatProvider: new System.Globalization.CultureInfo("tr-TR"));

        // File sink - Türkiye saati formatı
        loggerConfig.WriteTo.File(
            path: $"logs/{applicationName}-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff} [{Level:u3}] [{CorrelationId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
            formatProvider: new System.Globalization.CultureInfo("tr-TR"));

        // ELK sink (koşullu)
        if (loggingOptions.Elk.Enabled)
        {
            loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(loggingOptions.Elk.ElasticsearchUrl))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                IndexFormat = loggingOptions.Elk.IndexFormat ?? $"enterprise-{applicationName.ToLower()}-{{0:yyyy.MM.dd}}",
                NumberOfShards = loggingOptions.Elk.NumberOfShards ?? 3,
                NumberOfReplicas = loggingOptions.Elk.NumberOfReplicas ?? 1,
                BatchPostingLimit = loggingOptions.Elk.BatchSize,
                Period = TimeSpan.FromSeconds(loggingOptions.Elk.FlushIntervalSeconds),
                BufferBaseFilename = loggingOptions.Elk.BufferPath ?? $"./logs/elk-buffer-{applicationName}",
                ModifyConnectionSettings = conn =>
                {
                    if (!string.IsNullOrEmpty(loggingOptions.Elk.Username))
                    {
                        conn.BasicAuthentication(loggingOptions.Elk.Username, loggingOptions.Elk.Password);
                    }
                    return conn;
                }
            });
        }

        return loggerConfig;
    }
}

