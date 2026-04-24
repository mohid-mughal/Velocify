using MediatR;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Auth;

public class RevokeAllSessionsCommandHandler : IRequestHandler<RevokeAllSessionsCommand, Unit>
{
    private readonly IAuthService _authService;

    public RevokeAllSessionsCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Unit> Handle(RevokeAllSessionsCommand request, CancellationToken cancellationToken)
    {
        await _authService.RevokeAllSessions(request.TargetUserId);
        return Unit.Value;
    }
}
