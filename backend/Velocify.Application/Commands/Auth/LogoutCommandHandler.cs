using MediatR;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Auth;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _authService.Logout(request.UserId, request.RefreshToken);
        return Unit.Value;
    }
}
