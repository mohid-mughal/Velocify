using MediatR;
using Velocify.Application.DTOs.Users;

namespace Velocify.Application.Commands.Users;

public class UpdateCurrentUserCommand : IRequest<UserDto>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
