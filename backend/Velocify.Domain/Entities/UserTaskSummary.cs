using Velocify.Domain.Enums;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Domain.Entities;

/// <summary>
/// Represents a materialized view that pre-aggregates task counts per user per status.
/// This entity maps to the indexed view vw_UserTaskSummary in the database.
/// 
/// The view is created with SCHEMABINDING and a unique clustered index, making it
/// a materialized view that SQL Server maintains automatically. This eliminates
/// the need to run COUNT(*) GROUP BY queries on every dashboard request.
/// </summary>
public class UserTaskSummary
{
    public Guid UserId { get; set; }
    public TaskStatus Status { get; set; }
    public long TaskCount { get; set; }
}
