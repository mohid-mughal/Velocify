namespace Velocify.Application.Interfaces;

/// <summary>
/// Service for generating AI-powered daily digest summaries.
/// Implemented as IHostedService that runs daily at 8 AM.
/// Requirements: 10.1-10.7
/// </summary>
public interface IDailyDigestService
{
    /// <summary>
    /// Generates a personalized daily digest for a user.
    /// Includes tasks due today, overdue tasks, priority recommendations, and encouraging message.
    /// Uses LangChain summarization chain.
    /// Creates Notification with Type.AiSuggestion.
    /// Pushes via SignalR when user connects.
    /// </summary>
    /// <param name="userId">ID of the user to generate digest for</param>
    /// <returns>Generated digest content</returns>
    Task<DigestResult> GenerateDigest(Guid userId);
}

/// <summary>
/// Result of daily digest generation
/// </summary>
public class DigestResult
{
    public string Summary { get; set; } = string.Empty;
    public int TasksDueToday { get; set; }
    public int OverdueTasks { get; set; }
    public List<string> PriorityRecommendations { get; set; } = new();
    public string EncouragingMessage { get; set; } = string.Empty;
}
