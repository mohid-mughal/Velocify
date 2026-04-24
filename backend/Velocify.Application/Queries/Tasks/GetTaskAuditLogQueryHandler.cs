using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Tasks;

public class GetTaskAuditLogQueryHandler : IRequestHandler<GetTaskAuditLogQuery, List<TaskAuditLogDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskAuditLogQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskAuditLogDto>> Handle(GetTaskAuditLogQuery request, CancellationToken cancellationToken)
    {
        return await _taskRepository.GetAuditLog(request.TaskItemId);
    }
}
