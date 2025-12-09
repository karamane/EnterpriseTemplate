using System.Text;
using Enterprise.Infrastructure.CrossCutting.Options;
using Enterprise.Infrastructure.CrossCutting.Security;
using Enterprise.Infrastructure.CrossCutting.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

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

        // Redis (Token cache için)
        services.RegisterRedis(configuration);

        // JWT Authentication
        services.RegisterJwtAuthentication(configuration);

        return services;
    }

    /// <summary>
    /// Redis bağlantısını register eder (opsiyonel - hata olursa sessizce devam eder)
    /// </summary>
    public static IServiceCollection RegisterRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cacheSection = configuration.GetSection("Cache");
        var connectionString = cacheSection.GetValue<string>("ConnectionString");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Redis yapılandırması yok - in-memory cache kullanılacak
            return services;
        }

        try
        {
            // Redis connection multiplexer (singleton)
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = ConfigurationOptions.Parse(connectionString);
                options.AbortOnConnectFail = false; // Bağlantı hatası olsa bile devam et
                options.ConnectRetry = 3;
                options.ConnectTimeout = 5000;
                return ConnectionMultiplexer.Connect(options);
            });

            // Token cache service
            services.AddScoped<ITokenCacheService, RedisTokenCacheService>();
        }
        catch (Exception)
        {
            // Redis bağlantısı başarısız - uygulama in-memory cache ile çalışacak
        }

        return services;
    }

    /// <summary>
    /// JWT Authentication servislerini register eder
    /// </summary>
    public static IServiceCollection RegisterJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Options
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(jwtSection);

        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

        // SecretKey boşsa default development key kullan
        if (string.IsNullOrEmpty(jwtOptions.SecretKey))
        {
            jwtOptions.SecretKey = "DefaultDevelopmentSecretKeyThatIsAtLeast32Characters!";
        }

        // Authentication Service
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();

        // JWT Bearer Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
        });

        return services;
    }
}

