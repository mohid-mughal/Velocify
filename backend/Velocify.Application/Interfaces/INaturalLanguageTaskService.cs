using Velocify.Domain.Enums;

namespace Velocify.Application.Interfaces;

/// <summary>
/// Service for parsing natural language text into structured task data using AI.
/// Requirements: 8.1-8.7
/// </summary>
public interface INaturalLanguageTaskService
{
    /// <summary>
    /// Parses natural language text and extracts task information.
    /// Uses LangChain structured output parser with Polly retry policy (3 retries, exponential backoff).
    /// Logs to AiInteractionLog with FeatureType.TaskCreation.
    /// </summary>
    /// <param name="input">Natural language text describing the task</param>
    /// <returns>Parsed task information including title, description, priority, category, assignee email, and due date</returns>
    Task<ParsedTaskResult> ParseTaskFromText(string input);
}

/// <summary>
/// Result of natural language task parsing
/// </summary>
public class ParsedTaskResult
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TaskPriority? Priority { get; set; }
    public TaskCategory? Category { get; set; }
    public string? AssigneeEmail { get; set; }
    public DateTime? DueDate { get; set; }
}
