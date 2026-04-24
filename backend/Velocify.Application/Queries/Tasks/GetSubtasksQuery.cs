using MediatR;
using Velocify.Application.DTOs.Tasks;

namespace Velocify.Application.Queries.Tasks;

public class GetSubtasksQuery : IRequest<List<TaskDto>>
{
    public Guid ParentTaskId { get; set; }
}
