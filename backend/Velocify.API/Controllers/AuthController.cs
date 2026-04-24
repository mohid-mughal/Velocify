using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Velocify.Application.Commands.Auth;
using Velocify.Application.DTOs.Users;

namespace Velocify.API.Controllers;

/// <summary>
/// Handles all authentication-related operations including registration, login, token refresh, and logout.
/// Implements JWT-based authentication with refresh token rotation for security.
/// </summary>
public class AuthController : ApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a new user account with email and password.
    /// </summary>
    /// <param name="command">Registration details including first name, last name, email, and password</param>
    /// <returns>Authentication response with access token, refresh token, and user details</returns>
    /// <response code="200">User successfully registered and authenticated</response>
    /// <response code="400">Invalid registration data or validation errors</response>
    /// <response code="409">Email already exists</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="command">Login credentials including email and password</param>
    /// <returns>Authentication response with access token (15min TTL), refresh token (7day TTL), and user details</returns>
    /// <response code="200">User successfully authenticated</response>
    /// <response code="400">Invalid request format</response>
    /// <response code="401">Invalid credentials or inactive account</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Implements token rotation: the old refresh token is invalidated and a new one is issued.
    /// </summary>
    /// <param name="command">Refresh token from previous authentication</param>
    /// <returns>New authentication response with fresh access token and rotated refresh token</returns>
    /// <response code="200">Token successfully refreshed</response>
    /// <response code="400">Invalid request format</response>
    /// <response code="401">Invalid, expired, or revoked refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Logs out the current user by revoking their refresh token.
    /// The access token will remain valid until expiration (15 minutes) but cannot be refreshed.
    /// </summary>
    /// <param name="command">Logout request containing the refresh token to revoke</param>
    /// <returns>No content on successful logout</returns>
    /// <response code="204">User successfully logged out</response>
    /// <response code="400">Invalid request format</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        // Get UserId from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid user identity",
                Detail = "Unable to identify the current user from the authentication token.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        command.UserId = userId;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Revokes all active sessions for a specific user (SuperAdmin only).
    /// This forces the user to re-authenticate on all devices.
    /// </summary>
    /// <param name="command">Command containing the target user ID</param>
    /// <returns>No content on successful revocation</returns>
    /// <response code="204">All sessions successfully revoked</response>
    /// <response code="400">Invalid request format</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have SuperAdmin role</response>
    /// <response code="404">Target user not found</response>
    [HttpPost("revoke-all-sessions")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAllSessions([FromBody] RevokeAllSessionsCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
