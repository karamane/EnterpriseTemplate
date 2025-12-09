-- ============================================
-- SQL Server DDL Scripts
-- Enterprise Template
-- Version: 2.0.0
-- Date: 2025-12-05
-- ============================================
-- Execution Order:
--   1. Drop existing objects (optional)
--   2. Business Tables (CUSTOMERS, ORDERS, ORDER_ITEMS)
--   3. Auth Tables (USERS, REFRESH_TOKENS)
--   4. Log Tables (LOG_*)
--   5. Views
--   6. Seed Data (Demo Users)
-- ============================================

-- ============================================
-- SECTION 1: DROP EXISTING OBJECTS (Optional)
-- ============================================
/*
-- Uncomment to drop existing tables before recreation
IF OBJECT_ID('[dbo].[ORDER_ITEMS]', 'U') IS NOT NULL DROP TABLE [dbo].[ORDER_ITEMS];
IF OBJECT_ID('[dbo].[ORDERS]', 'U') IS NOT NULL DROP TABLE [dbo].[ORDERS];
IF OBJECT_ID('[dbo].[CUSTOMERS]', 'U') IS NOT NULL DROP TABLE [dbo].[CUSTOMERS];
IF OBJECT_ID('[dbo].[REFRESH_TOKENS]', 'U') IS NOT NULL DROP TABLE [dbo].[REFRESH_TOKENS];
IF OBJECT_ID('[dbo].[USERS]', 'U') IS NOT NULL DROP TABLE [dbo].[USERS];
IF OBJECT_ID('[dbo].[LOG_REQUEST_LOGS]', 'U') IS NOT NULL DROP TABLE [dbo].[LOG_REQUEST_LOGS];
IF OBJECT_ID('[dbo].[LOG_RESPONSE_LOGS]', 'U') IS NOT NULL DROP TABLE [dbo].[LOG_RESPONSE_LOGS];
IF OBJECT_ID('[dbo].[LOG_EXCEPTION_LOGS]', 'U') IS NOT NULL DROP TABLE [dbo].[LOG_EXCEPTION_LOGS];
IF OBJECT_ID('[dbo].[LOG_BUSINESS_EXCEPTION_LOGS]', 'U') IS NOT NULL DROP TABLE [dbo].[LOG_BUSINESS_EXCEPTION_LOGS];
IF OBJECT_ID('[dbo].[LOG_AUDIT_LOGS]', 'U') IS NOT NULL DROP TABLE [dbo].[LOG_AUDIT_LOGS];
GO
*/

-- ============================================
-- SECTION 2: BUSINESS TABLES
-- ============================================

-- ============================================
-- 2.1 CUSTOMERS Table
-- ============================================
CREATE TABLE [dbo].[CUSTOMERS] (
    [ID]                    BIGINT          IDENTITY(1,1) NOT NULL,
    [FIRST_NAME]            NVARCHAR(100)   NOT NULL,
    [LAST_NAME]             NVARCHAR(100)   NOT NULL,
    [EMAIL]                 NVARCHAR(256)   NOT NULL,
    [PHONE_NUMBER]          NVARCHAR(20)    NULL,
    [IS_ACTIVE]             BIT             NOT NULL DEFAULT 1,
    [REGISTERED_AT]         DATETIME2       NOT NULL,
    [CREATED_AT]            DATETIME2       NOT NULL,
    [CREATED_BY]            NVARCHAR(100)   NULL,
    [UPDATED_AT]            DATETIME2       NULL,
    [UPDATED_BY]            NVARCHAR(100)   NULL,
    [IS_DELETED]            BIT             NOT NULL DEFAULT 0,
    [DELETED_AT]            DATETIME2       NULL,
    [DELETED_BY]            NVARCHAR(100)   NULL,
    CONSTRAINT [PK_CUSTOMERS] PRIMARY KEY CLUSTERED ([ID])
);
GO

