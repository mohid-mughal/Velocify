using MediatR;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Users;

/// <summary>
/// Handler for GetActiveUsersQuery.
/// Returns all active users for task assignment dropdowns.
/// No role restriction - all authenticated users can see the list.
/// </summary>
public class GetActiveUsersQueryHandler : IRequestHandler<GetActiveUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetActiveUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
    {
        return await _userRepository.GetActiveUsers();
    }
}
