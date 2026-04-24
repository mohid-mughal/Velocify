using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Velocify.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Optimization migration that adds composite indexes for common query patterns.
    /// 
    /// Performance Benefits:
    /// - Composite indexes support queries that filter on multiple columns simultaneously
    /// - Column order matters: most selective columns should be first
    /// - These indexes eliminate the need for index intersection or table scans
    /// - Covering indexes reduce the need to access the base table
    /// 
    /// Requirement: 15.2 - Database SHALL maintain composite indexes on common query patterns
    /// </summary>
    public partial class AddCompositeIndexesOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Composite index: (AssignedToUserId, Status, IsDeleted)
            // Query Pattern: Dashboard and task list views that filter by assignee and status
            // Example queries:
            //   - SELECT * FROM TaskItems WHERE AssignedToUserId = @userId AND Status = @status AND IsDeleted = 0
            //   - SELECT COUNT(*) FROM TaskItems WHERE AssignedToUserId = @userId AND Status IN (0,1,2) AND IsDeleted = 0
            // Why this order:
            //   - AssignedToUserId is most selective (filters to specific user's tasks)
            //   - Status is next (filters to specific workflow state)
            //   - IsDeleted is last (binary filter, least selective)
            // Impact: Serves GET /api/v1/dashboard/summary and GET /api/v1/tasks endpoints
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItem_Dashboard' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    CREATE INDEX IX_TaskItem_Dashboard 
                    ON TaskItems(AssignedToUserId, Status, IsDeleted);
                END
            ");

            // Composite index: (DueDate, Priority, IsDeleted)
            // Query Pattern: Overdue task queries and priority-based sorting
            // Example queries:
            //   - SELECT * FROM TaskItems WHERE DueDate < GETUTCDATE() AND IsDeleted = 0 ORDER BY Priority
            //   - SELECT * FROM TaskItems WHERE DueDate BETWEEN @start AND @end AND Priority = @priority AND IsDeleted = 0
            // Why this order:
            //   - DueDate is most selective (range queries on dates are common)
            //   - Priority is next (used for sorting and filtering)
            //   - IsDeleted is last (binary filter)
            // Impact: Serves GET /api/v1/dashboard/overdue and date-range filtered task queries
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItem_Overdue' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    CREATE INDEX IX_TaskItem_Overdue 
                    ON TaskItems(DueDate, Priority, IsDeleted);
                END
            ");

            // Composite index: (CreatedByUserId, CreatedAt DESC, IsDeleted)
            // Query Pattern: User activity history and "my created tasks" views
            // Example queries:
            //   - SELECT * FROM TaskItems WHERE CreatedByUserId = @userId AND IsDeleted = 0 ORDER BY CreatedAt DESC
            //   - SELECT TOP 10 * FROM TaskItems WHERE CreatedByUserId = @userId AND IsDeleted = 0 ORDER BY CreatedAt DESC
            // Why this order:
            //   - CreatedByUserId is most selective (filters to specific user)
            //   - CreatedAt DESC supports descending sort (most recent first) without additional sorting
            //   - IsDeleted is last (binary filter)
            // Impact: Serves user profile pages showing recently created tasks
            // Note: DESC in index definition means the index stores values in descending order,
            //       eliminating the need for SQL Server to sort results when ORDER BY CreatedAt DESC is used
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItem_CreatedBy' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    CREATE INDEX IX_TaskItem_CreatedBy 
                    ON TaskItems(CreatedByUserId, CreatedAt DESC, IsDeleted);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop composite indexes in reverse order
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItem_CreatedBy' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    DROP INDEX IX_TaskItem_CreatedBy ON TaskItems;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItem_Overdue' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    DROP INDEX IX_TaskItem_Overdue ON TaskItems;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItem_Dashboard' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    DROP INDEX IX_TaskItem_Dashboard ON TaskItems;
                END
            ");
        }
    }
}
