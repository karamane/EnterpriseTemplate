using Enterprise.Core.Shared.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Enterprise.Core.Shared.Extensions;

/// <summary>
/// Swagger yapılandırma sınıfı
/// Parametrik olarak kontrol edilebilir
/// </summary>
public static class RegisterSwaggerConfiguration
{
    /// <summary>
    /// Swagger servislerini register eder
    /// </summary>
    public static IServiceCollection RegisterSwagger(
        this IServiceCollection services,
        IConfiguration configuration,
        string? defaultTitle = null,
        string? defaultDescription = null)
    {
        var options = configuration.GetSection(SwaggerOptions.SectionName).Get<SwaggerOptions>()
            ?? new SwaggerOptions();

        if (!options.Enabled)
            return services;

        services.Configure<SwaggerOptions>(configuration.GetSection(SwaggerOptions.SectionName));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(options.Version, new OpenApiInfo
            {
                Title = defaultTitle ?? options.Title,
                Version = options.Version,
                Description = defaultDescription ?? options.Description,
                Contact = options.Contact != null ? new OpenApiContact
                {
                    Name = options.Contact.Name,
                    Email = options.Contact.Email,
                    Url = !string.IsNullOrEmpty(options.Contact.Url) ? new Uri(options.Contact.Url) : null
                } : null,
                License = options.License != null ? new OpenApiLicense
                {
                    Name = options.License.Name,
                    Url = !string.IsNullOrEmpty(options.License.Url) ? new Uri(options.License.Url) : null
                } : null
            });

            // Bearer Token Authentication
            if (options.EnableBearerAuth)
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }

            // API Key Authentication
            if (options.EnableApiKeyAuth)
            {
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "API Key Authentication. Example: \"X-API-Key: {key}\"",
                    Name = "X-API-Key",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }

            // XML Comments
            if (options.IncludeXmlComments)
            {
                var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
                foreach (var xmlFile in xmlFiles)
                {
                    try
                    {
                        c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
                    }
                    catch
                    {
                        // XML dosyası okunamazsa devam et
                    }
                }
            }

            // Custom schema IDs
            c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        });

        return services;
    }

    /// <summary>
    /// Swagger middleware'ini pipeline'a ekler
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithConfig(
        this IApplicationBuilder app,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration.GetSection(SwaggerOptions.SectionName).Get<SwaggerOptions>()
            ?? new SwaggerOptions();

        if (!options.Enabled)
            return app;

        // Ortam kontrolü
        if (options.AllowedEnvironments.Length > 0)
        {
            var isAllowed = options.AllowedEnvironments
                .Any(e => e.Equals(environment.EnvironmentName, StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
                return app;
        }

        // Swagger JSON
        app.UseSwagger(c =>
        {
            c.RouteTemplate = $"{options.RoutePrefix}/{{documentName}}/swagger.json";
        });

        // Swagger UI
        if (options.EnableUI)
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/{options.RoutePrefix}/{options.Version}/swagger.json", $"{options.Title} {options.Version}");
                c.RoutePrefix = options.RoutePrefix;

                // DocExpansion
                c.DocExpansion(options.DocExpansion switch
                {
                    "none" => Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None,
                    "full" => Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.Full,
                    _ => Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List
                });

                // Dark theme
                if (options.Theme.Equals("dark", StringComparison.OrdinalIgnoreCase))
                {
                    c.InjectStylesheet("/swagger-ui/dark-theme.css");
                }

                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();
            });
        }

        return app;
    }
}

