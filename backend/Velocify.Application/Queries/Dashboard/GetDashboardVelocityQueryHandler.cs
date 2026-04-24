using MediatR;
using Velocify.Application.DTOs.Dashboard;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Dashboard;

public class GetDashboardVelocityQueryHandler : IRequestHandler<GetDashboardVelocityQuery, List<VelocityDataPoint>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardVelocityQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<List<VelocityDataPoint>> Handle(GetDashboardVelocityQuery request, CancellationToken cancellationToken)
    {
        return await _dashboardRepository.GetDashboardVelocity(request.UserId, request.Days);
    }
}
