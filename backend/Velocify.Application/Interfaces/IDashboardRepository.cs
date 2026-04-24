using Velocify.Application.DTOs.Dashboard;
using Velocify.Application.DTOs.Tasks;

namespace Velocify.Application.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetDashboardSummary(Guid userId);
    Task<List<VelocityDataPoint>> GetDashboardVelocity(Guid userId, int days);
    Task<List<WorkloadDistributionDto>> GetWorkloadDistribution();
    Task<List<TaskDto>> GetOverdueTasks(Guid userId);
}
