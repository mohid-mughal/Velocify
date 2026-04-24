using MediatR;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;
using Velocify.Domain.Enums;

namespace Velocify.Application.Queries.Users;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private const int MaxPageSize = 100;
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Only Admin and SuperAdmin can list users
        if (request.CurrentUserRole == UserRole.Member)
        {
            return new PagedResult<UserDto>
            {
                Items = new List<UserDto>(),
                TotalCount = 0,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        var pageSize = Math.Min(request.PageSize, MaxPageSize);

        var result = await _userRepository.GetList(
            page: request.Page,
            pageSize: pageSize);

        // Admin can only see team members (excluding SuperAdmins)
        // TODO: Implement proper team hierarchy when team management is added
        if (request.CurrentUserRole == UserRole.Admin)
        {
            result.Items = result.Items.Where(u => u.Role != UserRole.SuperAdmin).ToList();
            result.TotalCount = result.Items.Count;
        }

        return result;
    }
}
