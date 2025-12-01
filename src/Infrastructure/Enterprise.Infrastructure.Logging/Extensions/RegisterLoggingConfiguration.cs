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

        var fileOptions = configuration.GetSection(Options.FileLoggingOptions.SectionName).Get<Options.FileLoggingOptions>()
            ?? new Options.FileLoggingOptions();

        // Log klasör yolunu oluştur
        var basePath = fileOptions.BasePath;
        var appFolder = fileOptions.UseApplicationSubfolder 
            ? Path.Combine(basePath, applicationName.Replace(".", "-").ToLower())
            : basePath;

        // Klasörleri oluştur
        EnsureLogDirectories(appFolder, fileOptions.SeparateFiles);

        // Serilog rolling interval dönüşümü
        var rollingInterval = ConvertRollingInterval(fileOptions.RollingInterval);

        // Türkiye formatı
        var turkeyFormat = new System.Globalization.CultureInfo("tr-TR");

        // Output template'leri
        const string consoleTemplate = "[{Timestamp:dd.MM.yyyy HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}";
        const string fileTemplate = "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] [{SourceContext}] [{MachineName}] {Message:lj}{NewLine}{Exception}";
        const string jsonTemplate = "{ \"timestamp\": \"{Timestamp:o}\", \"level\": \"{Level}\", \"correlationId\": \"{CorrelationId}\", \"source\": \"{SourceContext}\", \"machine\": \"{MachineName}\", \"message\": {Message:lj}, \"exception\": \"{Exception}\" }";

        var outputTemplate = fileOptions.UseJsonFormat ? jsonTemplate : fileTemplate;

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

        // Console sink
        loggerConfig.WriteTo.Console(
            outputTemplate: consoleTemplate,
            formatProvider: turkeyFormat);

        // File sinks
        if (fileOptions.Enabled)
        {
            // 1. Tüm loglar
            if (fileOptions.SeparateFiles.AllLogs)
            {
                loggerConfig.WriteTo.File(
                    path: Path.Combine(appFolder, "all", "log-.txt"),
                    rollingInterval: rollingInterval,
                    retainedFileCountLimit: fileOptions.RetentionDays,
                    fileSizeLimitBytes: fileOptions.MaxFileSizeMB * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    outputTemplate: outputTemplate,
                    formatProvider: turkeyFormat,
                    shared: true);
            }

            // 2. Error ve üstü loglar
            if (fileOptions.SeparateFiles.ErrorLogs)
            {
                loggerConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                    .WriteTo.File(
                        path: Path.Combine(appFolder, "errors", "error-.txt"),
                        rollingInterval: rollingInterval,
                        retainedFileCountLimit: fileOptions.RetentionDays * 2, // Hataları daha uzun tut
                        fileSizeLimitBytes: fileOptions.MaxFileSizeMB * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        outputTemplate: outputTemplate,
                        formatProvider: turkeyFormat,
                        shared: true));
            }

            // 3. Request/Response logları (SourceContext filtrelemesi)
            if (fileOptions.SeparateFiles.RequestLogs)
            {
                loggerConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => 
                        e.Properties.ContainsKey("LogType") && 
                        (e.Properties["LogType"].ToString().Contains("Request") || 
                         e.Properties["LogType"].ToString().Contains("Response")))
                    .WriteTo.File(
                        path: Path.Combine(appFolder, "requests", "request-.txt"),
                        rollingInterval: rollingInterval,
                        retainedFileCountLimit: fileOptions.RetentionDays,
                        fileSizeLimitBytes: fileOptions.MaxFileSizeMB * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        outputTemplate: outputTemplate,
                        formatProvider: turkeyFormat,
                        shared: true));
            }

            // 4. Performance logları
            if (fileOptions.SeparateFiles.PerformanceLogs)
            {
                loggerConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => 
                        e.Properties.ContainsKey("LogType") && 
                        e.Properties["LogType"].ToString().Contains("Performance"))
                    .WriteTo.File(
                        path: Path.Combine(appFolder, "performance", "perf-.txt"),
                        rollingInterval: rollingInterval,
                        retainedFileCountLimit: fileOptions.RetentionDays,
                        fileSizeLimitBytes: fileOptions.MaxFileSizeMB * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        outputTemplate: outputTemplate,
                        formatProvider: turkeyFormat,
                        shared: true));
            }

            // 5. Business exception logları
            if (fileOptions.SeparateFiles.BusinessLogs)
            {
                loggerConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => 
                        e.Properties.ContainsKey("LogType") && 
                        e.Properties["LogType"].ToString().Contains("Business"))
                    .WriteTo.File(
                        path: Path.Combine(appFolder, "business", "business-.txt"),
                        rollingInterval: rollingInterval,
                        retainedFileCountLimit: fileOptions.RetentionDays * 2,
                        fileSizeLimitBytes: fileOptions.MaxFileSizeMB * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        outputTemplate: outputTemplate,
                        formatProvider: turkeyFormat,
                        shared: true));
            }

            // 6. Security/Audit logları
            if (fileOptions.SeparateFiles.SecurityLogs)
            {
                loggerConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => 
                        e.Properties.ContainsKey("LogType") && 
                        (e.Properties["LogType"].ToString().Contains("Security") || 
                         e.Properties["LogType"].ToString().Contains("Audit")))
                    .WriteTo.File(
                        path: Path.Combine(appFolder, "security", "audit-.txt"),
                        rollingInterval: rollingInterval,
                        retainedFileCountLimit: fileOptions.RetentionDays * 3, // Audit logları daha uzun tut
                        fileSizeLimitBytes: fileOptions.MaxFileSizeMB * 1024 * 1024,
                        rollOnFileSizeLimit: true,
                        outputTemplate: outputTemplate,
                        formatProvider: turkeyFormat,
                        shared: true));
            }
        }

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
                BufferBaseFilename = loggingOptions.Elk.BufferPath ?? Path.Combine(appFolder, "elk-buffer"),
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

    /// <summary>
    /// Log klasörlerini oluşturur
    /// </summary>
    private static void EnsureLogDirectories(string basePath, SeparateLogFilesOptions options)
    {
        var directories = new List<string> { basePath };

        if (options.AllLogs) directories.Add(Path.Combine(basePath, "all"));
        if (options.ErrorLogs) directories.Add(Path.Combine(basePath, "errors"));
        if (options.RequestLogs) directories.Add(Path.Combine(basePath, "requests"));
        if (options.PerformanceLogs) directories.Add(Path.Combine(basePath, "performance"));
        if (options.BusinessLogs) directories.Add(Path.Combine(basePath, "business"));
        if (options.SecurityLogs) directories.Add(Path.Combine(basePath, "security"));

        foreach (var dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }

    /// <summary>
    /// Rolling interval dönüşümü
    /// </summary>
    private static RollingInterval ConvertRollingInterval(Options.LogRollingInterval interval)
    {
        return interval switch
        {
            Options.LogRollingInterval.Infinite => RollingInterval.Infinite,
            Options.LogRollingInterval.Year => RollingInterval.Year,
            Options.LogRollingInterval.Month => RollingInterval.Month,
            Options.LogRollingInterval.Day => RollingInterval.Day,
            Options.LogRollingInterval.Hour => RollingInterval.Hour,
            Options.LogRollingInterval.Minute => RollingInterval.Minute,
            _ => RollingInterval.Day
        };
    }
}

