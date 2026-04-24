using MediatR;

namespace Velocify.Application.Commands.Notifications;

public class MarkNotificationAsReadCommand : IRequest<Unit>
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
}
