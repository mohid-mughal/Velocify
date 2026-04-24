using MediatR;

namespace Velocify.Application.Commands.Tasks;

public class DeleteCommentCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    
    // Set by handler from authenticated user context
    public Guid UserId { get; set; }
}
