using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Tasks;

public class GetTaskCommentsQueryHandler : IRequestHandler<GetTaskCommentsQuery, List<CommentDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskCommentsQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<CommentDto>> Handle(GetTaskCommentsQuery request, CancellationToken cancellationToken)
    {
        return await _taskRepository.GetComments(request.TaskItemId);
    }
}
