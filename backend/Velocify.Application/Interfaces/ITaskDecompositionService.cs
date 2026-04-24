using Velocify.Application.DTOs.AI;

namespace Velocify.Application.Interfaces;

/// <summary>
/// Service for AI-powered task decomposition into subtasks.
/// Requirements: 9.1-9.6
/// </summary>
public interface ITaskDecompositionService
{
    /// <summary>
    /// Analyzes a task and generates subtask suggestions.
    /// Uses LangChain structured output parser.
    /// Caps subtask generation at 8 items.
    /// Logs to AiInteractionLog with FeatureType.Decomposition.
    /// </summary>
    /// <param name="taskId">ID of the task to decompose</param>
    /// <returns>List of suggested subtasks with titles and estimated hours</returns>
    Task<List<SubtaskSuggestion>> DecomposeTask(Guid taskId);
}
