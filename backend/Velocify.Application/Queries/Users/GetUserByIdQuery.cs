using MediatR;
using Velocify.Application.DTOs.Users;
using Velocify.Domain.Enums;

namespace Velocify.Application.Queries.Users;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid Id { get; set; }
    public Guid CurrentUserId { get; set; }
    public UserRole CurrentUserRole { get; set; }
}
