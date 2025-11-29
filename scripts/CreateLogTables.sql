-- ============================================
-- Enterprise Log Database Schema
-- Version: 2.0.0
-- Date: 2024
-- TimeZone: Turkey Standard Time (UTC+3)
-- ============================================

USE [EnterpriseLogs]
GO

-- Schema oluştur
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Log')
BEGIN
    EXEC('CREATE SCHEMA [Log]')
END
GO

-- ============================================
-- Türkiye saati dönüşüm fonksiyonu
-- ============================================
IF OBJECT_ID('dbo.fn_ToTurkeyTime', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_ToTurkeyTime
GO

CREATE FUNCTION dbo.fn_ToTurkeyTime(@utcDate DATETIME2)
RETURNS DATETIME2
AS
BEGIN
    -- Türkiye UTC+3 (Yaz saati uygulaması kaldırıldı)
    RETURN DATEADD(HOUR, 3, @utcDate)
END
GO

-- ============================================
-- Request Logs Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RequestLogs' AND schema_id = SCHEMA_ID('Log'))
BEGIN
    CREATE TABLE [Log].[RequestLogs] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [LogId] NVARCHAR(50) NOT NULL,
        [CorrelationId] NVARCHAR(100) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL DEFAULT DATEADD(HOUR, 3, GETUTCDATE()), -- Türkiye saati
        [TimestampUtc] DATETIME2(3) NOT NULL DEFAULT GETUTCDATE(),
        [HttpMethod] NVARCHAR(10) NULL,
        [RequestPath] NVARCHAR(2000) NULL,
        [QueryString] NVARCHAR(MAX) NULL,
        [RequestBody] NVARCHAR(MAX) NULL,
        [RequestHeaders] NVARCHAR(MAX) NULL,
        [ContentType] NVARCHAR(200) NULL,
        [ContentLength] BIGINT NULL,
        [ClientIp] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [UserId] NVARCHAR(100) NULL,
        [Layer] NVARCHAR(50) NOT NULL,
        [ServerName] NVARCHAR(100) NULL,
        [ApplicationName] NVARCHAR(100) NOT NULL,
        [LogDate] AS CAST([Timestamp] AS DATE) PERSISTED,
        CONSTRAINT [PK_RequestLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    CREATE NONCLUSTERED INDEX [IX_RequestLogs_CorrelationId] 
        ON [Log].[RequestLogs]([CorrelationId]) 
        INCLUDE ([Timestamp], [RequestPath])

    CREATE NONCLUSTERED INDEX [IX_RequestLogs_Timestamp] 
        ON [Log].[RequestLogs]([Timestamp])

    CREATE NONCLUSTERED INDEX [IX_RequestLogs_LogDate] 
        ON [Log].[RequestLogs]([LogDate])
END
GO

-- ============================================
-- Response Logs Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ResponseLogs' AND schema_id = SCHEMA_ID('Log'))
BEGIN
    CREATE TABLE [Log].[ResponseLogs] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [LogId] NVARCHAR(50) NOT NULL,
        [CorrelationId] NVARCHAR(100) NOT NULL,
        [RequestLogId] NVARCHAR(50) NULL,
        [Timestamp] DATETIME2(3) NOT NULL DEFAULT DATEADD(HOUR, 3, GETUTCDATE()), -- Türkiye saati
        [TimestampUtc] DATETIME2(3) NOT NULL DEFAULT GETUTCDATE(),
        [StatusCode] INT NOT NULL,
        [StatusDescription] NVARCHAR(200) NULL,
        [ResponseBody] NVARCHAR(MAX) NULL,
        [ResponseHeaders] NVARCHAR(MAX) NULL,
        [ContentType] NVARCHAR(200) NULL,
        [ContentLength] BIGINT NULL,
        [DurationMs] BIGINT NOT NULL,
        [DbQueryCount] INT NULL,
        [DbQueryDurationMs] BIGINT NULL,
        [CacheHitCount] INT NULL,
        [CacheMissCount] INT NULL,
        [Layer] NVARCHAR(50) NOT NULL,
        [ServerName] NVARCHAR(100) NULL,
        [ApplicationName] NVARCHAR(100) NOT NULL,
        [LogDate] AS CAST([Timestamp] AS DATE) PERSISTED,
        CONSTRAINT [PK_ResponseLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    CREATE NONCLUSTERED INDEX [IX_ResponseLogs_CorrelationId] 
        ON [Log].[ResponseLogs]([CorrelationId]) 
        INCLUDE ([Timestamp], [StatusCode], [DurationMs])

    CREATE NONCLUSTERED INDEX [IX_ResponseLogs_Timestamp] 
        ON [Log].[ResponseLogs]([Timestamp])

    CREATE NONCLUSTERED INDEX [IX_ResponseLogs_DurationMs] 
        ON [Log].[ResponseLogs]([DurationMs]) 
        WHERE [DurationMs] > 1000  -- Slow requests
END
GO

-- ============================================
-- Exception Logs Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExceptionLogs' AND schema_id = SCHEMA_ID('Log'))
BEGIN
    CREATE TABLE [Log].[ExceptionLogs] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [LogId] NVARCHAR(50) NOT NULL,
        [CorrelationId] NVARCHAR(100) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL DEFAULT DATEADD(HOUR, 3, GETUTCDATE()), -- Türkiye saati
        [TimestampUtc] DATETIME2(3) NOT NULL DEFAULT GETUTCDATE(),
        [LogLevel] NVARCHAR(20) NOT NULL DEFAULT 'Error',
        [ExceptionType] NVARCHAR(500) NULL,
        [ExceptionMessage] NVARCHAR(MAX) NULL,
        [StackTrace] NVARCHAR(MAX) NULL,
        [Source] NVARCHAR(500) NULL,
        [HResult] INT NULL,
        [InnerExceptionType] NVARCHAR(500) NULL,
        [InnerExceptionMessage] NVARCHAR(MAX) NULL,
        [InnerStackTrace] NVARCHAR(MAX) NULL,
        [Layer] NVARCHAR(50) NOT NULL,
        [ClassName] NVARCHAR(200) NULL,
        [MethodName] NVARCHAR(200) NULL,
        [RequestPath] NVARCHAR(2000) NULL,
        [HttpMethod] NVARCHAR(10) NULL,
        [ExceptionCategory] NVARCHAR(50) NULL,
        [IsHandled] BIT NOT NULL DEFAULT 0,
        [IsTransient] BIT NOT NULL DEFAULT 0,
        [RetryCount] INT NULL,
        [ServerName] NVARCHAR(100) NULL,
        [ClientIp] NVARCHAR(50) NULL,
        [UserId] NVARCHAR(100) NULL,
        [ApplicationName] NVARCHAR(100) NOT NULL,
        [LogDate] AS CAST([Timestamp] AS DATE) PERSISTED,
        CONSTRAINT [PK_ExceptionLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    CREATE NONCLUSTERED INDEX [IX_ExceptionLogs_CorrelationId] 
        ON [Log].[ExceptionLogs]([CorrelationId]) 
        INCLUDE ([Timestamp], [ExceptionType])

    CREATE NONCLUSTERED INDEX [IX_ExceptionLogs_Timestamp] 
        ON [Log].[ExceptionLogs]([Timestamp])

    CREATE NONCLUSTERED INDEX [IX_ExceptionLogs_ExceptionCategory] 
        ON [Log].[ExceptionLogs]([ExceptionCategory], [Timestamp])
END
GO

-- ============================================
-- Business Exception Logs Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BusinessExceptionLogs' AND schema_id = SCHEMA_ID('Log'))
BEGIN
    CREATE TABLE [Log].[BusinessExceptionLogs] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [LogId] NVARCHAR(50) NOT NULL,
        [CorrelationId] NVARCHAR(100) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL DEFAULT DATEADD(HOUR, 3, GETUTCDATE()), -- Türkiye saati
        [TimestampUtc] DATETIME2(3) NOT NULL DEFAULT GETUTCDATE(),
        [BusinessOperation] NVARCHAR(200) NULL,
        [BusinessErrorCode] NVARCHAR(50) NULL,
        [BusinessErrorMessage] NVARCHAR(MAX) NULL,
        [UserFriendlyMessage] NVARCHAR(500) NULL,
        [SuggestedAction] NVARCHAR(500) NULL,
        [AffectedEntity] NVARCHAR(200) NULL,
        [AffectedEntityId] NVARCHAR(100) NULL,
        [RuleName] NVARCHAR(200) NULL,
        [RuleDescription] NVARCHAR(500) NULL,
        [ValidationErrors] NVARCHAR(MAX) NULL,
        [Layer] NVARCHAR(50) NOT NULL,
        [ClassName] NVARCHAR(200) NULL,
        [ServerName] NVARCHAR(100) NULL,
        [ClientIp] NVARCHAR(50) NULL,
        [UserId] NVARCHAR(100) NULL,
        [ApplicationName] NVARCHAR(100) NOT NULL,
        [LogDate] AS CAST([Timestamp] AS DATE) PERSISTED,
        CONSTRAINT [PK_BusinessExceptionLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    CREATE NONCLUSTERED INDEX [IX_BusinessExceptionLogs_CorrelationId] 
        ON [Log].[BusinessExceptionLogs]([CorrelationId])

    CREATE NONCLUSTERED INDEX [IX_BusinessExceptionLogs_BusinessErrorCode] 
        ON [Log].[BusinessExceptionLogs]([BusinessErrorCode], [Timestamp])
END
GO

-- ============================================
-- Audit Logs Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs' AND schema_id = SCHEMA_ID('Log'))
BEGIN
    CREATE TABLE [Log].[AuditLogs] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [LogId] NVARCHAR(50) NOT NULL,
        [CorrelationId] NVARCHAR(100) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL DEFAULT DATEADD(HOUR, 3, GETUTCDATE()), -- Türkiye saati
        [TimestampUtc] DATETIME2(3) NOT NULL DEFAULT GETUTCDATE(),
        [Action] NVARCHAR(50) NULL,
        [EntityType] NVARCHAR(200) NULL,
        [EntityId] NVARCHAR(100) NULL,
        [OldValues] NVARCHAR(MAX) NULL,
        [NewValues] NVARCHAR(MAX) NULL,
        [Changes] NVARCHAR(MAX) NULL,
        [IsSuccess] BIT NOT NULL DEFAULT 1,
        [FailureReason] NVARCHAR(500) NULL,
        [DurationMs] BIGINT NULL,
        [Layer] NVARCHAR(50) NOT NULL,
        [UserId] NVARCHAR(100) NULL,
        [ClientIp] NVARCHAR(50) NULL,
        [ServerName] NVARCHAR(100) NULL,
        [ApplicationName] NVARCHAR(100) NOT NULL,
        [LogDate] AS CAST([Timestamp] AS DATE) PERSISTED,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    CREATE NONCLUSTERED INDEX [IX_AuditLogs_CorrelationId] 
        ON [Log].[AuditLogs]([CorrelationId])

    CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityType_EntityId] 
        ON [Log].[AuditLogs]([EntityType], [EntityId])

    CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId] 
        ON [Log].[AuditLogs]([UserId], [Timestamp])
END
GO

PRINT 'Log tables created successfully!'
GO

