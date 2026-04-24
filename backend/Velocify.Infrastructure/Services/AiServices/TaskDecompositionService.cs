using System.Diagnostics;
using System.Security.Claims;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Velocify.Application.DTOs.AI;
using Velocify.Application.Interfaces;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;

namespace Velocify.Infrastructure.Services.AiServices;

/// <summary>
/// Service for AI-powered task decomposition into subtasks using LangChain.NET.
/// Implements retry policy with Polly for resilience against transient AI service failures.
/// Requirements: 9.1-9.6
/// </summary>
public class TaskDecompositionService : ITaskDecompositionService
{
    private readonly VelocifyDbContext _context;
    private readonly ILogger<TaskDecompositionService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly AsyncRetryPolicy _retryPolicy;

    public TaskDecompositionService(
        VelocifyDbContext context,
        ILogger<TaskDecompositionService> logger,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;

        // RETRY POLICY EXPLANATION:
        // AI services can experience transient failures due to rate limiting, network issues, or temporary service degradation.
        // Without retries, users would see errors for requests that could succeed on a second attempt.
        //
        // EXPONENTIAL BACKOFF STRATEGY:
        // - Attempt 1: Immediate execution
        // - Attempt 2: Wait 1 second (2^0 = 1)
        // - Attempt 3: Wait 2 seconds (2^1 = 2)
        // - Attempt 4: Wait 4 seconds (2^2 = 4)
        //
        // This pattern prevents overwhelming the AI service during outages while giving it time to recover.
        // Total maximum wait time: 1 + 2 + 4 = 7 seconds across 3 retries (4 total attempts).
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Task decomposition failed on attempt {RetryCount}. Retrying after {RetryDelay}ms. Error: {ErrorMessage}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Analyzes a task and generates subtask suggestions using LangChain structured output parser.
    /// REQUIREMENT 9.1: Analyze the title and description and generate up to 8 subtask suggestions
    /// REQUIREMENT 9.2: Return a list of suggested titles and estimated hours
    /// REQUIREMENT 9.5: Use LangChain structured output parser to extract subtask information
    /// REQUIREMENT 9.6: Log decomposition requests to AiInteractionLog with FeatureType.Decomposition
    /// </summary>
    public async Task<List<SubtaskSuggestion>> DecomposeTask(Guid taskId)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = GetCurrentUserId();

        try
        {
            // Retrieve the task from the database
            var task = await _context.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
            {
                throw new InvalidOperationException($"Task with ID {taskId} not found or has been deleted");
            }

            _logger.LogInformation(
                "Starting task decomposition for task {TaskId} ('{TaskTitle}') by user {UserId}",
                taskId,
                task.Title,
                userId);

            // Execute AI decomposition with retry policy
            var subtasks = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await DecomposeWithLangChain(task);
            });

            stopwatch.Stop();

            // REQUIREMENT 9.6: Log to AiInteractionLog with FeatureType.Decomposition
            await LogAiInteraction(
                userId,
                task,
                subtasks,
                tokensUsed: null, // LangChain 0.13.0 may not expose token counts directly
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            _logger.LogInformation(
                "Successfully decomposed task {TaskId} into {SubtaskCount} subtasks for user {UserId} in {ElapsedMs}ms",
                taskId,
                subtasks.Count,
                userId,
                stopwatch.ElapsedMilliseconds);

            return subtasks;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Failed to decompose task {TaskId} after all retry attempts for user {UserId}. Elapsed: {ElapsedMs}ms",
                taskId,
                userId,
                stopwatch.ElapsedMilliseconds);