-- Indexes
CREATE UNIQUE NONCLUSTERED INDEX [IDX_CUSTOMERS_EMAIL] ON [dbo].[CUSTOMERS]([EMAIL]) WHERE [IS_DELETED] = 0;
CREATE NONCLUSTERED INDEX [IDX_CUSTOMERS_IS_DELETED] ON [dbo].[CUSTOMERS]([IS_DELETED]);
CREATE NONCLUSTERED INDEX [IDX_CUSTOMERS_IS_ACTIVE] ON [dbo].[CUSTOMERS]([IS_ACTIVE]);
GO

-- ============================================
-- 2.2 ORDERS Table
-- ============================================
CREATE TABLE [dbo].[ORDERS] (
    [ID]                    BIGINT          IDENTITY(1,1) NOT NULL,
    [CUSTOMER_ID]           BIGINT          NOT NULL,
    [TOTAL_AMOUNT]          DECIMAL(18,2)   NOT NULL,
    [STATUS]                NVARCHAR(50)    NOT NULL DEFAULT 'Pending',
    [NOTES]                 NVARCHAR(500)   NULL,
    [ORDER_DATE]            DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    [CREATED_AT]            DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    [CREATED_BY]            NVARCHAR(100)   NULL,
    [UPDATED_AT]            DATETIME2       NULL,
    [UPDATED_BY]            NVARCHAR(100)   NULL,
    [IS_DELETED]            BIT             NOT NULL DEFAULT 0,
    [DELETED_AT]            DATETIME2       NULL,
    [DELETED_BY]            NVARCHAR(100)   NULL,
    CONSTRAINT [PK_ORDERS] PRIMARY KEY CLUSTERED ([ID]),
    CONSTRAINT [FK_ORDERS_CUSTOMER] FOREIGN KEY ([CUSTOMER_ID]) REFERENCES [dbo].[CUSTOMERS]([ID])
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IDX_ORDERS_CUSTOMER] ON [dbo].[ORDERS]([CUSTOMER_ID]);
CREATE NONCLUSTERED INDEX [IDX_ORDERS_STATUS] ON [dbo].[ORDERS]([STATUS]);
CREATE NONCLUSTERED INDEX [IDX_ORDERS_DATE] ON [dbo].[ORDERS]([ORDER_DATE]);
CREATE NONCLUSTERED INDEX [IDX_ORDERS_IS_DELETED] ON [dbo].[ORDERS]([IS_DELETED]);
GO

-- ============================================
-- 2.3 ORDER_ITEMS Table
-- ============================================
CREATE TABLE [dbo].[ORDER_ITEMS] (
    [ID]                    BIGINT          IDENTITY(1,1) NOT NULL,
    [ORDER_ID]              BIGINT          NOT NULL,
    [PRODUCT_ID]            INT             NOT NULL,
    [PRODUCT_NAME]          NVARCHAR(200)   NOT NULL,
    [QUANTITY]              INT             NOT NULL,
    [UNIT_PRICE]            DECIMAL(18,2)   NOT NULL,
    [CREATED_AT]            DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    [CREATED_BY]            NVARCHAR(100)   NULL,
    [UPDATED_AT]            DATETIME2       NULL,
    [UPDATED_BY]            NVARCHAR(100)   NULL,
    CONSTRAINT [PK_ORDER_ITEMS] PRIMARY KEY CLUSTERED ([ID]),
    CONSTRAINT [FK_ORDER_ITEMS_ORDER] FOREIGN KEY ([ORDER_ID]) REFERENCES [dbo].[ORDERS]([ID])
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IDX_ORDER_ITEMS_ORDER] ON [dbo].[ORDER_ITEMS]([ORDER_ID]);
CREATE NONCLUSTERED INDEX [IDX_ORDER_ITEMS_PRODUCT] ON [dbo].[ORDER_ITEMS]([PRODUCT_ID]);
GO

-- ============================================
-- SECTION 3: AUTH TABLES
-- ============================================

