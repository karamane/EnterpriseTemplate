using System.Data;
using Enterprise.Infrastructure.Persistence.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace Enterprise.Infrastructure.Persistence.Factories;

/// <summary>
/// Database connection factory interface
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Yeni bir connection oluşturur
    /// </summary>
    IDbConnection CreateConnection();

    /// <summary>
    /// Async olarak yeni bir connection oluşturur ve açar
    /// </summary>
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktif provider
    /// </summary>
    DatabaseProvider Provider { get; }
}

/// <summary>
/// Database connection factory implementasyonu
/// SqlServer ve Oracle arasında switch yapılabilir
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public DbConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public DatabaseProvider Provider => _options.Provider;

    public IDbConnection CreateConnection()
    {
        return _options.Provider switch
        {
            DatabaseProvider.SqlServer => new SqlConnection(_options.ConnectionString),
            DatabaseProvider.Oracle => new OracleConnection(_options.ConnectionString),
            DatabaseProvider.PostgreSql => CreatePostgreSqlConnection(),
            DatabaseProvider.MySql => CreateMySqlConnection(),
            DatabaseProvider.SQLite => CreateSQLiteConnection(),
            _ => throw new NotSupportedException($"Database provider '{_options.Provider}' is not supported.")
        };
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();

        switch (connection)
        {
            case SqlConnection sqlConnection:
                await sqlConnection.OpenAsync(cancellationToken);
                break;
            case OracleConnection oracleConnection:
                await oracleConnection.OpenAsync(cancellationToken);
                break;
            default:
                // Diğer provider'lar için generic Open
                connection.Open();
                break;
        }

        return connection;
    }

    #region Provider Specific Connection Creators


    private IDbConnection CreatePostgreSqlConnection()
    {
        // Npgsql paketi gerekli
        var npgsqlAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Npgsql");

        if (npgsqlAssembly == null)
        {
            try
            {
                npgsqlAssembly = System.Reflection.Assembly.Load("Npgsql");
            }
            catch
            {
                throw new InvalidOperationException(
                    "PostgreSQL provider is configured but Npgsql package is not installed. " +
                    "Please install: dotnet add package Npgsql");
            }
        }

        var connectionType = npgsqlAssembly.GetType("Npgsql.NpgsqlConnection");
        var connection = Activator.CreateInstance(connectionType!, _options.ConnectionString) as IDbConnection;
        return connection ?? throw new InvalidOperationException("Failed to create PostgreSQL connection.");
    }

    private IDbConnection CreateMySqlConnection()
    {
        // MySqlConnector paketi gerekli
        var mysqlAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "MySqlConnector");

        if (mysqlAssembly == null)
        {
            try
            {
                mysqlAssembly = System.Reflection.Assembly.Load("MySqlConnector");
            }
            catch
            {
                throw new InvalidOperationException(
                    "MySQL provider is configured but MySqlConnector package is not installed. " +
                    "Please install: dotnet add package MySqlConnector");
            }
        }

        var connectionType = mysqlAssembly.GetType("MySqlConnector.MySqlConnection");
        var connection = Activator.CreateInstance(connectionType!, _options.ConnectionString) as IDbConnection;
        return connection ?? throw new InvalidOperationException("Failed to create MySQL connection.");
    }

    private IDbConnection CreateSQLiteConnection()
    {
        // Microsoft.Data.Sqlite paketi gerekli
        var sqliteAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Microsoft.Data.Sqlite");

        if (sqliteAssembly == null)
        {
            try
            {
                sqliteAssembly = System.Reflection.Assembly.Load("Microsoft.Data.Sqlite");
            }
            catch
            {
                throw new InvalidOperationException(
                    "SQLite provider is configured but Microsoft.Data.Sqlite package is not installed. " +
                    "Please install: dotnet add package Microsoft.Data.Sqlite");
            }
        }

        var connectionType = sqliteAssembly.GetType("Microsoft.Data.Sqlite.SqliteConnection");
        var connection = Activator.CreateInstance(connectionType!, _options.ConnectionString) as IDbConnection;
        return connection ?? throw new InvalidOperationException("Failed to create SQLite connection.");
    }

    #endregion
}

