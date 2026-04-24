using MediatR;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Users;
using Velocify.Domain.Enums;

namespace Velocify.Application.Queries.Users;

public class GetUsersQuery : IRequest<PagedResult<UserDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid CurrentUserId { get; set; }
    public UserRole CurrentUserRole { get; set; }
}