-- ============================================
-- 3.1 USERS Table
-- ============================================
CREATE TABLE [dbo].[USERS] (
    [ID]                    BIGINT          IDENTITY(1,1) NOT NULL,
    [USERNAME]              NVARCHAR(100)   NOT NULL,
    [PASSWORD_HASH]         NVARCHAR(256)   NOT NULL,
    [EMAIL]                 NVARCHAR(256)   NOT NULL,
    [FULL_NAME]             NVARCHAR(200)   NULL,
    [ROLES]                 NVARCHAR(500)   NULL,
    [IS_ACTIVE]             BIT             NOT NULL DEFAULT 1,
    [LAST_LOGIN_AT]         DATETIME2       NULL,
    [CREATED_AT]            DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    [CREATED_BY]            NVARCHAR(100)   NULL,
    [UPDATED_AT]            DATETIME2       NULL,
    [UPDATED_BY]            NVARCHAR(100)   NULL,
    [IS_DELETED]            BIT             NOT NULL DEFAULT 0,
    [DELETED_AT]            DATETIME2       NULL,
    [DELETED_BY]            NVARCHAR(100)   NULL,
    CONSTRAINT [PK_USERS] PRIMARY KEY CLUSTERED ([ID])
);
GO

-- Indexes
CREATE UNIQUE NONCLUSTERED INDEX [IDX_USERS_USERNAME] ON [dbo].[USERS]([USERNAME]) WHERE [IS_DELETED] = 0;
CREATE UNIQUE NONCLUSTERED INDEX [IDX_USERS_EMAIL] ON [dbo].[USERS]([EMAIL]) WHERE [IS_DELETED] = 0;
CREATE NONCLUSTERED INDEX [IDX_USERS_IS_ACTIVE] ON [dbo].[USERS]([IS_ACTIVE]);
CREATE NONCLUSTERED INDEX [IDX_USERS_IS_DELETED] ON [dbo].[USERS]([IS_DELETED]);
GO

-- ============================================
-- 3.2 REFRESH_TOKENS Table
-- ============================================
CREATE TABLE [dbo].[REFRESH_TOKENS] (
    [ID]                    BIGINT          IDENTITY(1,1) NOT NULL,
    [USER_ID]               BIGINT          NOT NULL,
    [TOKEN]                 NVARCHAR(500)   NOT NULL,
    [EXPIRES_AT]            DATETIME2       NOT NULL,
    [CREATED_AT]            DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    [CREATED_BY_IP]         NVARCHAR(50)    NULL,
    [REVOKED_AT]            DATETIME2       NULL,
    [REVOKED_BY_IP]         NVARCHAR(50)    NULL,
    [REPLACED_BY_TOKEN]     NVARCHAR(500)   NULL,
    [REVOKED_REASON]        NVARCHAR(500)   NULL,
    CONSTRAINT [PK_REFRESH_TOKENS] PRIMARY KEY CLUSTERED ([ID]),
    CONSTRAINT [FK_REFRESH_TOKENS_USERS] FOREIGN KEY ([USER_ID]) REFERENCES [dbo].[USERS]([ID]) ON DELETE CASCADE
);
GO

-- Indexes
CREATE UNIQUE NONCLUSTERED INDEX [IDX_REFRESH_TOKENS_TOKEN] ON [dbo].[REFRESH_TOKENS]([TOKEN]);
CREATE NONCLUSTERED INDEX [IDX_REFRESH_TOKENS_USER_ID] ON [dbo].[REFRESH_TOKENS]([USER_ID]);
CREATE NONCLUSTERED INDEX [IDX_REFRESH_TOKENS_EXPIRES_AT] ON [dbo].[REFRESH_TOKENS]([EXPIRES_AT]);
GO

-- ============================================
-- SECTION 4: LOG TABLES
-- ============================================

