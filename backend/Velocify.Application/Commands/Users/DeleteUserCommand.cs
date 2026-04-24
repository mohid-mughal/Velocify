using MediatR;

namespace Velocify.Application.Commands.Users;

public class DeleteUserCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}
