using MediatR;
using Velocify.Application.DTOs.Users;

namespace Velocify.Application.Queries.Users;

/// <summary>
/// Query to get all active users for task assignment purposes.
/// Available to all authenticated users (no role restriction).
/// </summary>
public class GetActiveUsersQuery : IRequest<List<UserDto>>
{
}
