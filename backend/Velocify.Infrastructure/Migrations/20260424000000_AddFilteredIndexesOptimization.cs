using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Velocify.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Optimization migration that adds filtered indexes for soft-deleted records.
    /// 
    /// Performance Benefits:
    /// - Filtered indexes exclude IsDeleted = 1 rows, reducing index size by ~20-40% in typical scenarios
    /// - Smaller indexes mean faster query execution and reduced memory footprint
    /// - SQL Server can use these indexes more efficiently for queries with WHERE IsDeleted = 0 predicates
    /// - Index maintenance (INSERT/UPDATE/DELETE) is faster since deleted rows aren't indexed
    /// 
    /// Requirement: 15.1 - Database SHALL use filtered indexes that exclude IsDeleted = 1 rows
    /// </summary>
    public partial class AddFilteredIndexesOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: These filtered indexes already exist in the InitialCreate migration.
            // This migration serves as documentation of the optimization strategy.
            // In a real-world scenario, this would be a separate migration applied after initial deployment.

            // Filtered index on TaskItems.AssignedToUserId
            // Why: Most queries filter by AssignedToUserId AND IsDeleted = 0
            // Impact: Reduces index size and improves query performance for active task lookups
            // Example query: SELECT * FROM TaskItems WHERE AssignedToUserId = @userId AND IsDeleted = 0
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItem_AssignedTo_Active' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    CREATE INDEX IX_TaskItem_AssignedTo_Active 
                    ON TaskItems(AssignedToUserId) 
                    WHERE IsDeleted = 0;
                END
            ");

            // Filtered index on TaskComments.TaskItemId
            // Why: Comment queries always filter by TaskItemId AND IsDeleted = 0
            // Impact: Improves performance of comment thread loading, which happens on every task detail view
            // Example query: SELECT * FROM TaskComments WHERE TaskItemId = @taskId AND IsDeleted = 0 ORDER BY CreatedAt
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskComments_Task_Active' AND object_id = OBJECT_ID('TaskComments'))
                BEGIN
                    CREATE INDEX IX_TaskComments_Task_Active 
                    ON TaskComments(TaskItemId) 
                    WHERE IsDeleted = 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the filtered indexes
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItem_AssignedTo_Active' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    DROP INDEX IX_TaskItem_AssignedTo_Active ON TaskItems;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskComments_Task_Active' AND object_id = OBJECT_ID('TaskComments'))
                BEGIN
                    DROP INDEX IX_TaskComments_Task_Active ON TaskComments;
                END
            ");
        }
    }
}
