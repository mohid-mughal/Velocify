using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Notifications;
using Velocify.Domain.Enums;

namespace Velocify.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> CreateNotification(Guid userId, NotificationType type, string message, Guid? taskItemId = null);
    Task<PagedResult<NotificationDto>> GetUserNotifications(Guid userId, int page, int pageSize, bool? isRead = null);
    Task MarkAsRead(Guid notificationId, Guid userId);
    Task MarkAllAsRead(Guid userId);
}
