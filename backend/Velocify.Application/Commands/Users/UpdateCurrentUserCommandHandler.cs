using MediatR;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Users;

public class UpdateCurrentUserCommandHandler : IRequestHandler<UpdateCurrentUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public UpdateCurrentUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
        }

        // Check if email is being changed and if the new email is already taken
        if (user.Email != request.Email)
        {
            var existingUser = await _userRepository.GetByEmail(request.Email);
            if (existingUser != null && existingUser.Id != request.UserId)
            {
                throw new InvalidOperationException($"Email '{request.Email}' is already in use by another user.");
            }
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;

        return await _userRepository.Update(user);
    }
}
