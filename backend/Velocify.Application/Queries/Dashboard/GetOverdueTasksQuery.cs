using MediatR;
using Velocify.Application.DTOs.Tasks;

namespace Velocify.Application.Queries.Dashboard;

public class GetOverdueTasksQuery : IRequest<List<TaskDto>>
{
    public Guid UserId { get; set; }
}
