using MediatR;
using Velocify.Application.DTOs.Users;

namespace Velocify.Application.Commands.Auth;

public class RefreshTokenCommand : IRequest<AuthResponseDto>
{
    public string RefreshToken { get; set; } = string.Empty;
}
