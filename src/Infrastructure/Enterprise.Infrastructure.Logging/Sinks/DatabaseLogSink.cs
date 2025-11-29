using System.Data;
using System.Text.Json;
using Dapper;
using Enterprise.Core.Application.Models.Logging;
using Enterprise.Core.Shared.Constants;
using Enterprise.Infrastructure.Logging.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enterprise.Infrastructure.Logging.Sinks;

/// <summary>
/// Database log sink
/// Logları SQL Server'a yazar
/// </summary>
public class DatabaseLogSink : ILogSink
{
    private readonly ILogger<DatabaseLogSink> _logger;
    private readonly DatabaseLogOptions _options;

    public string Name => "Database";
    public bool IsEnabled => _options.Enabled && !string.IsNullOrEmpty(_options.ConnectionString);

    public DatabaseLogSink(
        ILogger<DatabaseLogSink> logger,
        IOptions<LoggingOptions> options)
    {
        _logger = logger;
        _options = options.Value.Database;
    }

    public async Task WriteAsync(BaseLogEntry entry, CancellationToken cancellationToken = default)
    {
        await WriteBatchAsync(new[] { entry }, cancellationToken);
    }

    public async Task WriteBatchAsync(IEnumerable<BaseLogEntry> entries, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
            return;

        var entryList = entries.ToList();
        if (entryList.Count == 0)
            return;

        try
        {
            await using var connection = new SqlConnection(_options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Log türlerine göre grupla ve uygun tablolara yaz
            var requestLogs = entryList.OfType<RequestLogEntry>().ToList();
            var responseLogs = entryList.OfType<ResponseLogEntry>().ToList();
            var exceptionLogs = entryList.OfType<ExceptionLogEntry>()
                .Where(e => e is not BusinessExceptionLogEntry).ToList();
            var businessExceptionLogs = entryList.OfType<BusinessExceptionLogEntry>().ToList();
            var auditLogs = entryList.OfType<AuditLogEntry>().ToList();

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                if (requestLogs.Any())
                    await InsertRequestLogsAsync(connection, transaction, requestLogs);

                if (responseLogs.Any())
                    await InsertResponseLogsAsync(connection, transaction, responseLogs);

                if (exceptionLogs.Any())
                    await InsertExceptionLogsAsync(connection, transaction, exceptionLogs);

                if (businessExceptionLogs.Any())
                    await InsertBusinessExceptionLogsAsync(connection, transaction, businessExceptionLogs);

                if (auditLogs.Any())
                    await InsertAuditLogsAsync(connection, transaction, auditLogs);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing {Count} logs to database", entryList.Count);
            throw;
        }
    }

    private static async Task InsertRequestLogsAsync(
        SqlConnection connection,
        IDbTransaction transaction,
        IEnumerable<RequestLogEntry> entries)
    {
        const string sql = @"
            INSERT INTO [Log].[RequestLogs] 
            ([LogId], [CorrelationId], [Timestamp], [HttpMethod], [RequestPath], [QueryString], 
             [RequestBody], [RequestHeaders], [ContentType], [ContentLength], [ClientIp], 
             [UserAgent], [UserId], [Layer], [ServerName], [ApplicationName])
            VALUES 
            (@LogId, @CorrelationId, @Timestamp, @HttpMethod, @RequestPath, @QueryString,
             @RequestBody, @RequestHeaders, @ContentType, @ContentLength, @ClientIp,
             @UserAgent, @UserId, @Layer, @ServerName, @ApplicationName)";

        var parameters = entries.Select(e => new
        {
            e.LogId,
            e.CorrelationId,
            e.Timestamp,
            e.HttpMethod,
            e.RequestPath,
            e.QueryString,
            e.RequestBody,
            RequestHeaders = e.RequestHeaders != null ? JsonSerializer.Serialize(e.RequestHeaders) : null,
            e.ContentType,
            e.ContentLength,
            e.ClientIp,
            e.UserAgent,
            e.UserId,
            e.Layer,
            e.ServerName,
            e.ApplicationName
        });

        await connection.ExecuteAsync(sql, parameters, transaction);
    }

    private static async Task InsertResponseLogsAsync(
        SqlConnection connection,
        IDbTransaction transaction,
        IEnumerable<ResponseLogEntry> entries)
    {
        const string sql = @"
            INSERT INTO [Log].[ResponseLogs]
            ([LogId], [CorrelationId], [RequestLogId], [Timestamp], [StatusCode], [DurationMs],
             [ResponseBody], [ContentType], [DbQueryCount], [DbQueryDurationMs], [CacheHitCount],
             [CacheMissCount], [Layer], [ServerName], [ApplicationName])
            VALUES
            (@LogId, @CorrelationId, @RequestLogId, @Timestamp, @StatusCode, @DurationMs,
             @ResponseBody, @ContentType, @DbQueryCount, @DbQueryDurationMs, @CacheHitCount,
             @CacheMissCount, @Layer, @ServerName, @ApplicationName)";

        var parameters = entries.Select(e => new
        {
            e.LogId,
            e.CorrelationId,
            e.RequestLogId,
            e.Timestamp,
            e.StatusCode,
            e.DurationMs,
            e.ResponseBody,
            e.ContentType,
            e.DbQueryCount,
            e.DbQueryDurationMs,
            e.CacheHitCount,
            e.CacheMissCount,
            e.Layer,
            e.ServerName,
            e.ApplicationName
        });

        await connection.ExecuteAsync(sql, parameters, transaction);
    }

    private static async Task InsertExceptionLogsAsync(
        SqlConnection connection,
        IDbTransaction transaction,
        IEnumerable<ExceptionLogEntry> entries)
    {
        const string sql = @"
            INSERT INTO [Log].[ExceptionLogs]
            ([LogId], [CorrelationId], [Timestamp], [LogLevel], [ExceptionType], [ExceptionMessage],
             [StackTrace], [Source], [InnerExceptionType], [InnerExceptionMessage], [Layer],
             [ClassName], [MethodName], [RequestPath], [HttpMethod], [ExceptionCategory], 
             [IsHandled], [IsTransient], [ServerName], [ClientIp], [UserId], [ApplicationName])
            VALUES
            (@LogId, @CorrelationId, @Timestamp, @LogLevel, @ExceptionType, @ExceptionMessage,
             @StackTrace, @Source, @InnerExceptionType, @InnerExceptionMessage, @Layer,
             @ClassName, @MethodName, @RequestPath, @HttpMethod, @ExceptionCategory,
             @IsHandled, @IsTransient, @ServerName, @ClientIp, @UserId, @ApplicationName)";

        var parameters = entries.Select(e => new
        {
            e.LogId,
            e.CorrelationId,
            e.Timestamp,
            e.LogLevel,
            e.ExceptionType,
            e.ExceptionMessage,
            e.StackTrace,
            e.Source,
            e.InnerExceptionType,
            e.InnerExceptionMessage,
            e.Layer,
            e.ClassName,
            e.MethodName,
            e.RequestPath,
            e.HttpMethod,
            e.ExceptionCategory,
            e.IsHandled,
            e.IsTransient,
            e.ServerName,
            e.ClientIp,
            e.UserId,
            e.ApplicationName
        });

        await connection.ExecuteAsync(sql, parameters, transaction);
    }

    private static async Task InsertBusinessExceptionLogsAsync(
        SqlConnection connection,
        IDbTransaction transaction,
        IEnumerable<BusinessExceptionLogEntry> entries)
    {
        const string sql = @"
            INSERT INTO [Log].[BusinessExceptionLogs]
            ([LogId], [CorrelationId], [Timestamp], [BusinessOperation], [BusinessErrorCode],
             [BusinessErrorMessage], [UserFriendlyMessage], [SuggestedAction], [AffectedEntity],
             [AffectedEntityId], [RuleName], [ValidationErrors], [Layer], [ClassName],
             [ServerName], [ClientIp], [UserId], [ApplicationName])
            VALUES
            (@LogId, @CorrelationId, @Timestamp, @BusinessOperation, @BusinessErrorCode,
             @BusinessErrorMessage, @UserFriendlyMessage, @SuggestedAction, @AffectedEntity,
             @AffectedEntityId, @RuleName, @ValidationErrors, @Layer, @ClassName,
             @ServerName, @ClientIp, @UserId, @ApplicationName)";

        var parameters = entries.Select(e => new
        {
            e.LogId,
            e.CorrelationId,
            e.Timestamp,
            e.BusinessOperation,
            e.BusinessErrorCode,
            e.BusinessErrorMessage,
            e.UserFriendlyMessage,
            e.SuggestedAction,
            e.AffectedEntity,
            e.AffectedEntityId,
            e.RuleName,
            ValidationErrors = e.ValidationErrors != null ? JsonSerializer.Serialize(e.ValidationErrors) : null,
            e.Layer,
            e.ClassName,
            e.ServerName,
            e.ClientIp,
            e.UserId,
            e.ApplicationName
        });

        await connection.ExecuteAsync(sql, parameters, transaction);
    }

    private static async Task InsertAuditLogsAsync(
        SqlConnection connection,
        IDbTransaction transaction,
        IEnumerable<AuditLogEntry> entries)
    {
        const string sql = @"
            INSERT INTO [Log].[AuditLogs]
            ([LogId], [CorrelationId], [Timestamp], [Action], [EntityType], [EntityId],
             [OldValues], [NewValues], [Changes], [IsSuccess], [FailureReason], [DurationMs],
             [Layer], [UserId], [ClientIp], [ServerName], [ApplicationName])
            VALUES
            (@LogId, @CorrelationId, @Timestamp, @Action, @EntityType, @EntityId,
             @OldValues, @NewValues, @Changes, @IsSuccess, @FailureReason, @DurationMs,
             @Layer, @UserId, @ClientIp, @ServerName, @ApplicationName)";

        var parameters = entries.Select(e => new
        {
            e.LogId,
            e.CorrelationId,
            e.Timestamp,
            e.Action,
            e.EntityType,
            e.EntityId,
            e.OldValues,
            e.NewValues,
            Changes = e.Changes != null ? JsonSerializer.Serialize(e.Changes) : null,
            e.IsSuccess,
            e.FailureReason,
            e.DurationMs,
            e.Layer,
            e.UserId,
            e.ClientIp,
            e.ServerName,
            e.ApplicationName
        });

        await connection.ExecuteAsync(sql, parameters, transaction);
    }
}

