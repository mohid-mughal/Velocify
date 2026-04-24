using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Velocify.Domain.Entities;

namespace Velocify.Infrastructure.Data.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        // Primary Key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.RefreshToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.IsRevoked)
            .HasDefaultValue(false);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.IpAddress)
            .HasMaxLength(50)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_UserSessions_User");

        builder.HasIndex(s => s.RefreshToken)
            .HasDatabaseName("IX_UserSessions_Token");

        // Relationships
        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
