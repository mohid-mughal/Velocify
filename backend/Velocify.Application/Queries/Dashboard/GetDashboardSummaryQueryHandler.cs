using MediatR;
using Velocify.Application.DTOs.Dashboard;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Dashboard;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardSummaryQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _dashboardRepository.GetDashboardSummary(request.UserId);
    }
}
