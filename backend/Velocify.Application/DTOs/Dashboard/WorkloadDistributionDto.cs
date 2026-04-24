using Velocify.Application.DTOs.Users;

namespace Velocify.Application.DTOs.Dashboard;

public class WorkloadDistributionDto
{
    public UserSummaryDto User { get; set; } = null!;
    public int TotalTaskCount { get; set; }
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int BlockedCount { get; set; }
}
