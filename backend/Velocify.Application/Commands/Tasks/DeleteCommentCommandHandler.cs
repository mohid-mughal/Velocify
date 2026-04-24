using MediatR;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Tasks;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Unit>
{
    private readonly ITaskRepository _taskRepository;

    public DeleteCommentCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        // Verify comment exists
        var comment = await _taskRepository.GetCommentById(request.Id);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {request.Id} not found");
        }

        // Delete the comment (authorization check will be performed in repository)
        // The repository will check if the user can delete the comment based on:
        // - User owns the comment, OR
        // - User is Admin or SuperAdmin
        await _taskRepository.DeleteComment(request.Id, request.UserId);
        
        return Unit.Value;
    }
}
