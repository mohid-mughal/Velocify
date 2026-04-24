using System.Diagnostics;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Velocify.Application.Interfaces;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Infrastructure.Services.AiServices;

/// <summary>
/// Background service that generates AI-powered daily digest summaries for all active users.
/// Runs daily at 8 AM and creates notifications that are pushed via SignalR when users connect.
/// Requirements: 10.1-10.7
/// </summary>
public class DailyDigestService : IDailyDigestService, IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyDigestService> _logger;
    private readonly IConfiguration _configuration;
    private Timer? _timer;

    // SCHEDULED EXECUTION TIME:
    // REQUIREMENT 10.1: "WHEN the daily digest job runs at 8 AM THEN the Backend SHALL generate a digest for each active user"
    // We run at 8 AM local time to ensure users receive their digest at the start of their workday.
    private static readonly TimeSpan ScheduledTime = new TimeSpan(8, 0, 0); // 8:00 AM

    public DailyDigestService(
        IServiceProvider serviceProvider,
        ILogger<DailyDigestService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Starts the hosted service and schedules the daily digest generation.
    /// REQUIREMENT 10.6: "THE Backend SHALL implement digest generation as an IHostedService that runs on a daily schedule"
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Daily Digest Service is starting. Scheduled to run daily at {ScheduledTime}", ScheduledTime);

        // Calculate time until next 8 AM
        var now = DateTime.Now;
        var nextRun = now.Date.Add(ScheduledTime);
        
        // If it's already past 8 AM today, schedule for tomorrow
        if (now > nextRun)
        {
            nextRun = nextRun.AddDays(1);
        }

        var timeUntilNextRun = nextRun - now;

        _logger.LogInformation(
            "Next daily digest generation scheduled for {NextRun} (in {Hours}h {Minutes}m)",
            nextRun,
            (int)timeUntilNextRun.TotalHours,
            timeUntilNextRun.Minutes);

        // Schedule the timer to run at 8 AM daily
        _timer = new Timer(
            callback: async _ => await ExecuteDailyDigestGeneration(),
            state: null,
            dueTime: timeUntilNextRun,
            period: TimeSpan.FromDays(1)); // Repeat every 24 hours

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the hosted service.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Daily Digest Service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the daily digest generation for all active users.
    /// This method is called by the timer at 8 AM daily.
    /// </summary>
    private async Task ExecuteDailyDigestGeneration()
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting daily digest generation for all active users");

        try
        {
            // Create a new scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<VelocifyDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var taskHubService = scope.ServiceProvider.GetRequiredService<ITaskHubService>();

            // Get all active users
            var activeUsers = await context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            _logger.LogInformation("Found {UserCount} active users for digest generation", activeUsers.Count);

            var successCount = 0;
            var failureCount = 0;

            // Generate digest for each active user
            foreach (var user in activeUsers)
            {
                try
                {
                    var digest = await GenerateDigest(user.Id);

                    // REQUIREMENT 10.3: "WHEN a digest is generated THEN the Backend SHALL store it as a Notification with Type set to AiSuggestion"
                    var notification = await notificationService.CreateNotification(
                        userId: user.Id,
                        type: NotificationType.AiSuggestion,
                        message: digest.Summary,
                        taskItemId: null);

                    // REQUIREMENT 10.4: "WHEN a user connects via SignalR THEN the Backend SHALL push any unread digest notifications to the client"
                    // We push the notification immediately; if the user is not connected, they'll receive it when they connect
                    await taskHubService.NotifyAiSuggestion(
                        userId: user.Id,
                        suggestionType: "DailyDigest",
                        message: digest.Summary);

                    successCount++;

                    _logger.LogInformation(
                        "Successfully generated digest for user {UserId} ({UserEmail}). Tasks due today: {DueToday}, Overdue: {Overdue}",
                        user.Id,
                        user.Email,
                        digest.TasksDueToday,
                        digest.OverdueTasks);
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(
                        ex,
                        "Failed to generate digest for user {UserId} ({UserEmail})",
                        user.Id,
                        user.Email);
                }
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "Daily digest generation completed. Success: {SuccessCount}, Failures: {FailureCount}, Duration: {ElapsedMs}ms",
                successCount,
                failureCount,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Critical error during daily digest generation. Duration: {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Generates a personalized daily digest for a specific user.
    /// REQUIREMENT 10.2: Include tasks due today, overdue tasks, priority recommendations, and encouraging message
    /// REQUIREMENT 10.7: Use LangChain summarization chain to generate digest content
    /// </summary>
    public async Task<DigestResult> GenerateDigest(Guid userId)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Generating daily digest for user {UserId}", userId);

        try
        {
            // Create a new scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<VelocifyDbContext>();

            // Get user information
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null)
            {
                throw new InvalidOperationException($"User {userId} not found or inactive");
            }

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // REQUIREMENT 10.2: Query tasks due today and overdue for the user
            var tasksDueToday = await context.TaskItems
                .Where(t => t.AssignedToUserId == userId &&
                           !t.IsDeleted &&
                           t.Status != TaskStatus.Completed &&
                           t.DueDate.HasValue &&
                           t.DueDate.Value >= today &&
                           t.DueDate.Value < tomorrow)
                .OrderBy(t => t.Priority)
                .ToListAsync();

            var overdueTasks = await context.TaskItems
                .Where(t => t.AssignedToUserId == userId &&
                           !t.IsDeleted &&
                           t.Status != TaskStatus.Completed &&
                           t.DueDate.HasValue &&
                           t.DueDate.Value < today)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            var allRelevantTasks = tasksDueToday.Concat(overdueTasks).ToList();

            _logger.LogInformation(
                "User {UserId} has {DueTodayCount} tasks due today and {OverdueCount} overdue tasks",
                userId,
                tasksDueToday.Count,
                overdueTasks.Count);

            // REQUIREMENT 10.7: Use LangChain summarization chain to generate digest content
            var digestSummary = await GenerateDigestWithLangChain(
                user,
                tasksDueToday,
                overdueTasks,
                allRelevantTasks);

            stopwatch.Stop();

            // Log AI interaction
            await LogAiInteraction(
                context,
                userId,
                tasksDueToday.Count,
                overdueTasks.Count,
                digestSummary,
                tokensUsed: null,
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            var result = new DigestResult
            {
                Summary = digestSummary,
                TasksDueToday = tasksDueToday.Count,
                OverdueTasks = overdueTasks.Count,
                PriorityRecommendations = ExtractPriorityRecommendations(allRelevantTasks),
                EncouragingMessage = ExtractEncouragingMessage(digestSummary)
            };

            _logger.LogInformation(
                "Successfully generated digest for user {UserId} in {ElapsedMs}ms",
                userId,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Failed to generate digest for user {UserId} after {ElapsedMs}ms",
                userId,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Generates digest content using LangChain summarization chain.
    /// REQUIREMENT 10.2: Include tasks due today, overdue tasks, priority recommendations, and encouraging message
    /// </summary>
    private async Task<string> GenerateDigestWithLangChain(
        User user,
        List<TaskItem> tasksDueToday,
        List<TaskItem> overdueTasks,
        List<TaskItem> allRelevantTasks)
    {
        // Get OpenAI API key from configuration
        var apiKey = _configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured");

        // Initialize OpenAI provider and chat model
        var provider = new OpenAiProvider(apiKey);
        var model = new OpenAiChatModel(provider, id: "gpt-3.5-turbo");

        // Build task summaries for the prompt
        var dueTodaySummary = tasksDueToday.Any()
            ? string.Join("\n", tasksDueToday.Select(t => $"- [{t.Priority}] {t.Title}"))
            : "No tasks due today";

        var overdueSummary = overdueTasks.Any()
            ? string.Join("\n", overdueTasks.Select(t => $"- [{t.Priority}] {t.Title} (Due: {t.DueDate:yyyy-MM-dd})"))
            : "No overdue tasks";

        // LANGCHAIN SUMMARIZATION PROMPT:
        // We provide structured task data and ask the model to generate a personalized,
        // encouraging daily digest that helps the user prioritize their work.
        //
        // REQUIREMENT 10.2: Include tasks due today, overdue tasks, priority recommendations, and encouraging message
        var prompt = $@"You are a helpful productivity assistant. Generate a personalized daily digest for {user.FirstName}.

Today's Date: {DateTime.UtcNow:yyyy-MM-dd}

Tasks Due Today ({tasksDueToday.Count}):
{dueTodaySummary}

Overdue Tasks ({overdueTasks.Count}):
{overdueSummary}

Instructions:
1. Start with a friendly greeting using the user's first name
2. Summarize the tasks due today, highlighting critical and high priority items
3. If there are overdue tasks, gently remind the user and suggest prioritizing them
4. Provide 2-3 specific priority recommendations based on task priority and due dates
5. End with an encouraging message to motivate the user
6. Keep the tone positive, supportive, and professional
7. Keep the total length under 500 words

Generate the daily digest:";

        var response = await model.GenerateAsync(prompt);
        var digestContent = response.LastMessageContent ?? "Unable to generate digest at this time.";

        return digestContent;
    }

    /// <summary>
    /// Extracts priority recommendations from tasks.
    /// </summary>
    private List<string> ExtractPriorityRecommendations(List<TaskItem> tasks)
    {
        var recommendations = new List<string>();

        // Recommend critical tasks first
        var criticalTasks = tasks.Where(t => t.Priority == TaskPriority.Critical).Take(3).ToList();
        foreach (var task in criticalTasks)
        {
            recommendations.Add($"Focus on critical task: {task.Title}");
        }

        // If no critical tasks, recommend high priority tasks
        if (!criticalTasks.Any())
        {
            var highPriorityTasks = tasks.Where(t => t.Priority == TaskPriority.High).Take(3).ToList();
            foreach (var task in highPriorityTasks)
            {
                recommendations.Add($"Prioritize: {task.Title}");
            }
        }

        return recommendations;
    }

    /// <summary>
    /// Extracts encouraging message from the digest summary.
    /// </summary>
    private string ExtractEncouragingMessage(string digestSummary)
    {
        // Extract the last paragraph as the encouraging message
        var lines = digestSummary.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        return lines.LastOrDefault() ?? "You've got this! Have a productive day!";
    }

    /// <summary>
    /// Logs AI interaction to the AiInteractionLog table.
    /// </summary>
    private async Task LogAiInteraction(
        VelocifyDbContext context,
        Guid userId,
        int tasksDueToday,
        int overdueTasks,
        string digestSummary,
        int? tokensUsed,
        int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.Digest,
                InputSummary = $"Tasks due today: {tasksDueToday}, Overdue: {overdueTasks}",
                OutputSummary = digestSummary.Length > 1000 
                    ? digestSummary.Substring(0, 1000) + "..." 
                    : digestSummary,
                TokensUsed = tokensUsed,
                LatencyMs = latencyMs,
                CreatedAt = DateTime.UtcNow
            };

            context.AiInteractionLogs.Add(log);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don't fail the main operation if logging fails
            _logger.LogError(ex, "Failed to log AI interaction for user {UserId}", userId);
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
