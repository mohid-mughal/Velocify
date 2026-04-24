using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Velocify.Application.Commands.Users;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Queries.Users;

namespace Velocify.API.Controllers;

/// <summary>
/// Handles all user management operations including profile management, user listing, role assignment, and productivity metrics.
/// Implements role-based access control with different permissions for Member, Admin, and SuperAdmin roles.
/// </summary>
[Authorize]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the current authenticated user's profile information.
    /// </summary>
    /// <returns>Current user's profile details including productivity score</returns>
    /// <response code="200">User profile successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userId = GetCurrentUserId();
        var query = new GetCurrentUserQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = "The current user profile could not be found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates the current authenticated user's profile information.
    /// </summary>
    /// <param name="command">Updated profile information including first name, last name, and email</param>
    /// <returns>Updated user profile</returns>
    /// <response code="200">Profile successfully updated</response>
    /// <response code="400">Invalid profile data or validation errors</response>
    /// <response code="401">User not authenticated</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateCurrentUserCommand command)
    {
        command.UserId = GetCurrentUserId();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Gets a paginated list of all users (Admin and SuperAdmin only).
    /// Admins see only their team members, SuperAdmins see all users.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of users</returns>
    /// <response code="200">User list successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have Admin or SuperAdmin role</response>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetUsersQuery { Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific user by ID (Admin and SuperAdmin only).
    /// Admins can only view their team members, SuperAdmins can view any user.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User profile details</returns>
    /// <response code="200">User successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have Admin or SuperAdmin role</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {id} could not be found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates a user's role (SuperAdmin only).
    /// This immediately changes the user's permissions across the platform.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">New role assignment</param>
    /// <returns>Updated user profile</returns>
    /// <response code="200">Role successfully updated</response>
    /// <response code="400">Invalid role value</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have SuperAdmin role</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}/role")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleCommand command)
    {
        command.UserId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a user (SuperAdmin only).
    /// The user record is marked as inactive but not physically removed from the database.
    /// </summary>
    /// <param name="id">User ID to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">User successfully deleted</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have SuperAdmin role</response>
    /// <response code="404">User not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand { UserId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Gets productivity metrics for a specific user.
    /// Users can view their own productivity, Admins can view team members, SuperAdmins can view any user.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Productivity score and historical trend data</returns>
    /// <response code="200">Productivity metrics successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to view this user's productivity</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}/productivity")]
    [ProducesResponseType(typeof(ProductivityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductivityDto>> GetUserProductivity(Guid id)
    {
        var query = new GetUserProductivityQuery { UserId = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Productivity data not found",
                Detail = $"Productivity data for user with ID {id} could not be found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
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
