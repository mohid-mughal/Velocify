using MediatR;

namespace Velocify.Application.Commands.Tasks;

public class DeleteTaskCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    
    // Set by handler from authenticated user context
    public Guid DeletedByUserId { get; set; }
}