-- ============================================
-- 4.1 LOG_REQUEST_LOGS - HTTP Request bilgileri
-- ============================================
CREATE TABLE [dbo].[LOG_REQUEST_LOGS] (
    [LOG_ID]                NVARCHAR(36)    NOT NULL,
    [CORRELATION_ID]        NVARCHAR(36)    NOT NULL,
    [TIMESTAMP]             DATETIME2       NOT NULL,
    [HTTP_METHOD]           NVARCHAR(10)    NULL,
    [REQUEST_PATH]          NVARCHAR(2000)  NULL,
    [QUERY_STRING]          NVARCHAR(4000)  NULL,
    [REQUEST_BODY]          NVARCHAR(MAX)   NULL,
    [REQUEST_HEADERS]       NVARCHAR(MAX)   NULL,
    [CONTENT_TYPE]          NVARCHAR(200)   NULL,
    [CONTENT_LENGTH]        BIGINT          NULL,
    [CLIENT_IP]             NVARCHAR(50)    NULL,
    [USER_AGENT]            NVARCHAR(500)   NULL,
    [USER_ID]               NVARCHAR(100)   NULL,
    [LAYER]                 NVARCHAR(50)    NULL,
    [SERVER_NAME]           NVARCHAR(100)   NULL,
    [APPLICATION_NAME]      NVARCHAR(100)   NULL,
    CONSTRAINT [PK_LOG_REQUEST_LOGS] PRIMARY KEY CLUSTERED ([LOG_ID])
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IDX_REQUEST_LOGS_CORRELATION] ON [dbo].[LOG_REQUEST_LOGS]([CORRELATION_ID]);
CREATE NONCLUSTERED INDEX [IDX_REQUEST_LOGS_TIMESTAMP] ON [dbo].[LOG_REQUEST_LOGS]([TIMESTAMP]);
CREATE NONCLUSTERED INDEX [IDX_REQUEST_LOGS_USER] ON [dbo].[LOG_REQUEST_LOGS]([USER_ID]);
CREATE NONCLUSTERED INDEX [IDX_REQUEST_LOGS_PATH] ON [dbo].[LOG_REQUEST_LOGS]([REQUEST_PATH]);
GO

-- ============================================
-- 4.2 LOG_RESPONSE_LOGS - HTTP Response bilgileri
-- ============================================
CREATE TABLE [dbo].[LOG_RESPONSE_LOGS] (
    [LOG_ID]                NVARCHAR(36)    NOT NULL,
    [CORRELATION_ID]        NVARCHAR(36)    NOT NULL,
    [REQUEST_LOG_ID]        NVARCHAR(36)    NULL,
    [TIMESTAMP]             DATETIME2       NOT NULL,
    [STATUS_CODE]           INT             NULL,
    [DURATION_MS]           BIGINT          NULL,
    [RESPONSE_BODY]         NVARCHAR(MAX)   NULL,
    [CONTENT_TYPE]          NVARCHAR(200)   NULL,
    [DB_QUERY_COUNT]        INT             NULL,
    [DB_QUERY_DURATION_MS]  BIGINT          NULL,
    [CACHE_HIT_COUNT]       INT             NULL,
    [CACHE_MISS_COUNT]      INT             NULL,
    [LAYER]                 NVARCHAR(50)    NULL,
    [SERVER_NAME]           NVARCHAR(100)   NULL,
    [APPLICATION_NAME]      NVARCHAR(100)   NULL,
    CONSTRAINT [PK_LOG_RESPONSE_LOGS] PRIMARY KEY CLUSTERED ([LOG_ID])
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IDX_RESPONSE_LOGS_CORRELATION] ON [dbo].[LOG_RESPONSE_LOGS]([CORRELATION_ID]);
CREATE NONCLUSTERED INDEX [IDX_RESPONSE_LOGS_REQUEST] ON [dbo].[LOG_RESPONSE_LOGS]([REQUEST_LOG_ID]);
CREATE NONCLUSTERED INDEX [IDX_RESPONSE_LOGS_TIMESTAMP] ON [dbo].[LOG_RESPONSE_LOGS]([TIMESTAMP]);
CREATE NONCLUSTERED INDEX [IDX_RESPONSE_LOGS_STATUS] ON [dbo].[LOG_RESPONSE_LOGS]([STATUS_CODE]);
CREATE NONCLUSTERED INDEX [IDX_RESPONSE_LOGS_DURATION] ON [dbo].[LOG_RESPONSE_LOGS]([DURATION_MS]);
GO

