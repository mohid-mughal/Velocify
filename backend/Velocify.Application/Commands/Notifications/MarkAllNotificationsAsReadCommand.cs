using MediatR;

namespace Velocify.Application.Commands.Notifications;

public class MarkAllNotificationsAsReadCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}
