using MediatR;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Notifications;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Unit>
{
    private readonly INotificationService _notificationService;

    public MarkAllNotificationsAsReadCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsRead(request.UserId);
        return Unit.Value;
    }
}