            // Log failed interaction
            await LogFailedAiInteraction(
                userId,
                taskId,
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            throw new InvalidOperationException(
                "Unable to decompose task into subtasks. Please try again or create subtasks manually.",
                ex);
        }
    }

    /// <summary>
    /// Performs the actual AI decomposition using LangChain structured output parser.
    /// Uses OpenAI GPT model to break down a complex task into smaller subtasks.
    /// REQUIREMENT 9.1: Cap subtask generation at 8 items
    /// </summary>
    private async Task<List<SubtaskSuggestion>> DecomposeWithLangChain(TaskItem task)
    {
        // Get OpenAI API key from configuration
        var apiKey = _configuration["OpenAI:ApiKey"] 
            ?? throw new InvalidOperationException("OpenAI API key not configured");

        // Initialize OpenAI provider and chat model
        // Using gpt-3.5-turbo for fast, cost-effective decomposition
        var provider = new OpenAiProvider(apiKey);
        var model = new OpenAiChatModel(provider, id: "gpt-3.5-turbo");

        // LANGCHAIN STRUCTURED OUTPUT PARSER:
        // We use a carefully crafted prompt to instruct the model to break down the task
        // and return subtasks in a structured JSON format that we can parse reliably.
        //
        // REQUIREMENT 9.1: Generate up to 8 subtask suggestions
        // REQUIREMENT 9.2: Return Title and EstimatedHours for each subtask
        var prompt = $@"You are a task decomposition assistant. Break down the following task into smaller, actionable subtasks.

Task Information:
Title: {task.Title}
Description: {task.Description}
Priority: {task.Priority}
Category: {task.Category}
Estimated Hours: {task.EstimatedHours?.ToString() ?? "Not specified"}

Instructions:
- Generate between 3 and 8 subtasks (maximum 8)
- Each subtask should be specific, actionable, and independently completable
- Provide a clear title for each subtask (max 200 characters)
- Estimate hours for each subtask (can be decimal, e.g., 0.5, 1.5, 2.0)
- The sum of subtask hours should roughly equal or be less than the parent task's estimated hours
- If the task is already small or simple, generate fewer subtasks
- Return ONLY valid JSON, no additional text

Return JSON in this exact format (array of subtasks):
[
  {{
    ""title"": ""Subtask title here"",
    ""estimatedHours"": 2.0
  }},
  {{
    ""title"": ""Another subtask title"",
    ""estimatedHours"": 1.5
  }}
]";

        var response = await model.GenerateAsync(prompt);
        var jsonResponse = response.LastMessageContent ?? "[]";

        // Parse the JSON response into List<SubtaskSuggestion>
        var subtasks = System.Text.Json.JsonSerializer.Deserialize<List<SubtaskSuggestion>>(
            jsonResponse,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<SubtaskSuggestion>();

        // REQUIREMENT 9.1: Cap subtask generation at 8 items
        if (subtasks.Count > 8)
        {
            _logger.LogWarning(
                "AI generated {SubtaskCount} subtasks, capping at 8 as per requirement 9.1",
                subtasks.Count);
            subtasks = subtasks.Take(8).ToList();
        }

        return subtasks;
    }

    /// <summary>
    /// Logs AI interaction to the AiInteractionLog table for tracking and analytics.
    /// REQUIREMENT 9.6: Log decomposition requests to AiInteractionLog with FeatureType.Decomposition
    /// </summary>
    private async Task LogAiInteraction(
        Guid userId,
        TaskItem task,
        List<SubtaskSuggestion> subtasks,
        int? tokensUsed,
        int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.Decomposition,
                InputSummary = $"Task: {task.Title} (ID: {task.Id})",
                OutputSummary = $"Generated {subtasks.Count} subtasks: {string.Join(", ", subtasks.Take(3).Select(s => s.Title))}{(subtasks.Count > 3 ? "..." : "")}",
                TokensUsed = tokensUsed,
                LatencyMs = latencyMs,
                CreatedAt = DateTime.UtcNow
            };

            _context.AiInteractionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don't fail the main operation if logging fails
            _logger.LogError(ex, "Failed to log AI interaction for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Logs a failed AI interaction attempt.
    /// </summary>
    private async Task LogFailedAiInteraction(Guid userId, Guid taskId, int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.Decomposition,
                InputSummary = $"Task ID: {taskId}",
                OutputSummary = "Failed to decompose task",
                TokensUsed = null,
                LatencyMs = latencyMs,
                CreatedAt = DateTime.UtcNow
            };

            _context.AiInteractionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don't fail the main operation if logging fails
            _logger.LogError(ex, "Failed to log failed AI interaction for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Gets the current user ID from the HTTP context claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            // For background jobs or system operations, use a system user ID
            return Guid.Empty;
        }

        return userId;
    }
}
