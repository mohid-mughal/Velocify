using MediatR;
using Velocify.Application.DTOs.Tasks;

namespace Velocify.Application.Commands.Tasks;

public class CreateCommentCommand : IRequest<CommentDto>
{
    public Guid TaskItemId { get; set; }
    public string Content { get; set; } = string.Empty;
    
    // Set by handler from authenticated user context
    public Guid UserId { get; set; }
}
