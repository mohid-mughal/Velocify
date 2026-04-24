using MediatR;
using Velocify.Application.DTOs.Users;

namespace Velocify.Application.Queries.Users;

public class GetCurrentUserQuery : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
}
