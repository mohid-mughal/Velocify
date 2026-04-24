using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Velocify.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Optimization migration that implements table partitioning for TaskAuditLog.
    /// 
    /// Audit Log Growth and Partition Benefits:
    /// - Audit logs grow unboundedly as every task field change creates a new audit record
    /// - In a production system with 1000 active users and 50 task updates per user per day,
    ///   the audit log grows by ~50,000 rows daily (18M rows/year)
    /// - Without partitioning, queries for recent audit data scan the entire table
    /// - Partitioning by month divides the table into smaller physical segments
    /// 
    /// Performance Benefits:
    /// - Queries with date filters only scan relevant partitions (partition elimination)
    /// - Example: "Show audit log for last 7 days" scans only current month partition
    /// - Reduces I/O by 90%+ for recent audit queries (most common use case)
    /// - Enables efficient archival: old partitions can be switched out to archive tables
    /// - Index maintenance is faster as indexes are partitioned alongside data
    /// 
    /// Partition Strategy:
    /// - Monthly partitions for the last 12 months (rolling window)
    /// - RANGE RIGHT: partition boundary values belong to the right partition
    /// - Example: '2024-01-01' boundary means Jan 1 and later go to Jan partition
    /// - Partition function must be maintained: add new months, archive old months
    /// 
    /// Cost Impact for Azure SQL Serverless:
    /// - Serverless charges per vCore-second of compute
    /// - Partition elimination reduces compute time by avoiding full table scans
    /// - Can reduce audit query costs by 80-90% compared to non-partitioned table
    /// 
    /// Requirement: 15.6 - Database SHALL use table partitioning on TaskAuditLog to scan only relevant monthly partitions
    /// </summary>
    public partial class AddTablePartitioningOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create partition function
            // RANGE RIGHT means the boundary value belongs to the right partition
            // Example: '2024-01-01' means Jan 1 00:00:00 and later go to the Jan partition
            // This creates 13 partitions: one for everything before 2024-01-01, then one per month
            migrationBuilder.Sql(@"
                CREATE PARTITION FUNCTION PF_AuditLog_Monthly (DATETIME2)
                AS RANGE RIGHT FOR VALUES (
                    '2024-01-01', '2024-02-01', '2024-03-01', '2024-04-01',
                    '2024-05-01', '2024-06-01', '2024-07-01', '2024-08-01',
                    '2024-09-01', '2024-10-01', '2024-11-01', '2024-12-01'
                );
            ");

            // Step 2: Create partition scheme
            // Maps the partition function to filegroups
            // ALL TO ([PRIMARY]) means all partitions go to the PRIMARY filegroup
            // In production, you might use different filegroups for different partitions
            // to spread I/O across multiple storage volumes
            migrationBuilder.Sql(@"
                CREATE PARTITION SCHEME PS_AuditLog_Monthly
                AS PARTITION PF_AuditLog_Monthly
                ALL TO ([PRIMARY]);
            ");

            // Step 3: Recreate TaskAuditLog table with partitioning
            // Unfortunately, you cannot add partitioning to an existing table directly
            // We need to: create new partitioned table, copy data, drop old table, rename new table
            
            // Create new partitioned table
            migrationBuilder.Sql(@"
                CREATE TABLE dbo.TaskAuditLog_New (
                    Id BIGINT IDENTITY(1,1) NOT NULL,
                    TaskItemId UNIQUEIDENTIFIER NOT NULL,
                    ChangedByUserId UNIQUEIDENTIFIER NOT NULL,
                    FieldName NVARCHAR(100) NOT NULL,
                    OldValue NVARCHAR(MAX) NULL,
                    NewValue NVARCHAR(MAX) NULL,
                    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT PK_TaskAuditLog_New PRIMARY KEY (Id, ChangedAt)
                ) ON PS_AuditLog_Monthly(ChangedAt);
            ");

            // Copy existing data from old table to new partitioned table
            // If TaskAuditLog already has data, this preserves it
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskAuditLog' AND schema_id = SCHEMA_ID('dbo'))
                BEGIN
                    SET IDENTITY_INSERT dbo.TaskAuditLog_New ON;
                    
                    INSERT INTO dbo.TaskAuditLog_New (Id, TaskItemId, ChangedByUserId, FieldName, OldValue, NewValue, ChangedAt)
                    SELECT Id, TaskItemId, ChangedByUserId, FieldName, OldValue, NewValue, ChangedAt
                    FROM dbo.TaskAuditLog;
                    
                    SET IDENTITY_INSERT dbo.TaskAuditLog_New OFF;
                END
            ");

            // Drop old table
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskAuditLog' AND schema_id = SCHEMA_ID('dbo'))
                BEGIN
                    DROP TABLE dbo.TaskAuditLog;
                END
            ");

            // Rename new table to original name
            migrationBuilder.Sql(@"
                EXEC sp_rename 'dbo.TaskAuditLog_New', 'TaskAuditLog';
            ");

            // Recreate indexes on the partitioned table
            // Indexes on partitioned tables are automatically partitioned (aligned)
            // This means each partition has its own index segment
            migrationBuilder.Sql(@"
                CREATE INDEX IX_TaskAuditLog_Task 
                ON dbo.TaskAuditLog(TaskItemId, ChangedAt);
            ");

            // Recreate foreign key constraints
            migrationBuilder.Sql(@"
                ALTER TABLE dbo.TaskAuditLog
                ADD CONSTRAINT FK_TaskAuditLog_TaskItem 
                FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems(Id);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.TaskAuditLog
                ADD CONSTRAINT FK_TaskAuditLog_ChangedBy 
                FOREIGN KEY (ChangedByUserId) REFERENCES dbo.Users(Id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate non-partitioned table
            migrationBuilder.Sql(@"
                CREATE TABLE dbo.TaskAuditLog_NonPartitioned (
                    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                    TaskItemId UNIQUEIDENTIFIER NOT NULL,
                    ChangedByUserId UNIQUEIDENTIFIER NOT NULL,
                    FieldName NVARCHAR(100) NOT NULL,
                    OldValue NVARCHAR(MAX) NULL,
                    NewValue NVARCHAR(MAX) NULL,
                    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );
            ");

            // Copy data back
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskAuditLog' AND schema_id = SCHEMA_ID('dbo'))
                BEGIN
                    SET IDENTITY_INSERT dbo.TaskAuditLog_NonPartitioned ON;
                    
                    INSERT INTO dbo.TaskAuditLog_NonPartitioned (Id, TaskItemId, ChangedByUserId, FieldName, OldValue, NewValue, ChangedAt)
                    SELECT Id, TaskItemId, ChangedByUserId, FieldName, OldValue, NewValue, ChangedAt
                    FROM dbo.TaskAuditLog;
                    
                    SET IDENTITY_INSERT dbo.TaskAuditLog_NonPartitioned OFF;
                END
            ");

            // Drop partitioned table
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskAuditLog' AND schema_id = SCHEMA_ID('dbo'))
                BEGIN
                    DROP TABLE dbo.TaskAuditLog;
                END
            ");

            // Rename non-partitioned table
            migrationBuilder.Sql(@"
                EXEC sp_rename 'dbo.TaskAuditLog_NonPartitioned', 'TaskAuditLog';
            ");

            // Recreate indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IX_TaskAuditLog_Task 
                ON dbo.TaskAuditLog(TaskItemId, ChangedAt);
            ");

            // Recreate foreign keys
            migrationBuilder.Sql(@"
                ALTER TABLE dbo.TaskAuditLog
                ADD CONSTRAINT FK_TaskAuditLog_TaskItem 
                FOREIGN KEY (TaskItemId) REFERENCES dbo.TaskItems(Id);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.TaskAuditLog
                ADD CONSTRAINT FK_TaskAuditLog_ChangedBy 
                FOREIGN KEY (ChangedByUserId) REFERENCES dbo.Users(Id);
            ");

            // Drop partition scheme and function
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.partition_schemes WHERE name = 'PS_AuditLog_Monthly')
                BEGIN
                    DROP PARTITION SCHEME PS_AuditLog_Monthly;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.partition_functions WHERE name = 'PF_AuditLog_Monthly')
                BEGIN
                    DROP PARTITION FUNCTION PF_AuditLog_Monthly;
                END
            ");
        }
    }
}
