using MediatR;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;
using Velocify.Domain.Enums;

namespace Velocify.Application.Queries.Users;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // SuperAdmin can access all users
        if (request.CurrentUserRole == UserRole.SuperAdmin)
        {
            return await _userRepository.GetById(request.Id);
        }

        // Admin can access team members (for now, simplified to all users except SuperAdmins)
        // TODO: Implement proper team hierarchy when team management is added
        if (request.CurrentUserRole == UserRole.Admin)
        {
            var user = await _userRepository.GetById(request.Id);
            if (user != null && user.Role != UserRole.SuperAdmin)
            {
                return user;
            }
            return null;
        }

        // Member can only access their own data
        if (request.Id != request.CurrentUserId)
        {
            return null;
        }

        return await _userRepository.GetById(request.Id);
    }
}