-- ============================================
-- 4.3 LOG_EXCEPTION_LOGS - Sistem hataları
-- ============================================
CREATE TABLE [dbo].[LOG_EXCEPTION_LOGS] (
    [LOG_ID]                NVARCHAR(36)    NOT NULL,
    [CORRELATION_ID]        NVARCHAR(36)    NOT NULL,
    [TIMESTAMP]             DATETIME2       NOT NULL,
    [LOG_LEVEL]             NVARCHAR(20)    NULL,
    [EXCEPTION_TYPE]        NVARCHAR(500)   NULL,
    [EXCEPTION_MESSAGE]     NVARCHAR(4000)  NULL,
    [STACK_TRACE]           NVARCHAR(MAX)   NULL,
    [SOURCE]                NVARCHAR(500)   NULL,
    [INNER_EXCEPTION_TYPE]  NVARCHAR(500)   NULL,
    [INNER_EXCEPTION_MESSAGE] NVARCHAR(4000) NULL,
    [LAYER]                 NVARCHAR(50)    NULL,
    [CLASS_NAME]            NVARCHAR(500)   NULL,
    [METHOD_NAME]           NVARCHAR(200)   NULL,
    [REQUEST_PATH]          NVARCHAR(2000)  NULL,
    [HTTP_METHOD]           NVARCHAR(10)    NULL,
    [EXCEPTION_CATEGORY]    NVARCHAR(100)   NULL,
    [IS_HANDLED]            BIT             NULL,
    [IS_TRANSIENT]          BIT             NULL,
    [SERVER_NAME]           NVARCHAR(100)   NULL,
    [CLIENT_IP]             NVARCHAR(50)    NULL,
    [USER_ID]               NVARCHAR(100)   NULL,
    [APPLICATION_NAME]      NVARCHAR(100)   NULL,
    CONSTRAINT [PK_LOG_EXCEPTION_LOGS] PRIMARY KEY CLUSTERED ([LOG_ID])
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IDX_EXCEPTION_LOGS_CORRELATION] ON [dbo].[LOG_EXCEPTION_LOGS]([CORRELATION_ID]);
CREATE NONCLUSTERED INDEX [IDX_EXCEPTION_LOGS_TIMESTAMP] ON [dbo].[LOG_EXCEPTION_LOGS]([TIMESTAMP]);
CREATE NONCLUSTERED INDEX [IDX_EXCEPTION_LOGS_TYPE] ON [dbo].[LOG_EXCEPTION_LOGS]([EXCEPTION_TYPE]);
CREATE NONCLUSTERED INDEX [IDX_EXCEPTION_LOGS_LEVEL] ON [dbo].[LOG_EXCEPTION_LOGS]([LOG_LEVEL]);
CREATE NONCLUSTERED INDEX [IDX_EXCEPTION_LOGS_CATEGORY] ON [dbo].[LOG_EXCEPTION_LOGS]([EXCEPTION_CATEGORY]);
GO

-- ============================================
-- 4.4 LOG_BUSINESS_EXCEPTION_LOGS - İş kuralı hataları
-- ============================================
CREATE TABLE [dbo].[LOG_BUSINESS_EXCEPTION_LOGS] (
    [LOG_ID]                NVARCHAR(36)    NOT NULL,
    [CORRELATION_ID]        NVARCHAR(36)    NOT NULL,
    [TIMESTAMP]             DATETIME2       NOT NULL,
    [BUSINESS_OPERATION]    NVARCHAR(200)   NULL,
    [BUSINESS_ERROR_CODE]   NVARCHAR(50)    NULL,
    [BUSINESS_ERROR_MESSAGE] NVARCHAR(4000) NULL,
    [USER_FRIENDLY_MESSAGE] NVARCHAR(2000)  NULL,
    [SUGGESTED_ACTION]      NVARCHAR(1000)  NULL,
    [AFFECTED_ENTITY]       NVARCHAR(200)   NULL,
    [AFFECTED_ENTITY_ID]    NVARCHAR(100)   NULL,
    [RULE_NAME]             NVARCHAR(200)   NULL,
    [VALIDATION_ERRORS]     NVARCHAR(MAX)   NULL,
    [LAYER]                 NVARCHAR(50)    NULL,
    [CLASS_NAME]            NVARCHAR(500)   NULL,
    [SERVER_NAME]           NVARCHAR(100)   NULL,
    [CLIENT_IP]             NVARCHAR(50)    NULL,
    [USER_ID]               NVARCHAR(100)   NULL,
    [APPLICATION_NAME]      NVARCHAR(100)   NULL,
    CONSTRAINT [PK_LOG_BUSINESS_EXCEPTION] PRIMARY KEY CLUSTERED ([LOG_ID])
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IDX_BUS_EXC_LOGS_CORRELATION] ON [dbo].[LOG_BUSINESS_EXCEPTION_LOGS]([CORRELATION_ID]);
CREATE NONCLUSTERED INDEX [IDX_BUS_EXC_LOGS_TIMESTAMP] ON [dbo].[LOG_BUSINESS_EXCEPTION_LOGS]([TIMESTAMP]);
CREATE NONCLUSTERED INDEX [IDX_BUS_EXC_LOGS_ERROR_CODE] ON [dbo].[LOG_BUSINESS_EXCEPTION_LOGS]([BUSINESS_ERROR_CODE]);
CREATE NONCLUSTERED INDEX [IDX_BUS_EXC_LOGS_OPERATION] ON [dbo].[LOG_BUSINESS_EXCEPTION_LOGS]([BUSINESS_OPERATION]);
CREATE NONCLUSTERED INDEX [IDX_BUS_EXC_LOGS_ENTITY] ON [dbo].[LOG_BUSINESS_EXCEPTION_LOGS]([AFFECTED_ENTITY], [AFFECTED_ENTITY_ID]);
GO

