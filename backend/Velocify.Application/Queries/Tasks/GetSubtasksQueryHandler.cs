using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Tasks;

public class GetSubtasksQueryHandler : IRequestHandler<GetSubtasksQuery, List<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetSubtasksQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskDto>> Handle(GetSubtasksQuery request, CancellationToken cancellationToken)
    {
        return await _taskRepository.GetSubtasks(request.ParentTaskId);
    }
}
