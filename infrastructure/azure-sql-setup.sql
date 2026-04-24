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
PRINT 'Next steps:';
PRINT '1. Update the connection string in Azure App Service configuration';
PRINT '2. Store credentials in Azure Key Vault';
PRINT '3. Run EF Core migrations: dotnet ef database update';
PRINT '=============================================';
GO
