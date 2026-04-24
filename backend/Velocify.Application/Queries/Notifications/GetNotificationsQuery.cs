using MediatR;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Notifications;

namespace Velocify.Application.Queries.Notifications;

public class GetNotificationsQuery : IRequest<PagedResult<NotificationDto>>
{
    public Guid UserId { get; set; }
    public bool? IsRead { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
