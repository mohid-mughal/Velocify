using MediatR;
using Velocify.Application.DTOs.Users;

namespace Velocify.Application.Commands.Auth;

public class LoginCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
