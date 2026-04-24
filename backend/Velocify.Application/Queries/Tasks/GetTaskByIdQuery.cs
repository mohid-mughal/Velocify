using MediatR;
using Velocify.Application.DTOs.Tasks;

namespace Velocify.Application.Queries.Tasks;

public class GetTaskByIdQuery : IRequest<TaskDetailDto?>
{
    public Guid Id { get; set; }
}
