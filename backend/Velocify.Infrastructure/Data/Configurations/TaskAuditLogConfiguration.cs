using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Velocify.Domain.Entities;

namespace Velocify.Infrastructure.Data.Configurations;

public class TaskAuditLogConfiguration : IEntityTypeConfiguration<TaskAuditLog>
{
    public void Configure(EntityTypeBuilder<TaskAuditLog> builder)
    {
        // Primary Key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.TaskItemId)
            .IsRequired();

        builder.Property(a => a.ChangedByUserId)
            .IsRequired();

        builder.Property(a => a.FieldName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.OldValue)
            .IsRequired(false);

        builder.Property(a => a.NewValue)
            .IsRequired(false);

        builder.Property(a => a.ChangedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(a => new { a.TaskItemId, a.ChangedAt })
            .HasDatabaseName("IX_TaskAuditLog_Task");

        // Relationships
        builder.HasOne(a => a.TaskItem)
            .WithMany(t => t.AuditLogs)
            .HasForeignKey(a => a.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.ChangedBy)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
