namespace Enterprise.Infrastructure.Persistence.Options;

/// <summary>
/// Veritabanı yapılandırma seçenekleri
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// Veritabanı provider tipi (SqlServer, Oracle)
    /// </summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// ORM tipi (EfCore, Dapper)
    /// </summary>
    public OrmType OrmType { get; set; } = OrmType.EfCore;

    /// <summary>
    /// Command timeout (saniye)
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Retry count
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Retry delay (saniye)
    /// </summary>
    public int MaxRetryDelay { get; set; } = 30;

    /// <summary>
    /// Migration'ları otomatik uygula
    /// </summary>
    public bool EnableAutoMigration { get; set; } = false;

    /// <summary>
    /// Sensitive data logging (sadece development)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

/// <summary>
/// Desteklenen veritabanı provider'ları
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// Microsoft SQL Server
    /// </summary>
    SqlServer,

    /// <summary>
    /// Oracle Database
    /// </summary>
    Oracle,

    /// <summary>
    /// PostgreSQL
    /// </summary>
    PostgreSql,

    /// <summary>
    /// MySQL / MariaDB
    /// </summary>
    MySql,

    /// <summary>
    /// SQLite (test için)
    /// </summary>
    SQLite
}

/// <summary>
/// ORM tipi
/// </summary>
public enum OrmType
{
    /// <summary>
    /// Entity Framework Core
    /// </summary>
    EfCore,

    /// <summary>
    /// Dapper
    /// </summary>
    Dapper
}

