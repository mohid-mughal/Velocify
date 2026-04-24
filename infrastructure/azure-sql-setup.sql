-- =============================================
-- Velocify Platform - Azure SQL Database Setup
-- =============================================
-- This script bootstraps the Azure SQL Database for the Velocify platform.
-- It creates the database, configures serverless settings, and sets up
-- initial security configurations.
--
-- Prerequisites:
-- - Azure SQL Server must already exist
-- - Run this script with admin credentials
-- - Update the database name and settings as needed
--
-- Usage:
-- 1. Connect to your Azure SQL Server using SSMS or Azure Data Studio
-- 2. Run this script against the 'master' database
-- 3. The script will create the VelocifyDB database and configure it
-- 4. Update your application connection string with the credentials
-- 5. Run EF Core migrations: dotnet ef database update
--
-- Important Notes:
-- - This script is idempotent (safe to run multiple times)
-- - Serverless tier auto-pauses after 60 minutes of inactivity
-- - Serverless tier auto-resumes on first connection
-- - Min capacity: 0.5 vCores (paused), Max capacity: 2 vCores (active)
-- =============================================

-- Create the database with serverless configuration
-- Serverless tier auto-pauses when inactive and auto-resumes on connection
-- Min capacity: 0.5 vCores (paused state)
-- Max capacity: 2 vCores (active state)
-- Auto-pause delay: 60 minutes of inactivity
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'VelocifyDB')
BEGIN
    CREATE DATABASE VelocifyDB
    (
        EDITION = 'GeneralPurpose',
        SERVICE_OBJECTIVE = 'GP_S_Gen5_2',
        MAXSIZE = 32 GB
    );
    PRINT 'Database VelocifyDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database VelocifyDB already exists.';
END
GO

-- Switch to the Velocify database
USE VelocifyDB;
GO

-- =============================================
-- Configure Database Settings
-- =============================================

-- Enable Query Store for performance monitoring
-- Query Store captures query execution plans and runtime statistics
-- Useful for identifying performance regressions after deployments
ALTER DATABASE CURRENT SET QUERY_STORE = ON;
ALTER DATABASE CURRENT SET QUERY_STORE (
    OPERATION_MODE = READ_WRITE,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    MAX_STORAGE_SIZE_MB = 1000,
    INTERVAL_LENGTH_MINUTES = 60
);
PRINT 'Query Store enabled.';
GO

-- Enable automatic tuning for performance optimization
-- Azure SQL can automatically create and drop indexes based on workload
ALTER DATABASE CURRENT SET AUTOMATIC_TUNING (FORCE_LAST_GOOD_PLAN = ON);
PRINT 'Automatic tuning enabled.';
GO

-- =============================================
-- Create Application User
-- =============================================
-- Create a dedicated user for the application with minimal required permissions
-- Replace 'VelocifyAppUser' and password with secure credentials
-- Store the password in Azure Key Vault or GitHub Secrets

IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'VelocifyAppUser')
BEGIN
    -- Create login at server level (run this on master database first)
    -- CREATE LOGIN VelocifyAppUser WITH PASSWORD = 'YourSecurePassword123!';
    
    -- Create user in the database
    CREATE USER VelocifyAppUser FOR LOGIN VelocifyAppUser;
    PRINT 'User VelocifyAppUser created.';
END
ELSE
BEGIN
    PRINT 'User VelocifyAppUser already exists.';
END
GO

-- Grant necessary permissions to the application user
-- db_datareader: Read data from all tables
-- db_datawriter: Insert, update, delete data in all tables
-- db_ddladmin: Required for EF Core migrations to create/alter tables
ALTER ROLE db_datareader ADD MEMBER VelocifyAppUser;
ALTER ROLE db_datawriter ADD MEMBER VelocifyAppUser;
ALTER ROLE db_ddladmin ADD MEMBER VelocifyAppUser;
PRINT 'Permissions granted to VelocifyAppUser.';
GO

-- Grant EXECUTE permission for stored procedures
GRANT EXECUTE TO VelocifyAppUser;
PRINT 'EXECUTE permission granted to VelocifyAppUser.';
GO

