using MediatR;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public RegisterUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return await _authService.Register(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);
    }
}
