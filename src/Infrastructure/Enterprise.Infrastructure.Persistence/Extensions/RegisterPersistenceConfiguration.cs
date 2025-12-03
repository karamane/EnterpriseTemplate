using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Infrastructure.Persistence.Context;
using Enterprise.Infrastructure.Persistence.Factories;
using Enterprise.Infrastructure.Persistence.Options;
using Enterprise.Infrastructure.Persistence.Repositories.Dapper;
using Enterprise.Infrastructure.Persistence.Repositories.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oracle.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Infrastructure;

namespace Enterprise.Infrastructure.Persistence.Extensions;

/// <summary>
/// Persistence servisleri yapılandırma sınıfı
/// SqlServer ve Oracle arasında switch yapılabilir
/// Plugin gibi tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterPersistenceConfiguration
{
    /// <summary>
    /// Persistence servislerini DI container'a register eder
    /// Provider ve ORM tipine göre yapılandırır
    /// </summary>
    /// <example>
    /// services.RegisterPersistence(configuration);
    /// </example>
    public static IServiceCollection RegisterPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database options
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        // Connection factory (tüm provider'lar için)
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        // ORM tipine göre yapılandır
        if (dbOptions.OrmType == OrmType.EfCore)
        {
            ConfigureEfCore(services, dbOptions);
        }
        else
        {
            ConfigureDapper(services);
        }

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }

    /// <summary>
    /// EF Core yapılandırması
    /// SqlServer ve Oracle destekli
    /// </summary>
    private static void ConfigureEfCore(IServiceCollection services, DatabaseOptions options)
    {
        services.AddDbContext<ApplicationDbContext>((sp, dbContextOptions) =>
        {
            var logger = sp.GetService<ILogger<ApplicationDbContext>>();

            // Provider'a göre yapılandır
            switch (options.Provider)
            {
                case DatabaseProvider.SqlServer:
                    ConfigureSqlServer(dbContextOptions, options);
                    break;

                case DatabaseProvider.Oracle:
                    ConfigureOracle(dbContextOptions, options);
                    break;

                case DatabaseProvider.PostgreSql:
                    ConfigurePostgreSql(dbContextOptions, options);
                    break;

                case DatabaseProvider.MySql:
                    ConfigureMySql(dbContextOptions, options);
                    break;

                case DatabaseProvider.SQLite:
                    ConfigureSQLite(dbContextOptions, options);
                    break;

                default:
                    throw new NotSupportedException($"Database provider '{options.Provider}' is not supported.");
            }

            // Development'ta detailed errors
            if (options.EnableSensitiveDataLogging)
            {
                dbContextOptions.EnableDetailedErrors();
                dbContextOptions.EnableSensitiveDataLogging();
            }

            logger?.LogInformation(
                "Database configured: Provider={Provider}, ORM={OrmType}",
                options.Provider,
                options.OrmType);
        });

        // EF Core repositories
        services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<>));
        services.AddScoped(typeof(IRepository<,>), typeof(EfCoreRepository<,>));
    }

    /// <summary>
    /// SQL Server yapılandırması
    /// </summary>
    private static void ConfigureSqlServer(DbContextOptionsBuilder options, DatabaseOptions dbOptions)
    {
        options.UseSqlServer(dbOptions.ConnectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: dbOptions.MaxRetryCount,
                maxRetryDelay: TimeSpan.FromSeconds(dbOptions.MaxRetryDelay),
                errorNumbersToAdd: null);

            sqlOptions.CommandTimeout(dbOptions.CommandTimeout);
        });
    }

    /// <summary>
    /// Oracle yapılandırması
    /// Oracle.EntityFrameworkCore paketi ile tam entegrasyon
    /// </summary>
    private static void ConfigureOracle(DbContextOptionsBuilder options, DatabaseOptions dbOptions)
    {
        options.UseOracle(dbOptions.ConnectionString, oracleOptions =>
        {
            // Command timeout
            oracleOptions.CommandTimeout(dbOptions.CommandTimeout);

            // Oracle 19c ve üstü için SQL uyumluluğu
            oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);

            // Retry stratejisi
            if (dbOptions.MaxRetryCount > 0)
            {
                oracleOptions.ExecutionStrategy(dependencies =>
                    new OracleRetryingExecutionStrategy(
                        dependencies,
                        maxRetryCount: dbOptions.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(dbOptions.MaxRetryDelay),
                        errorNumbersToAdd: null));
            }
        });
    }

    /// <summary>
    /// PostgreSQL yapılandırması
    /// </summary>
    private static void ConfigurePostgreSql(DbContextOptionsBuilder options, DatabaseOptions dbOptions)
    {
        var npgsqlExtensionsType = Type.GetType(
            "Npgsql.EntityFrameworkCore.PostgreSQL.NpgsqlDbContextOptionsBuilderExtensions, Npgsql.EntityFrameworkCore.PostgreSQL");

        if (npgsqlExtensionsType == null)
        {
            throw new InvalidOperationException(
                "PostgreSQL provider is configured but Npgsql.EntityFrameworkCore.PostgreSQL package is not installed.");
        }

        var useNpgsqlMethod = npgsqlExtensionsType.GetMethod(
            "UseNpgsql",
            new[] { typeof(DbContextOptionsBuilder), typeof(string) });

        useNpgsqlMethod?.Invoke(null, new object[] { options, dbOptions.ConnectionString });
    }

    /// <summary>
    /// MySQL yapılandırması
    /// </summary>
    private static void ConfigureMySql(DbContextOptionsBuilder options, DatabaseOptions dbOptions)
    {
        var mysqlExtensionsType = Type.GetType(
            "Pomelo.EntityFrameworkCore.MySql.MySqlDbContextOptionsBuilderExtensions, Pomelo.EntityFrameworkCore.MySql");

        if (mysqlExtensionsType == null)
        {
            throw new InvalidOperationException(
                "MySQL provider is configured but Pomelo.EntityFrameworkCore.MySql package is not installed.");
        }

        var useMySqlMethod = mysqlExtensionsType.GetMethods()
            .FirstOrDefault(m => m.Name == "UseMySql" && m.GetParameters().Length == 3);

        if (useMySqlMethod == null)
        {
            throw new InvalidOperationException("Could not find suitable UseMySql method.");
        }
    }

    /// <summary>
    /// SQLite yapılandırması (test için)
    /// </summary>
    private static void ConfigureSQLite(DbContextOptionsBuilder options, DatabaseOptions dbOptions)
    {
        options.UseSqlite(dbOptions.ConnectionString);
    }

    /// <summary>
    /// Dapper yapılandırması
    /// </summary>
    private static void ConfigureDapper(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<,>), typeof(DapperRepository<,>));
    }
}