-- =============================================
-- Connection String Template
-- =============================================
-- Use this connection string format in your application:
--
-- CRITICAL: Must include connection pooling parameters for Azure App Service F1 tier
--
-- Server=tcp:your-server.database.windows.net,1433;
-- Initial Catalog=VelocifyDB;
-- Persist Security Info=False;
-- User ID=VelocifyAppUser;
-- Password={your_password};
-- MultipleActiveResultSets=True;
-- Encrypt=True;
-- TrustServerCertificate=False;
-- Connection Timeout=30;
-- Min Pool Size=2;
-- Max Pool Size=100;
--
-- Connection Pooling Parameters Explained:
-- - Min Pool Size=2: Keeps 2 connections warm to reduce cold start latency
--   and CPU time on Azure App Service F1 tier (60 min/day CPU quota)
-- - Max Pool Size=100: Limits connections to prevent memory exhaustion
--   on F1 tier's 1GB RAM limit
--
-- Store this connection string in:
-- - Azure App Service: Configuration → Application settings → AZURE_SQL_CONNECTION_STRING
-- - GitHub Secrets: AZURE_SQL_CONNECTION_STRING (for CI/CD)
-- - Local Development: appsettings.Development.json (DO NOT commit to source control)
-- =============================================

-- =============================================
-- Verify Setup
-- =============================================
SELECT 
    'Database Setup Complete' AS Status,
    DB_NAME() AS DatabaseName,
    DATABASEPROPERTYEX(DB_NAME(), 'Edition') AS Edition,
    DATABASEPROPERTYEX(DB_NAME(), 'ServiceObjective') AS ServiceObjective,
    DATABASEPROPERTYEX(DB_NAME(), 'MaxSizeInBytes') / 1024 / 1024 / 1024 AS MaxSizeGB;
GO

-- Display created users
SELECT 
    name AS Username,
    type_desc AS UserType,
    create_date AS CreatedDate
FROM sys.database_principals
WHERE type IN ('S', 'U')
    AND name NOT IN ('dbo', 'guest', 'INFORMATION_SCHEMA', 'sys')
ORDER BY name;
GO

PRINT '=============================================';
PRINT 'Azure SQL Database setup completed successfully!';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Update the connection string in Azure App Service configuration';
PRINT '   - Azure Portal → App Service → Configuration → Application settings';
PRINT '   - Add: AZURE_SQL_CONNECTION_STRING';
PRINT '   - CRITICAL: Include Min Pool Size=2;Max Pool Size=100;';
PRINT '';
PRINT '2. Store credentials securely:';
PRINT '   - Azure Key Vault (recommended for production)';
PRINT '   - GitHub Secrets (for CI/CD deployment)';
PRINT '   - Never commit credentials to source control';
PRINT '';
PRINT '3. Configure Azure SQL firewall:';
PRINT '   - Azure Portal → SQL Server → Networking';
PRINT '   - Enable "Allow Azure services and resources to access this server"';
PRINT '   - Or use Azure CLI:';
PRINT '     az sql server firewall-rule create --resource-group velocify-rg \';
PRINT '       --server your-server --name AllowAzureServices \';
PRINT '       --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0';
PRINT '';
PRINT '4. Run EF Core migrations from your backend project:';
PRINT '   cd backend';
PRINT '   dotnet ef database update --project Velocify.Infrastructure';
PRINT '';
PRINT '5. Verify deployment:';
PRINT '   - Check health endpoint: https://your-app.azurewebsites.net/health';
PRINT '   - Monitor database connections in Azure Portal';
PRINT '   - Review Application Insights for errors';
PRINT '';
PRINT 'For detailed setup instructions, see:';
PRINT '- backend/AZURE-APP-SERVICE-SETUP.md';
PRINT '- backend/ENVIRONMENT-VARIABLES.md';
PRINT '- backend/AZURE-F1-TIER-BEST-PRACTICES.md';
PRINT '=============================================';
GO
