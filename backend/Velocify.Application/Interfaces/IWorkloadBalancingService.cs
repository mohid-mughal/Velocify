using Velocify.Application.DTOs.AI;

namespace Velocify.Application.Interfaces;

/// <summary>
/// Service for AI-powered workload balancing and task redistribution suggestions.
/// Admin/SuperAdmin only feature.
/// Requirements: 11.1-11.6
/// </summary>
public interface IWorkloadBalancingService
{
    /// <summary>
    /// Analyzes current task assignments, productivity scores, and due dates for all team members.
    /// Provides structured JSON to LangChain for analysis.
    /// Returns suggestions with task ID, suggested assignee ID, and reasoning.
    /// Logs to AiInteractionLog with FeatureType.Prioritization.
    /// </summary>
    /// <returns>List of workload balancing suggestions</returns>
    Task<List<WorkloadSuggestion>> GetSuggestions();
}
