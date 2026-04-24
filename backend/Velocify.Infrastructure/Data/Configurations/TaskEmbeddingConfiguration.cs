using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Velocify.Domain.Entities;

namespace Velocify.Infrastructure.Data.Configurations;

public class TaskEmbeddingConfiguration : IEntityTypeConfiguration<TaskEmbedding>
{
    public void Configure(EntityTypeBuilder<TaskEmbedding> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.TaskItemId)
            .IsRequired();

        builder.Property(e => e.EmbeddingVector)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Unique Constraint
        builder.HasIndex(e => e.TaskItemId)
            .IsUnique()
            .HasDatabaseName("IX_TaskEmbedding_TaskItem");

        // Relationships
        builder.HasOne(e => e.TaskItem)
            .WithOne(t => t.Embedding)
            .HasForeignKey<TaskEmbedding>(e => e.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
