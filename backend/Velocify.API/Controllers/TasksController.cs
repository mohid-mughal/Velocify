using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Velocify.Application.Commands.Tasks;
using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Queries.Tasks;

namespace Velocify.API.Controllers;

/// <summary>
/// Handles all task management operations including CRUD, filtering, comments, subtasks, and import/export.
/// Implements role-based access control with different permissions for Member, Admin, and SuperAdmin roles.
/// </summary>
[Authorize]
public class TasksController : ApiController
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a paginated and filterable list of tasks.
    /// Members see only their own tasks, Admins see team tasks, SuperAdmins see all tasks.
    /// </summary>
    /// <param name="query">Filter parameters including status, priority, category, assignee, date range, and search term</param>
    /// <returns>Paginated list of tasks</returns>
    /// <response code="200">Task list successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks([FromQuery] GetTaskListQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific task by ID with full details including comments, audit log, and subtasks.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    /// <response code="200">Task successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to view this task</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDetailDto>> GetTaskById(Guid id)
    {
        var query = new GetTaskByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"Task with ID {id} could not be found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="command">Task creation details including title, description, priority, category, assignee, and due date</param>
    /// <returns>Created task</returns>
    /// <response code="201">Task successfully created</response>
    /// <response code="400">Invalid task data or validation errors</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskCommand command)
    {
        command.CreatedByUserId = GetCurrentUserId();
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing task.
    /// Implements optimistic concurrency control using row versioning.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="command">Updated task details</param>
    /// <returns>Updated task</returns>
    /// <response code="200">Task successfully updated</response>
    /// <response code="400">Invalid task data or validation errors</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to update this task</response>
    /// <response code="404">Task not found</response>
    /// <response code="409">Concurrent update conflict detected</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskCommand command)
    {
        command.Id = id;
        command.UpdatedByUserId = GetCurrentUserId();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Updates only the status of a task.
    /// Sets CompletedAt timestamp when status changes to Completed.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="command">New status</param>
    /// <returns>Updated task</returns>
    /// <response code="200">Task status successfully updated</response>
    /// <response code="400">Invalid status value</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to update this task</response>
    /// <response code="404">Task not found</response>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusCommand command)
    {
        command.Id = id;
        command.UpdatedByUserId = GetCurrentUserId();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a task.
    /// The task record is marked as deleted but not physically removed from the database.
    /// </summary>
    /// <param name="id">Task ID to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Task successfully deleted</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to delete this task</response>
    /// <response code="404">Task not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var command = new DeleteTaskCommand { Id = id, DeletedByUserId = GetCurrentUserId() };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Gets the audit history for a specific task.
    /// Shows all field changes with timestamps and user information.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>List of audit log entries</returns>
    /// <response code="200">Audit history successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to view this task</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(List<TaskAuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TaskAuditLogDto>>> GetTaskHistory(Guid id)
    {
        var query = new GetTaskAuditLogQuery { TaskItemId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets all comments for a specific task.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>List of comments with sentiment scores</returns>
    /// <response code="200">Comments successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to view this task</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}/comments")]
    [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CommentDto>>> GetComments(Guid id)
    {
        var query = new GetTaskCommentsQuery { TaskItemId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Adds a comment to a task.
    /// Triggers asynchronous sentiment analysis.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="command">Comment content</param>
    /// <returns>Created comment with sentiment score</returns>
    /// <response code="201">Comment successfully created</response>
    /// <response code="400">Invalid comment data or validation errors</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to comment on this task</response>
    /// <response code="404">Task not found</response>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> CreateComment(Guid id, [FromBody] CreateCommentCommand command)
    {
        command.TaskItemId = id;
        command.UserId = GetCurrentUserId();
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetComments), new { id }, result);
    }

    /// <summary>
    /// Deletes a comment.
    /// Users can only delete their own comments unless they are Admin or SuperAdmin.
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="commentId">Comment ID to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Comment successfully deleted</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to delete this comment</response>
    /// <response code="404">Comment not found</response>
    [HttpDelete("{id}/comments/{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
    {
        var command = new DeleteCommentCommand { Id = commentId, UserId = GetCurrentUserId() };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Gets all subtasks for a specific task.
    /// </summary>
    /// <param name="id">Parent task ID</param>
    /// <returns>List of subtasks</returns>
    /// <response code="200">Subtasks successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to view this task</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}/subtasks")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TaskDto>>> GetSubtasks(Guid id)
    {
        var query = new GetSubtasksQuery { ParentTaskId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Exports tasks to CSV format.
    /// Members export their own tasks, Admins export team tasks, SuperAdmins export all tasks.
    /// </summary>
    /// <param name="query">Filter parameters for tasks to export</param>
    /// <returns>CSV file with task data</returns>
    /// <response code="200">Tasks successfully exported</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportTasks([FromBody] GetTaskListQuery query)
    {
        // Set a large page size to get all tasks for export
        query.PageSize = 10000;
        var result = await _mediator.Send(query);

        // Generate CSV content
        var csv = GenerateCsv(result.Items);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

        return File(bytes, "text/csv", $"tasks_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Imports tasks from CSV file with AI-powered normalization.
    /// The AI analyzes column headers and normalizes non-standard values.
    /// </summary>
    /// <param name="file">CSV file to import</param>
    /// <returns>Preview of normalized tasks for user confirmation</returns>
    /// <response code="200">Tasks successfully parsed and normalized</response>
    /// <response code="400">Invalid CSV format or validation errors</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult ImportTasks(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid file",
                Detail = "No file was uploaded or the file is empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid file type",
                Detail = "Only CSV files are supported.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // TODO: Implement AI-powered CSV import
        // This would involve:
        // 1. Reading the CSV file
        // 2. Calling IAiImportService to normalize the data
        // 3. Returning the normalized tasks for user confirmation
        // 4. User would then call CreateTask for each confirmed task

        return Ok(new { message = "Import functionality will be implemented in a future task" });
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

    /// <summary>
    /// Helper method to generate CSV content from task list.
    /// </summary>
    private string GenerateCsv(List<TaskDto> tasks)
    {
        var sb = new System.Text.StringBuilder();
        
        // Header
        sb.AppendLine("Id,Title,Description,Status,Priority,Category,AssignedTo,CreatedBy,DueDate,CompletedAt,EstimatedHours,ActualHours,Tags,CreatedAt,UpdatedAt");
        
        // Rows
        foreach (var task in tasks)
        {
            sb.AppendLine($"{task.Id}," +
                         $"\"{EscapeCsv(task.Title)}\"," +
                         $"\"{EscapeCsv(task.Description)}\"," +
                         $"{task.Status}," +
                         $"{task.Priority}," +
                         $"{task.Category}," +
                         $"\"{EscapeCsv($"{task.AssignedTo.FirstName} {task.AssignedTo.LastName}")}\"," +
                         $"\"{EscapeCsv($"{task.CreatedBy.FirstName} {task.CreatedBy.LastName}")}\"," +
                         $"{task.DueDate:yyyy-MM-dd HH:mm:ss}," +
                         $"{task.CompletedAt:yyyy-MM-dd HH:mm:ss}," +
                         $"{task.EstimatedHours}," +
                         $"{task.ActualHours}," +
                         $"\"{EscapeCsv(task.Tags)}\"," +
                         $"{task.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                         $"{task.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Helper method to escape CSV values.
    /// </summary>
    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        
        return value.Replace("\"", "\"\"");
    }
}
