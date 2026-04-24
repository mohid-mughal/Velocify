using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Velocify.Domain.Entities;

namespace Velocify.Infrastructure.Data.Configurations;

public class UserTaskSummaryConfiguration : IEntityTypeConfiguration<UserTaskSummary>
{
    public void Configure(EntityTypeBuilder<UserTaskSummary> builder)
    {
        // This is a keyless entity (view)
        builder.HasNoKey();

        // Map to database view
        builder.ToView("vw_UserTaskSummary");

        // Properties
        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.Status)
            .IsRequired();

        builder.Property(u => u.TaskCount)
            .IsRequired();
    }
}
