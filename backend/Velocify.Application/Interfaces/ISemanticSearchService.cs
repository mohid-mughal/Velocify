using Velocify.Application.DTOs.Tasks;

namespace Velocify.Application.Interfaces;

/// <summary>
/// Service for AI-powered semantic search using embedding vectors.
/// Requirements: 12.1-12.7
/// </summary>
public interface ISemanticSearchService
{
    /// <summary>
    /// Executes semantic search using both SQL LIKE search and embedding-based search in parallel.
    /// Generates query embedding with LangChain.
    /// Compares against TaskEmbedding table using cosine similarity.
    /// Merges and ranks results by combined score.
    /// Regenerates embeddings on task title/description change.
    /// Logs to AiInteractionLog with FeatureType.Search.
    /// </summary>
    /// <param name="query">Natural language search query</param>
    /// <returns>List of tasks ranked by relevance</returns>
    Task<List<TaskDto>> SearchTasks(string query);
}
