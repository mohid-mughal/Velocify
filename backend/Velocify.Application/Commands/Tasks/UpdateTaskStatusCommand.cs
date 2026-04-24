using MediatR;
using Velocify.Application.DTOs.Tasks;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Application.Commands.Tasks;

public class UpdateTaskStatusCommand : IRequest<TaskDto>
{
    public Guid Id { get; set; }
    public TaskStatus Status { get; set; }
    
    // Set by handler from authenticated user context
    public Guid UpdatedByUserId { get; set; }
}
