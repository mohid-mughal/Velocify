using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Velocify.Application.DTOs.AI;
using Velocify.Application.DTOs.Import;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;

namespace Velocify.API.Controllers;

/// <summary>
/// Provides AI-powered features for task management including natural language parsing,
/// task decomposition, semantic search, workload balancing, import normalization, and daily digest.
/// Requirements: 8.1-8.7, 9.1-9.6, 10.1-10.7, 11.1-11.6, 12.1-12.7, 13.1-13.7
/// </summary>
[Authorize]
public class AiController : ApiController
{
    private readonly INaturalLanguageTaskService _naturalLanguageTaskService;
    private readonly ITaskDecompositionService _taskDecompositionService;
    private readonly ISemanticSearchService _semanticSearchService;
    private readonly IWorkloadBalancingService _workloadBalancingService;
    private readonly IAiImportService _aiImportService;
    private readonly IDailyDigestService _dailyDigestService;

    public AiController(
        INaturalLanguageTaskService naturalLanguageTaskService,
        ITaskDecompositionService taskDecompositionService,
        ISemanticSearchService semanticSearchService,
        IWorkloadBalancingService workloadBalancingService,
        IAiImportService aiImportService,
        IDailyDigestService dailyDigestService)
    {
        _naturalLanguageTaskService = naturalLanguageTaskService;
        _taskDecompositionService = taskDecompositionService;
        _semanticSearchService = semanticSearchService;
        _workloadBalancingService = workloadBalancingService;
        _aiImportService = aiImportService;
        _dailyDigestService = dailyDigestService;
    }

    /// <summary>
    /// Parses natural language text into structured task data.
    /// Uses LangChain structured output parser with Polly retry policy.
    /// </summary>
    /// <param name="request">Natural language text describing the task</param>
    /// <returns>Parsed task information for user confirmation</returns>
    /// <response code="200">Task successfully parsed</response>
    /// <response code="400">Invalid input or parsing failed</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("parse-task")]
    [ProducesResponseType(typeof(ParsedTaskResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ParsedTaskResult>> ParseTask([FromBody] ParseTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Input))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid input",
                Detail = "Natural language input cannot be empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _naturalLanguageTaskService.ParseTaskFromText(request.Input);
        return Ok(result);
    }

    /// <summary>
    /// Decomposes a complex task into suggested subtasks using AI.
    /// Generates up to 8 subtask suggestions with titles and estimated hours.
    /// </summary>
    /// <param name="taskId">ID of the task to decompose</param>
    /// <returns>List of suggested subtasks</returns>
    /// <response code="200">Task successfully decomposed</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have permission to access this task</response>
    /// <response code="404">Task not found</response>
    [HttpPost("decompose/{taskId}")]
    [ProducesResponseType(typeof(List<SubtaskSuggestion>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<SubtaskSuggestion>>> DecomposeTask(Guid taskId)
    {
        var result = await _taskDecompositionService.DecomposeTask(taskId);
        return Ok(result);
    }

    /// <summary>
    /// Performs semantic search using AI-powered embedding vectors.
    /// Executes both SQL LIKE search and embedding-based search in parallel.
    /// </summary>
    /// <param name="request">Search query in natural language</param>
    /// <returns>List of tasks ranked by relevance</returns>
    /// <response code="200">Search completed successfully</response>
    /// <response code="400">Invalid search query</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("search")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TaskDto>>> SearchTasks([FromBody] SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid query",
                Detail = "Search query cannot be empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _semanticSearchService.SearchTasks(request.Query);
        return Ok(result);
    }

    /// <summary>
    /// Gets AI-powered workload balancing suggestions for task redistribution.
    /// Analyzes current task assignments, productivity scores, and due dates.
    /// Admin and SuperAdmin only.
    /// </summary>
    /// <returns>List of workload balancing suggestions</returns>
    /// <response code="200">Suggestions successfully generated</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User does not have Admin or SuperAdmin role</response>
    [HttpGet("workload-suggestions")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(List<WorkloadSuggestion>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<WorkloadSuggestion>>> GetWorkloadSuggestions()
    {
        var result = await _workloadBalancingService.GetSuggestions();
        return Ok(result);
    }

    /// <summary>
    /// Normalizes CSV import data using AI-powered column mapping and value normalization.
    /// Analyzes headers and maps them to internal schema fields.
    /// </summary>
    /// <param name="request">Raw CSV data as string</param>
    /// <returns>List of normalized task import rows for user review</returns>
    /// <response code="200">Import data successfully normalized</response>
    /// <response code="400">Invalid CSV format or validation errors</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("import-normalize")]
    [ProducesResponseType(typeof(List<TaskImportRow>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TaskImportRow>>> NormalizeImport([FromBody] ImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CsvData))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid input",
                Detail = "CSV data cannot be empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _aiImportService.NormalizeImport(request.CsvData);
        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's personalized AI-generated daily digest.
    /// Includes tasks due today, overdue tasks, priority recommendations, and encouraging message.
    /// </summary>
    /// <returns>Daily digest for the current user</returns>
    /// <response code="200">Digest successfully retrieved</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("digest/me")]
    [ProducesResponseType(typeof(DigestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DigestResult>> GetMyDigest()
    {
        var userId = GetCurrentUserId();
        var result = await _dailyDigestService.GenerateDigest(userId);
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

/// <summary>
/// Request model for natural language task parsing
/// </summary>
public class ParseTaskRequest
{
    public string Input { get; set; } = string.Empty;
}

/// <summary>
/// Request model for semantic search
/// </summary>
public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
}

/// <summary>
/// Request model for CSV import normalization
/// </summary>
public class ImportRequest
{
    public string CsvData { get; set; } = string.Empty;
}
