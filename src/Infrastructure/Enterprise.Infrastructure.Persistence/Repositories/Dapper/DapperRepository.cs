using System.Data;
using System.Linq.Expressions;
using Dapper;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Infrastructure.Persistence.Factories;
using Enterprise.Infrastructure.Persistence.Options;
using Microsoft.Extensions.Logging;

namespace Enterprise.Infrastructure.Persistence.Repositories.Dapper;

/// <summary>
/// Dapper generic repository implementasyonu
/// Performans kritik senaryolar için
/// SqlServer ve Oracle destekler
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">ID tipi</typeparam>
public class DapperRepository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class
{
    protected readonly IDbConnectionFactory ConnectionFactory;
    protected readonly ILogger<DapperRepository<TEntity, TId>> Logger;
    protected readonly ICorrelationContext CorrelationContext;
    protected readonly string TableName;
    protected readonly DatabaseProvider Provider;

    public DapperRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<DapperRepository<TEntity, TId>> logger,
        ICorrelationContext correlationContext)
    {
        ConnectionFactory = connectionFactory;
        Logger = logger;
        CorrelationContext = correlationContext;
        Provider = connectionFactory.Provider;
        TableName = GetTableName();
    }

    protected virtual string GetTableName()
    {
        // Oracle: büyük harf, SqlServer: PascalCase
        var baseName = typeof(TEntity).Name + "s";
        return Provider == DatabaseProvider.Oracle 
            ? baseName.ToUpperInvariant() 
            : baseName;
    }

    protected async Task<IDbConnection> CreateConnectionAsync()
    {
        return await ConnectionFactory.CreateOpenConnectionAsync();
    }

    /// <summary>
    /// Provider'a göre tablo/sütun adını formatlar
    /// Oracle: "TABLE_NAME", SqlServer: [TableName]
    /// </summary>
    protected string Quote(string identifier)
    {
        return Provider == DatabaseProvider.Oracle
            ? $"\"{identifier.ToUpperInvariant()}\""
            : $"[{identifier}]";
    }

    /// <summary>
    /// Provider'a göre parametre prefix'i döner
    /// Oracle: :param, SqlServer: @param
    /// </summary>
    protected string ParamPrefix => Provider == DatabaseProvider.Oracle ? ":" : "@";

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {Quote(TableName)} WHERE {Quote("Id")} = {ParamPrefix}Id";

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id });
        }
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {Quote(TableName)}";

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            var result = await connection.QueryAsync<TEntity>(sql);
            return result.ToList();
        }
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Dapper için expression'ı SQL'e çevirmek gerekir
        // Basit implementasyon - gerçek projede expression parser kullanılmalı
        Logger.LogWarning(
            "[{CorrelationId}] Dapper GetAsync with expression is not fully implemented. Use raw SQL methods.",
            CorrelationContext.CorrelationId);
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
        var orderDirection = ascending ? "ASC" : "DESC";

        var countSql = $"SELECT COUNT(*) FROM {Quote(TableName)}";
        
        // Provider-specific paging
        var dataSql = Provider == DatabaseProvider.Oracle
            ? $@"SELECT * FROM {Quote(TableName)} 
                 ORDER BY {Quote("Id")} {orderDirection}
                 OFFSET {ParamPrefix}Offset ROWS FETCH NEXT {ParamPrefix}PageSize ROWS ONLY"
            : $@"SELECT * FROM {Quote(TableName)}
                 ORDER BY {Quote("Id")} {orderDirection}
                 OFFSET {ParamPrefix}Offset ROWS
                 FETCH NEXT {ParamPrefix}PageSize ROWS ONLY";

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
            var items = await connection.QueryAsync<TEntity>(dataSql, new { Offset = offset, PageSize = pageSize });

            return (items.ToList(), totalCount);
        }
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Bu basit bir implementasyon - gerçek projede reflection veya source generator kullanılmalı
        var properties = typeof(TEntity).GetProperties()
            .Where(p => p.Name != "Id" && p.CanWrite && p.Name != "DomainEvents")
            .ToList();

        var columns = string.Join(", ", properties.Select(p => Quote(p.Name)));
        var values = string.Join(", ", properties.Select(p => $"{ParamPrefix}{p.Name}"));

        string sql;
        if (Provider == DatabaseProvider.Oracle)
        {
            // Oracle: RETURNING INTO ile ID almak için ayrı sorgu gerekir
            sql = $@"INSERT INTO {Quote(TableName)} ({columns}) VALUES ({values})";
            
            var connection = await CreateConnectionAsync();
            await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
            {
                await connection.ExecuteAsync(sql, entity);
                // Son eklenen ID'yi al
                var lastIdSql = $"SELECT MAX({Quote("Id")}) FROM {Quote(TableName)}";
                var lastId = await connection.ExecuteScalarAsync<TId>(lastIdSql);
                // Entity'yi geri oku (lastId null olabilir, bu durumda entity döndür)
                if (lastId is null)
                    return entity;
                return await GetByIdAsync(lastId, cancellationToken) ?? entity;
            }
        }
        else
        {
            // SQL Server: OUTPUT INSERTED ile doğrudan dönüş
            sql = $@"INSERT INTO {Quote(TableName)} ({columns})
                     OUTPUT INSERTED.*
                     VALUES ({values})";
            
            var connection = await CreateConnectionAsync();
            await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
            {
                var inserted = await connection.QuerySingleAsync<TEntity>(sql, entity);
                return inserted;
            }
        }
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
            .Where(p => p.Name != "Id" && p.CanWrite && p.Name != "DomainEvents")
            .ToList();

        var setClause = string.Join(", ", properties.Select(p => $"{Quote(p.Name)} = {ParamPrefix}{p.Name}"));

        var sql = $"UPDATE {Quote(TableName)} SET {setClause} WHERE {Quote("Id")} = {ParamPrefix}Id";

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            await connection.ExecuteAsync(sql, entity);
        }
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

        var sql = $"DELETE FROM {Quote(TableName)} WHERE {Quote("Id")} = {ParamPrefix}Id";

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }

    public virtual async Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var sql = $"DELETE FROM {Quote(TableName)} WHERE {Quote("Id")} = {ParamPrefix}Id";

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // NOT: Predicate şu an desteklenmiyor - tüm tablo için kontrol yapılır
        // Gerçek projede expression-to-SQL dönüşümü implemente edilmeli
        Logger.LogWarning(
            "[{CorrelationId}] Dapper ExistsAsync with expression is not fully implemented. Checking if any record exists.",
            CorrelationContext.CorrelationId);
            
        var sql = Provider == DatabaseProvider.Oracle
            ? $"SELECT CASE WHEN EXISTS (SELECT 1 FROM {Quote(TableName)}) THEN 1 ELSE 0 END FROM DUAL"
            : $"SELECT CASE WHEN EXISTS (SELECT 1 FROM {Quote(TableName)}) THEN 1 ELSE 0 END";

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            return await connection.ExecuteScalarAsync<int>(sql) == 1;
        }
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT COUNT(*) FROM {Quote(TableName)}";

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            return await connection.ExecuteScalarAsync<int>(sql);
        }
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

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            return await connection.QueryAsync<T>(sql, param);
        }
    }

    /// <summary>
    /// Raw SQL sorgusu çalıştırır (tek sonuç)
    /// </summary>
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
        }
    }

    /// <summary>
    /// Raw SQL komutu çalıştırır
    /// </summary>
    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        Logger.LogDebug(
            "[{CorrelationId}] Dapper Execute: {Sql}",
            CorrelationContext.CorrelationId, sql);

        var connection = await CreateConnectionAsync();
        await using (connection as IAsyncDisposable ?? throw new InvalidOperationException("Connection must be disposable"))
        {
            return await connection.ExecuteAsync(sql, param);
        }
    }

    #endregion
}

