using MediatR;

namespace Velocify.Application.Commands.Auth;

public class LogoutCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
