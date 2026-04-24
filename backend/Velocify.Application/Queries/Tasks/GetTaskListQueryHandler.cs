using MediatR;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Tasks;

public class GetTaskListQueryHandler : IRequestHandler<GetTaskListQuery, PagedResult<TaskDto>>
{
    private const int MaxPageSize = 100;
    private readonly ITaskRepository _taskRepository;

    public GetTaskListQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<PagedResult<TaskDto>> Handle(GetTaskListQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, MaxPageSize);

        return await _taskRepository.GetList(
            status: request.Status,
            priority: request.Priority,
            category: request.Category,
            assignedToUserId: request.AssignedToUserId,
            dueDateFrom: request.DueDateFrom,
            dueDateTo: request.DueDateTo,
            searchTerm: request.SearchTerm,
            page: request.Page,
            pageSize: pageSize);
    }
}
