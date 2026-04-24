using MediatR;
using Velocify.Application.DTOs.Users;
using Velocify.Domain.Enums;

namespace Velocify.Application.Commands.Users;

public class UpdateUserRoleCommand : IRequest<UserDto>
{
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }
}
