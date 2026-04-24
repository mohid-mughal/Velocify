using MediatR;
using Velocify.Application.DTOs.Dashboard;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Dashboard;

public class GetWorkloadDistributionQueryHandler : IRequestHandler<GetWorkloadDistributionQuery, List<WorkloadDistributionDto>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetWorkloadDistributionQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<List<WorkloadDistributionDto>> Handle(GetWorkloadDistributionQuery request, CancellationToken cancellationToken)
    {
        return await _dashboardRepository.GetWorkloadDistribution();
    }
}
