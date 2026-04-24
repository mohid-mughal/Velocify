using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Velocify.Application.Commands.Notifications;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Notifications;
using Velocify.Application.Queries.Notifications;

namespace Velocify.API.Controllers;

/// <summary>
/// Handles notification operations including retrieval and marking as read.
/// Users can only access their own notifications.
/// </summary>
[Authorize]
public class NotificationsController : ApiController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a paginated list of notifications for the current user.
    /// Can be filtered by read/unread status.
    /// </summary>
    /// <param name="isRead">Optional filter: true for read notifications, false for unread, null for all</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of notifications</returns>
    /// <response code="200">Notifications successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
        [FromQuery] bool? isRead = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var query = new GetNotificationsQuery
        {
            UserId = userId,
            IsRead = isRead,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// Users can only mark their own notifications as read.
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Notification successfully marked as read</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to access this notification</response>
    /// <response code="404">Notification not found</response>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = id,
            UserId = userId
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Marks all notifications for the current user as read.
    /// </summary>
    /// <returns>No content on success</returns>
    /// <response code="204">All notifications successfully marked as read</response>
    /// <response code="401">User not authenticated</response>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        var command = new MarkAllNotificationsAsReadCommand
        {
            UserId = userId
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Helper method to extract the current user's ID from JWT claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Unable to identify the current user from the authentication token.");
        }
        return userId;
    }
}