-- ============================================
-- 4.5 LOG_AUDIT_LOGS - Denetim kayıtları
-- ============================================
CREATE TABLE [dbo].[LOG_AUDIT_LOGS] (
    [LOG_ID]                NVARCHAR(36)    NOT NULL,
    [CORRELATION_ID]        NVARCHAR(36)    NOT NULL,
    [TIMESTAMP]             DATETIME2       NOT NULL,
    [ACTION]                NVARCHAR(50)    NULL,
    [ENTITY_TYPE]           NVARCHAR(200)   NULL,
    [ENTITY_ID]             NVARCHAR(100)   NULL,
    [OLD_VALUES]            NVARCHAR(MAX)   NULL,
    [NEW_VALUES]            NVARCHAR(MAX)   NULL,
    [CHANGES]               NVARCHAR(MAX)   NULL,
    [IS_SUCCESS]            BIT             NULL,
    [FAILURE_REASON]        NVARCHAR(2000)  NULL,
    [DURATION_MS]           BIGINT          NULL,
    [LAYER]                 NVARCHAR(50)    NULL,
    [USER_ID]               NVARCHAR(100)   NULL,
    [CLIENT_IP]             NVARCHAR(50)    NULL,
    [SERVER_NAME]           NVARCHAR(100)   NULL,
    [APPLICATION_NAME]      NVARCHAR(100)   NULL,
    CONSTRAINT [PK_LOG_AUDIT_LOGS] PRIMARY KEY CLUSTERED ([LOG_ID])
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IDX_AUDIT_LOGS_CORRELATION] ON [dbo].[LOG_AUDIT_LOGS]([CORRELATION_ID]);
CREATE NONCLUSTERED INDEX [IDX_AUDIT_LOGS_TIMESTAMP] ON [dbo].[LOG_AUDIT_LOGS]([TIMESTAMP]);
CREATE NONCLUSTERED INDEX [IDX_AUDIT_LOGS_ENTITY] ON [dbo].[LOG_AUDIT_LOGS]([ENTITY_TYPE], [ENTITY_ID]);
CREATE NONCLUSTERED INDEX [IDX_AUDIT_LOGS_USER] ON [dbo].[LOG_AUDIT_LOGS]([USER_ID]);
CREATE NONCLUSTERED INDEX [IDX_AUDIT_LOGS_ACTION] ON [dbo].[LOG_AUDIT_LOGS]([ACTION]);
GO

-- ============================================
-- SECTION 5: VIEWS
-- ============================================

-- Son 24 saat hata özeti
CREATE OR ALTER VIEW [dbo].[VW_EXCEPTION_SUMMARY_24H] AS
SELECT 
    DATEADD(HOUR, DATEDIFF(HOUR, 0, [TIMESTAMP]), 0) AS [HOUR],
    [EXCEPTION_TYPE],
    [LOG_LEVEL],
    COUNT(*) AS [ERROR_COUNT]
