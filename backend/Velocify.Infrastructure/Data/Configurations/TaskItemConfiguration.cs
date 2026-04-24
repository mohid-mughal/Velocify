using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Velocify.Domain.Entities;

namespace Velocify.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        // Primary Key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired(false);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.Priority)
            .IsRequired();

        builder.Property(t => t.Category)
            .IsRequired();

        builder.Property(t => t.AssignedToUserId)
            .IsRequired();

        builder.Property(t => t.CreatedByUserId)
            .IsRequired();

        builder.Property(t => t.ParentTaskId)
            .IsRequired(false);

        builder.Property(t => t.DueDate)
            .IsRequired(false);

        builder.Property(t => t.CompletedAt)
            .IsRequired(false);

        builder.Property(t => t.EstimatedHours)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(t => t.ActualHours)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(t => t.Tags)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(t => t.AiPriorityScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(t => t.PredictedCompletionProbability)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(t => t.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Concurrency Token
        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        // Filtered Indexes
        // Only indexes active records (IsDeleted = 0) to improve query performance
        // and reduce index size. Soft-deleted tasks are rarely queried.
        builder.HasIndex(t => t.AssignedToUserId)
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_TaskItem_AssignedTo_Active");

        // Composite Indexes
        // Dashboard queries frequently filter by assignee, status, and deleted flag
        builder.HasIndex(t => new { t.AssignedToUserId, t.Status, t.IsDeleted })
            .HasDatabaseName("IX_TaskItem_Dashboard");

        // Overdue task queries filter by due date and priority
        builder.HasIndex(t => new { t.DueDate, t.Priority, t.IsDeleted })
            .HasDatabaseName("IX_TaskItem_Overdue");

        // Task history queries filter by creator and sort by creation date
        builder.HasIndex(t => new { t.CreatedByUserId, t.CreatedAt, t.IsDeleted })
            .HasDatabaseName("IX_TaskItem_CreatedBy");

        // Tag search queries use LIKE operations on the Tags column
        builder.HasIndex(t => t.Tags)
            .HasDatabaseName("IX_TaskItem_Tags");

        // Relationships
        builder.HasOne(t => t.AssignedTo)
            .WithMany(u => u.TasksAssigned)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreatedBy)
            .WithMany(u => u.TasksCreated)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.TaskItem)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.AuditLogs)
            .WithOne(a => a.TaskItem)
            .HasForeignKey(a => a.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing relationship for parent/subtasks
        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Embedding)
            .WithOne(e => e.TaskItem)
            .HasForeignKey<TaskEmbedding>(e => e.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Notifications)
            .WithOne(n => n.TaskItem)
            .HasForeignKey(n => n.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
