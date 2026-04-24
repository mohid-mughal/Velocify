using MediatR;
using Velocify.Application.DTOs.Dashboard;

namespace Velocify.Application.Queries.Dashboard;

public class GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>
{
    public Guid UserId { get; set; }
}
