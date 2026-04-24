using MediatR;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Notifications;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Unit>
{
    private readonly INotificationService _notificationService;

    public MarkNotificationAsReadCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsRead(request.NotificationId, request.UserId);
        return Unit.Value;
    }
}
