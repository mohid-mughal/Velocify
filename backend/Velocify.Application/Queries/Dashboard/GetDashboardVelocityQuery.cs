using MediatR;
using Velocify.Application.DTOs.Dashboard;

namespace Velocify.Application.Queries.Dashboard;

public class GetDashboardVelocityQuery : IRequest<List<VelocityDataPoint>>
{
    public Guid UserId { get; set; }
    public int Days { get; set; } = 30;
}
