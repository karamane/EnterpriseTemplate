using System.Data;
using System.Linq.Expressions;
using Dapper;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Interfaces.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Enterprise.Infrastructure.Persistence.Repositories.Dapper;

/// <summary>
/// Dapper generic repository implementasyonu
/// Performans kritik senaryolar için
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">ID tipi</typeparam>
public class DapperRepository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class
{
    protected readonly string ConnectionString;
    protected readonly ILogger<DapperRepository<TEntity, TId>> Logger;
    protected readonly ICorrelationContext CorrelationContext;
    protected readonly string TableName;

    public DapperRepository(
        IConfiguration configuration,
        ILogger<DapperRepository<TEntity, TId>> logger,
        ICorrelationContext correlationContext)
    {
        ConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
        Logger = logger;
        CorrelationContext = correlationContext;
        TableName = GetTableName();
    }

    protected virtual string GetTableName()
    {
        // Convention: Entity ismi + "s"
        return typeof(TEntity).Name + "s";
    }

    protected async Task<SqlConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM [{TableName}] WHERE Id = @Id";

        await using var connection = await CreateConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id });
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM [{TableName}]";

        await using var connection = await CreateConnectionAsync();
        var result = await connection.QueryAsync<TEntity>(sql);
        return result.ToList();
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Dapper için expression'ı SQL'e çevirmek gerekir
        // Basit implementasyon - gerçek projede expression parser kullanılmalı
        Logger.LogWarning("Dapper GetAsync with expression is not fully implemented. Use raw SQL methods.");
        return await GetAllAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var all = await GetAsync(predicate, cancellationToken);
        return all.FirstOrDefault();
    }

    public virtual async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var offset = (pageNumber - 1) * pageSize;

        var countSql = $"SELECT COUNT(*) FROM [{TableName}]";
        var dataSql = $@"
            SELECT * FROM [{TableName}]
            ORDER BY Id {(ascending ? "ASC" : "DESC")}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        await using var connection = await CreateConnectionAsync();
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var items = await connection.QueryAsync<TEntity>(dataSql, new { Offset = offset, PageSize = pageSize });

        return (items.ToList(), totalCount);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Bu basit bir implementasyon - gerçek projede reflection veya source generator kullanılmalı
        var properties = typeof(TEntity).GetProperties()
            .Where(p => p.Name != "Id" && p.CanWrite)
            .ToList();

        var columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        var sql = $@"
            INSERT INTO [{TableName}] ({columns})
            OUTPUT INSERTED.*
            VALUES ({values})";

        await using var connection = await CreateConnectionAsync();
        var inserted = await connection.QuerySingleAsync<TEntity>(sql, entity);
        return inserted;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await AddAsync(entity, cancellationToken);
        }
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var properties = typeof(TEntity).GetProperties()
            .Where(p => p.Name != "Id" && p.CanWrite)
            .ToList();

        var setClause = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));

        var sql = $"UPDATE [{TableName}] SET {setClause} WHERE Id = @Id";

        await using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, entity);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await UpdateAsync(entity, cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var idProperty = typeof(TEntity).GetProperty("Id");
        var id = idProperty?.GetValue(entity);

        var sql = $"DELETE FROM [{TableName}] WHERE Id = @Id";

        await using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public virtual async Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var sql = $"DELETE FROM [{TableName}] WHERE Id = @Id";

        await using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT CASE WHEN EXISTS (SELECT 1 FROM [{TableName}]) THEN 1 ELSE 0 END";

        await using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(sql);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT COUNT(*) FROM [{TableName}]";

        await using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    #region Raw SQL Methods

    /// <summary>
    /// Raw SQL sorgusu çalıştırır
    /// </summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        Logger.LogDebug(
            "[{CorrelationId}] Dapper Query: {Sql}",
            CorrelationContext.CorrelationId, sql);

        await using var connection = await CreateConnectionAsync();
        return await connection.QueryAsync<T>(sql, param);
    }

    /// <summary>
    /// Raw SQL sorgusu çalıştırır (tek sonuç)
    /// </summary>
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        await using var connection = await CreateConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    /// <summary>
    /// Raw SQL komutu çalıştırır
    /// </summary>
    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        Logger.LogDebug(
            "[{CorrelationId}] Dapper Execute: {Sql}",
            CorrelationContext.CorrelationId, sql);

        await using var connection = await CreateConnectionAsync();
        return await connection.ExecuteAsync(sql, param);
    }

    #endregion
}

