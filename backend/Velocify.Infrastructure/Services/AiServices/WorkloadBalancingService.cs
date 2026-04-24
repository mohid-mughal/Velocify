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
/// Service for AI-powered workload balancing and task redistribution suggestions.
/// Analyzes task assignments, productivity scores, and due dates to provide intelligent
/// suggestions for balancing workload across team members.
/// Admin/SuperAdmin only feature.
/// Requirements: 11.1-11.6
/// </summary>
public class WorkloadBalancingService : IWorkloadBalancingService
{
    private readonly VelocifyDbContext _context;
    private readonly ILogger<WorkloadBalancingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly AsyncRetryPolicy _retryPolicy;

    public WorkloadBalancingService(
        VelocifyDbContext context,
        ILogger<WorkloadBalancingService> logger,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;

        // RETRY POLICY EXPLANATION:
        // AI services can experience transient failures due to rate limiting, network issues, or temporary service degradation.
        // Without retries, admins would see errors for requests that could succeed on a second attempt.
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
                        "Workload balancing analysis failed on attempt {RetryCount}. Retrying after {RetryDelay}ms. Error: {ErrorMessage}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Analyzes current task assignments, productivity scores, and due dates for all team members.
    /// Provides structured JSON to LangChain for analysis.
    /// Returns suggestions with task ID, suggested assignee ID, and reasoning.
    /// REQUIREMENT 11.1: Analyze current task assignments, productivity scores, and due dates for all team members
    /// REQUIREMENT 11.2: Return suggestions with task ID, suggested assignee ID, and reasoning
    /// REQUIREMENT 11.5: Provide the AI Engine with structured JSON summary of team workload
    /// REQUIREMENT 11.6: Log workload balancing requests to AiInteractionLog with FeatureType.Prioritization
    /// </summary>
    public async Task<List<WorkloadSuggestion>> GetSuggestions()
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = GetCurrentUserId();

        try
        {
            _logger.LogInformation(
                "Starting workload balancing analysis for admin user {UserId}",
                userId);

            // REQUIREMENT 11.1: Analyze current task assignments, productivity scores, and due dates
            var workloadData = await GatherWorkloadData();

            if (workloadData.TeamMembers.Count == 0)
            {
                _logger.LogWarning("No team members found for workload analysis");
                return new List<WorkloadSuggestion>();
            }

            // Execute AI analysis with retry policy
            var suggestions = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await AnalyzeWithLangChain(workloadData);
            });

            stopwatch.Stop();

            // REQUIREMENT 11.6: Log to AiInteractionLog with FeatureType.Prioritization
            await LogAiInteraction(
                userId,
                workloadData,
                suggestions,
                tokensUsed: null, // LangChain 0.13.0 may not expose token counts directly
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            _logger.LogInformation(
                "Successfully generated {SuggestionCount} workload balancing suggestions for user {UserId} in {ElapsedMs}ms",
                suggestions.Count,
                userId,
                stopwatch.ElapsedMilliseconds);

            return suggestions;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Failed to generate workload balancing suggestions after all retry attempts for user {UserId}. Elapsed: {ElapsedMs}ms",
                userId,
                stopwatch.ElapsedMilliseconds);

