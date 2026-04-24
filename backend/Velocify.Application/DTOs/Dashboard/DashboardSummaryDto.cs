namespace Velocify.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int BlockedCount { get; set; }
    public int OverdueCount { get; set; }
    public int DueTodayCount { get; set; }
}
