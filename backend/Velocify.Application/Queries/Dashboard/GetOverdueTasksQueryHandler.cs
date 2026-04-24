using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Dashboard;

public class GetOverdueTasksQueryHandler : IRequestHandler<GetOverdueTasksQuery, List<TaskDto>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetOverdueTasksQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<List<TaskDto>> Handle(GetOverdueTasksQuery request, CancellationToken cancellationToken)
    {
        return await _dashboardRepository.GetOverdueTasks(request.UserId);
    }
}