            // Log failed interaction
            await LogFailedAiInteraction(
                userId,
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            throw new InvalidOperationException(
                "Unable to generate workload balancing suggestions. Please try again later.",
                ex);
        }
    }

    /// <summary>
    /// Gathers comprehensive workload data for all team members.
    /// REQUIREMENT 11.1: Analyze current task assignments, productivity scores, and due dates
    /// REQUIREMENT 11.5: Provide structured JSON summary of team workload including task counts and productivity metrics
    /// </summary>
    private async Task<WorkloadData> GatherWorkloadData()
    {
        // Get all active users with their productivity scores
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Select(u => new TeamMemberWorkload
            {
                UserId = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                ProductivityScore = u.ProductivityScore,
                Role = u.Role
            })
            .ToListAsync();

        // Get task counts and workload metrics for each user
        foreach (var user in users)
        {
            var tasks = await _context.TaskItems
                .AsNoTracking()
                .Where(t => t.AssignedToUserId == user.UserId && !t.IsDeleted)
                .ToListAsync();

            user.TotalTaskCount = tasks.Count;
            user.PendingTaskCount = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Pending);
            user.InProgressTaskCount = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.InProgress);
            user.BlockedTaskCount = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Blocked);
            user.OverdueTaskCount = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Completed);
            user.CriticalTaskCount = tasks.Count(t => t.Priority == TaskPriority.Critical);
            user.HighPriorityTaskCount = tasks.Count(t => t.Priority == TaskPriority.High);
            user.TotalEstimatedHours = tasks.Where(t => t.EstimatedHours.HasValue).Sum(t => t.EstimatedHours!.Value);
        }

        // Get overloaded users' tasks that could be redistributed
        var overloadedUsers = users
            .Where(u => u.TotalTaskCount > 0)
            .OrderByDescending(u => u.TotalTaskCount)
            .Take(5) // Focus on top 5 most loaded users
            .ToList();

        var redistributableTasks = new List<RedistributableTask>();

        foreach (var user in overloadedUsers)
        {
            var tasks = await _context.TaskItems
                .AsNoTracking()
                .Where(t => t.AssignedToUserId == user.UserId 
                    && !t.IsDeleted 
                    && t.Status != Domain.Enums.TaskStatus.Completed
                    && t.Status != Domain.Enums.TaskStatus.Cancelled)
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .Take(10) // Consider top 10 tasks per overloaded user
                .ToListAsync();

            redistributableTasks.AddRange(tasks.Select(t => new RedistributableTask
            {
                TaskId = t.Id,
                Title = t.Title,
                Priority = t.Priority,
                Status = t.Status,
                Category = t.Category,
                CurrentAssigneeId = t.AssignedToUserId,
                DueDate = t.DueDate,
                EstimatedHours = t.EstimatedHours
            }));
        }

        return new WorkloadData
        {
            TeamMembers = users,
            RedistributableTasks = redistributableTasks,
            AnalysisTimestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Performs the actual AI analysis using LangChain to generate workload balancing suggestions.
    /// REQUIREMENT 11.2: Return suggestions with task ID, suggested assignee ID, and reasoning
    /// REQUIREMENT 11.5: Provide structured JSON summary of team workload
    /// </summary>
    private async Task<List<WorkloadSuggestion>> AnalyzeWithLangChain(WorkloadData workloadData)
    {
        // Get OpenAI API key from configuration
        var apiKey = _configuration["OpenAI:ApiKey"] 
            ?? throw new InvalidOperationException("OpenAI API key not configured");

        // Initialize OpenAI provider and chat model
        // Using gpt-3.5-turbo for fast, cost-effective analysis
        var provider = new OpenAiProvider(apiKey);
        var model = new OpenAiChatModel(provider, id: "gpt-3.5-turbo");

        // REQUIREMENT 11.5: Provide structured JSON summary of team workload
        var workloadSummary = System.Text.Json.JsonSerializer.Serialize(workloadData, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        });

        // LANGCHAIN STRUCTURED OUTPUT PARSER:
        // We provide comprehensive workload data and ask the AI to identify imbalances
        // and suggest specific task reassignments with reasoning.
        //
        // REQUIREMENT 11.1: Analyze task assignments, productivity scores, and due dates
        // REQUIREMENT 11.2: Return task ID, suggested assignee ID, and reasoning
        var prompt = $@"You are a workload balancing assistant for a task management system. Analyze the team workload data and suggest task reassignments to balance the workload.

Team Workload Data:
{workloadSummary}

Analysis Guidelines:
- Identify team members who are overloaded (high task count, many overdue tasks, low productivity score)
- Identify team members who have capacity (lower task count, higher productivity score)
- Consider task priority (Critical and High priority tasks need experienced assignees)
- Consider task category (match tasks to team members with relevant experience if possible)
- Consider due dates (urgent tasks should go to members with fewer urgent tasks)
- Suggest reassigning 3-8 tasks maximum (focus on high-impact reassignments)
- Provide clear reasoning for each suggestion

Return JSON in this exact format (array of suggestions):
[
  {{
    ""taskId"": ""guid-of-task-to-reassign"",
    ""suggestedAssigneeId"": ""guid-of-new-assignee"",
    ""reason"": ""Clear explanation of why this reassignment balances workload""
  }},
  {{
    ""taskId"": ""another-task-guid"",
    ""suggestedAssigneeId"": ""another-assignee-guid"",
    ""reason"": ""Another clear explanation""
  }}
]

Important:
- Only suggest reassignments if there is a clear workload imbalance
- If workload is already balanced, return an empty array []
- Use actual GUIDs from the provided data
- Return ONLY valid JSON, no additional text";

        var response = await model.GenerateAsync(prompt);
        var jsonResponse = response.LastMessageContent ?? "[]";

        // Parse the JSON response into List<WorkloadSuggestion>
        var suggestions = System.Text.Json.JsonSerializer.Deserialize<List<WorkloadSuggestion>>(
            jsonResponse,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<WorkloadSuggestion>();

        // Validate that suggested task IDs and assignee IDs exist in the workload data
        var validTaskIds = workloadData.RedistributableTasks.Select(t => t.TaskId).ToHashSet();
        var validUserIds = workloadData.TeamMembers.Select(u => u.UserId).ToHashSet();

        suggestions = suggestions
            .Where(s => validTaskIds.Contains(s.TaskId) && validUserIds.Contains(s.SuggestedAssigneeId))
            .ToList();

        if (suggestions.Count < System.Text.Json.JsonSerializer.Deserialize<List<WorkloadSuggestion>>(jsonResponse)?.Count)
        {
            _logger.LogWarning(
                "Some AI suggestions were filtered out due to invalid task or user IDs");
        }

        return suggestions;
    }

    /// <summary>
    /// Logs AI interaction to the AiInteractionLog table for tracking and analytics.
    /// REQUIREMENT 11.6: Log workload balancing requests to AiInteractionLog with FeatureType.Prioritization
    /// </summary>
    private async Task LogAiInteraction(
        Guid userId,
        WorkloadData workloadData,
        List<WorkloadSuggestion> suggestions,
        int? tokensUsed,
        int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.Prioritization,
                InputSummary = $"Analyzed {workloadData.TeamMembers.Count} team members with {workloadData.RedistributableTasks.Count} redistributable tasks",
                OutputSummary = suggestions.Count > 0
                    ? $"Generated {suggestions.Count} workload balancing suggestions"
                    : "No workload imbalance detected",
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
    private async Task LogFailedAiInteraction(Guid userId, int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.Prioritization,
                InputSummary = "Workload balancing analysis",
                OutputSummary = "Failed to generate suggestions",
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

/// <summary>
/// Internal data structure for workload analysis.
/// Contains comprehensive team workload information for AI analysis.
/// </summary>
internal class WorkloadData
{
    public List<TeamMemberWorkload> TeamMembers { get; set; } = new();
    public List<RedistributableTask> RedistributableTasks { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; }
}

/// <summary>
/// Workload metrics for a single team member.
/// </summary>
internal class TeamMemberWorkload
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal ProductivityScore { get; set; }
    public UserRole Role { get; set; }
    public int TotalTaskCount { get; set; }
    public int PendingTaskCount { get; set; }
    public int InProgressTaskCount { get; set; }
    public int BlockedTaskCount { get; set; }
    public int OverdueTaskCount { get; set; }
    public int CriticalTaskCount { get; set; }
    public int HighPriorityTaskCount { get; set; }
    public decimal TotalEstimatedHours { get; set; }
}

/// <summary>
/// Task that could potentially be redistributed to balance workload.
/// </summary>
internal class RedistributableTask
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public Domain.Enums.TaskStatus Status { get; set; }
    public TaskCategory Category { get; set; }
    public Guid CurrentAssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
}
