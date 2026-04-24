using MediatR;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Enums;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Application.Queries.Tasks;

public class GetTaskListQuery : IRequest<PagedResult<TaskDto>>
{
    public TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public TaskCategory? Category { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
