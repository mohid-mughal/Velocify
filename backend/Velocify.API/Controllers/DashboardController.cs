using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Velocify.Application.DTOs.Dashboard;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Queries.Dashboard;

namespace Velocify.API.Controllers;

/// <summary>
/// Provides dashboard analytics and metrics for task management.
/// Implements role-based access control with workload distribution restricted to Admin and SuperAdmin roles.
/// </summary>
[Authorize]
public class DashboardController : ApiController
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets dashboard summary with task counts grouped by status.
    /// Uses the indexed view vw_UserTaskSummary for optimized performance.
    /// </summary>
    /// <returns>Task counts by status</returns>
    /// <response code="200">Dashboard summary successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var userId = GetCurrentUserId();
        var query = new GetDashboardSummaryQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets velocity data showing task completion counts per day for the last 30 days.
    /// </summary>
    /// <param name="days">Number of days to retrieve (default: 30)</param>
    /// <returns>List of velocity data points</returns>
    /// <response code="200">Velocity data successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("velocity")]
    [ProducesResponseType(typeof(List<VelocityDataPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<VelocityDataPoint>>> GetVelocity([FromQuery] int days = 30)
    {
        var userId = GetCurrentUserId();
        var query = new GetDashboardVelocityQuery { UserId = userId, Days = days };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets workload distribution showing task counts per team member.
    /// Admin only - Members cannot access this endpoint.
    /// </summary>
    /// <returns>List of workload distribution data</returns>
    /// <response code="200">Workload distribution successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have Admin or SuperAdmin role</response>
    [HttpGet("workload")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(List<WorkloadDistributionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<WorkloadDistributionDto>>> GetWorkload()
    {
        var query = new GetWorkloadDistributionQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets overdue tasks where DueDate is before current time and Status is not Completed.
    /// </summary>
    /// <returns>List of overdue tasks</returns>
    /// <response code="200">Overdue tasks successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TaskDto>>> GetOverdue()
    {
        var userId = GetCurrentUserId();
        var query = new GetOverdueTasksQuery { UserId = userId };
        var result = await _mediator.Send(query);
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
