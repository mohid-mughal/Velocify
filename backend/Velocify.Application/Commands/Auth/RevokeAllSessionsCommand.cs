using MediatR;

namespace Velocify.Application.Commands.Auth;

public class RevokeAllSessionsCommand : IRequest<Unit>
{
    public Guid TargetUserId { get; set; }
}
