using MediatR;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Notifications;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Queries.Notifications;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private const int MaxPageSize = 100;
    private readonly INotificationService _notificationService;

    public GetNotificationsQueryHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, MaxPageSize);

        return await _notificationService.GetUserNotifications(
            userId: request.UserId,
            page: request.Page,
            pageSize: pageSize,
            isRead: request.IsRead);
    }
}
