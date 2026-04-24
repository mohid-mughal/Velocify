using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Enums;

namespace Velocify.Application.Commands.Tasks;

public class CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public TaskCategory Category { get; set; }
    public Guid AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public string Tags { get; set; } = string.Empty;
    
    // Set by handler from authenticated user context
    public Guid CreatedByUserId { get; set; }
}
