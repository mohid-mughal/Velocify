using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Velocify.Domain.Entities;

namespace Velocify.Infrastructure.Data.Configurations;

public class AiInteractionLogConfiguration : IEntityTypeConfiguration<AiInteractionLog>
{
    public void Configure(EntityTypeBuilder<AiInteractionLog> builder)
    {
        // Primary Key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.FeatureType)
            .IsRequired();

        builder.Property(a => a.InputSummary)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.OutputSummary)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.TokensUsed)
            .IsRequired(false);

        builder.Property(a => a.LatencyMs)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(a => new { a.UserId, a.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_AiInteractionLog_User");

        builder.HasIndex(a => new { a.FeatureType, a.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_AiInteractionLog_Feature");

        // Relationships
        builder.HasOne(a => a.User)
            .WithMany(u => u.AiInteractionLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