FROM [dbo].[LOG_EXCEPTION_LOGS]
WHERE [TIMESTAMP] > DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY DATEADD(HOUR, DATEDIFF(HOUR, 0, [TIMESTAMP]), 0), [EXCEPTION_TYPE], [LOG_LEVEL];
GO

-- Request performans özeti
CREATE OR ALTER VIEW [dbo].[VW_REQUEST_PERFORMANCE] AS
SELECT 
    DATEADD(HOUR, DATEDIFF(HOUR, 0, req.[TIMESTAMP]), 0) AS [HOUR],
    req.[REQUEST_PATH],
    COUNT(*) AS [REQUEST_COUNT],
    AVG(res.[DURATION_MS]) AS [AVG_DURATION_MS],
    MAX(res.[DURATION_MS]) AS [MAX_DURATION_MS],
    MIN(res.[DURATION_MS]) AS [MIN_DURATION_MS]
FROM [dbo].[LOG_REQUEST_LOGS] req
JOIN [dbo].[LOG_RESPONSE_LOGS] res ON req.[CORRELATION_ID] = res.[CORRELATION_ID]
WHERE req.[TIMESTAMP] > DATEADD(HOUR, -24, GETUTCDATE())
GROUP BY DATEADD(HOUR, DATEDIFF(HOUR, 0, req.[TIMESTAMP]), 0), req.[REQUEST_PATH];
GO

-- Kullanıcı aktivite özeti
CREATE OR ALTER VIEW [dbo].[VW_USER_ACTIVITY] AS
SELECT 
    [USER_ID],
    COUNT(*) AS [TOTAL_REQUESTS],
    MIN([TIMESTAMP]) AS [FIRST_REQUEST],
    MAX([TIMESTAMP]) AS [LAST_REQUEST]
FROM [dbo].[LOG_REQUEST_LOGS]
WHERE [USER_ID] IS NOT NULL
  AND [TIMESTAMP] > DATEADD(DAY, -7, GETUTCDATE())
GROUP BY [USER_ID];
GO

-- ============================================
-- SECTION 6: SEED DATA
-- ============================================

-- Demo Admin User (admin / Admin123!)
-- BCrypt Hash generated with cost factor 11
INSERT INTO [dbo].[USERS] ([USERNAME], [PASSWORD_HASH], [EMAIL], [FULL_NAME], [ROLES], [IS_ACTIVE], [CREATED_AT])
VALUES ('admin', '$2a$11$UcRiXou1e8p4CI/2SFNu3.cbl67H3dk3Qi8RoJpVwvnSsAbdJ8Jeu', 'admin@enterprise.com', 'System Administrator', 'Admin,User', 1, GETUTCDATE());

-- Demo User (user / User123!)
INSERT INTO [dbo].[USERS] ([USERNAME], [PASSWORD_HASH], [EMAIL], [FULL_NAME], [ROLES], [IS_ACTIVE], [CREATED_AT])
VALUES ('user', '$2a$11$5j6SZSlEEN2VxkrORAmsoePChDpau9mCMB0Rj8Gd16QXd9nVvdclK', 'user@enterprise.com', 'Demo User', 'User', 1, GETUTCDATE());
GO

-- ============================================
-- VERIFICATION
-- ============================================
PRINT 'SQL Server schema created successfully';
PRINT 'Tables: CUSTOMERS, ORDERS, ORDER_ITEMS, USERS, REFRESH_TOKENS, LOG_*';
PRINT 'Demo users: admin/Admin123!, user/User123!';
GO

-- ============================================
-- NOTES
-- ============================================
-- BCrypt hash generation (C#):
--   using BCrypt.Net;
--   var hash = BCrypt.Net.BCrypt.HashPassword("Admin123!", BCrypt.Net.BCrypt.GenerateSalt(11));
--
-- Password verification (C#):
--   bool isValid = BCrypt.Net.BCrypt.Verify("Admin123!", hash);
-- ============================================

