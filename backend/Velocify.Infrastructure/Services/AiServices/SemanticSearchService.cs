using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;

namespace Velocify.Infrastructure.Services.AiServices;

/// <summary>
/// Service for AI-powered semantic search using embedding vectors.
/// Executes both SQL LIKE search and embedding-based search in parallel.
/// Requirements: 12.1-12.7
/// </summary>
public class SemanticSearchService : ISemanticSearchService
{
    private readonly VelocifyDbContext _context;
    private readonly ILogger<SemanticSearchService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    // Weights for combining SQL and semantic search scores
    private const double SqlSearchWeight = 0.4;
    private const double SemanticSearchWeight = 0.6;

    public SemanticSearchService(
        VelocifyDbContext context,
        ILogger<SemanticSearchService> logger,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _mapper = mapper;
    }

    /// <summary>
    /// Executes semantic search using both SQL LIKE search and embedding-based search in parallel.
    /// REQUIREMENT 12.1: Execute both SQL LIKE search and embedding-based semantic search in parallel
    /// REQUIREMENT 12.2: Generate an embedding vector for the query using LangChain
    /// REQUIREMENT 12.3: Retrieve cached embeddings from TaskEmbedding table and calculate similarity scores
    /// REQUIREMENT 12.4: Combine and rank results by combined relevance score
    /// REQUIREMENT 12.7: Log semantic search requests to AiInteractionLog with FeatureType.Search
    /// </summary>
    public async Task<List<TaskDto>> SearchTasks(string query)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = GetCurrentUserId();

        try
        {
            _logger.LogInformation(
                "Starting semantic search for user {UserId}. Query: {Query}",
                userId,
                query);

            // REQUIREMENT 12.1: Run SQL LIKE search and embedding search in parallel (Task.WhenAll)
            // This parallelization reduces total search latency by executing both searches concurrently
            var sqlSearchTask = ExecuteSqlLikeSearch(query, userId);
            var semanticSearchTask = ExecuteSemanticSearch(query, userId);

            await Task.WhenAll(sqlSearchTask, semanticSearchTask);

            var sqlResults = await sqlSearchTask;
            var semanticResults = await semanticSearchTask;

            // REQUIREMENT 12.4: Merge and rank results by combined score
            var mergedResults = MergeAndRankResults(sqlResults, semanticResults);

            stopwatch.Stop();

            // REQUIREMENT 12.7: Log to AiInteractionLog with FeatureType.Search
            await LogAiInteraction(
                userId,
                query,
                mergedResults.Count,
                tokensUsed: null,
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            _logger.LogInformation(
                "Semantic search completed for user {UserId} in {ElapsedMs}ms. Found {ResultCount} results",
                userId,
                stopwatch.ElapsedMilliseconds,
                mergedResults.Count);

            return mergedResults;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Semantic search failed for user {UserId}. Query: {Query}. Elapsed: {ElapsedMs}ms",
                userId,
                query,
                stopwatch.ElapsedMilliseconds);

            // Log failed interaction
            await LogAiInteraction(
                userId,
                query,
                resultCount: 0,
                tokensUsed: null,
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            throw new InvalidOperationException(
                "Unable to complete semantic search. Please try again.",
                ex);
        }
    }

    /// <summary>
    /// Executes traditional SQL LIKE search on task title, description, and tags.
    /// Returns tasks with relevance scores based on match quality.
    /// </summary>
    private async Task<List<(TaskItem Task, double Score)>> ExecuteSqlLikeSearch(string query, Guid userId)
    {
        var searchTerm = $"%{query}%";

        // Query tasks that match the search term in title, description, or tags
        // Score is based on where the match occurs (title matches score higher)
        var tasks = await _context.TaskItems
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => !t.IsDeleted)
            .Where(t => EF.Functions.Like(t.Title, searchTerm) ||
                       EF.Functions.Like(t.Description, searchTerm) ||
                       EF.Functions.Like(t.Tags, searchTerm))
            .AsNoTracking()
            .ToListAsync();

        // Calculate relevance scores
        var results = tasks.Select(task =>
        {
            double score = 0.0;

            // Title matches are most relevant
            if (task.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                score += 1.0;
            }

            // Description matches are moderately relevant
            if (task.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.5;
            }

            // Tag matches are less relevant
            if (task.Tags.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.3;
            }

            return (Task: task, Score: score);
        })
        .Where(r => r.Score > 0)
        .ToList();

        _logger.LogDebug(
            "SQL LIKE search found {Count} results for query: {Query}",
            results.Count,
            query);

        return results;
    }

    /// <summary>
    /// Executes embedding-based semantic search using cosine similarity.
    /// REQUIREMENT 12.2: Generate query embedding with LangChain
    /// REQUIREMENT 12.3: Compare against TaskEmbedding table (cosine similarity)
    /// </summary>
    private async Task<List<(TaskItem Task, double Score)>> ExecuteSemanticSearch(string query, Guid userId)
    {
        try
        {
            // REQUIREMENT 12.2: Generate query embedding with LangChain
            var queryEmbedding = await GenerateEmbedding(query);

            // REQUIREMENT 12.3: Retrieve cached embeddings from TaskEmbedding table
            var taskEmbeddings = await _context.TaskEmbeddings
                .Include(te => te.TaskItem)
                    .ThenInclude(t => t.AssignedTo)
                .Include(te => te.TaskItem)
                    .ThenInclude(t => t.CreatedBy)
                .Where(te => !te.TaskItem.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            // Calculate cosine similarity for each task
            var results = new List<(TaskItem Task, double Score)>();

            foreach (var taskEmbedding in taskEmbeddings)
            {
                var taskVector = DeserializeEmbedding(taskEmbedding.EmbeddingVector);
                var similarity = CalculateCosineSimilarity(queryEmbedding, taskVector);

                // Only include results with meaningful similarity (> 0.5)
                if (similarity > 0.5)
                {
                    results.Add((Task: taskEmbedding.TaskItem, Score: similarity));
                }
            }

            _logger.LogDebug(
                "Semantic search found {Count} results for query: {Query}",
                results.Count,
                query);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Semantic search failed, returning empty results. Query: {Query}",
                query);

            // Return empty results if semantic search fails
            // SQL search results will still be available
            return new List<(TaskItem Task, double Score)>();
        }
    }

    /// <summary>
    /// Generates an embedding vector for the given text using LangChain OpenAI embeddings.
    /// REQUIREMENT 12.2: Generate embedding vector for query using LangChain
    /// </summary>
    private async Task<float[]> GenerateEmbedding(string text)
    {
        var apiKey = _configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured");

        var provider = new OpenAiProvider(apiKey);
        
        // Use OpenAI's text-embedding-ada-002 model for generating embeddings
        // This model produces 1536-dimensional vectors optimized for semantic similarity
        var embeddingModel = new OpenAiEmbeddingModel(provider, id: "text-embedding-ada-002");

        var response = await embeddingModel.CreateEmbeddingsAsync(text);
        
        // Extract the embedding vector from the response
        // The response.Values contains the embedding vectors
        var embedding = response.Values.FirstOrDefault();
        
        if (embedding == null || embedding.Length == 0)
        {
            throw new InvalidOperationException("Failed to generate embedding vector");
        }

        return embedding;
    }

    /// <summary>
    /// Calculates cosine similarity between two embedding vectors.
    /// REQUIREMENT 12.3: Calculate similarity scores using cosine similarity
    /// 
    /// Cosine similarity formula: cos(θ) = (A · B) / (||A|| * ||B||)
    /// Returns a value between -1 and 1, where 1 means identical vectors
    /// </summary>
    private double CalculateCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        double dotProduct = 0.0;
        double magnitude1 = 0.0;
        double magnitude2 = 0.0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0.0 || magnitude2 == 0.0)
        {
            return 0.0;
        }

        return dotProduct / (magnitude1 * magnitude2);
    }

    /// <summary>
    /// Merges SQL and semantic search results, combining scores and ranking by relevance.
    /// REQUIREMENT 12.4: Combine and rank results by combined relevance score
    /// </summary>
    private List<TaskDto> MergeAndRankResults(
        List<(TaskItem Task, double Score)> sqlResults,
        List<(TaskItem Task, double Score)> semanticResults)
    {
        // Create a dictionary to combine scores for tasks found in both searches
        var combinedScores = new Dictionary<Guid, (TaskItem Task, double CombinedScore)>();

        // Add SQL search results with weighted scores
        foreach (var (task, score) in sqlResults)
        {
            var weightedScore = score * SqlSearchWeight;
            combinedScores[task.Id] = (Task: task, CombinedScore: weightedScore);
        }

        // Add or update with semantic search results
        foreach (var (task, score) in semanticResults)
        {
            var weightedScore = score * SemanticSearchWeight;

            if (combinedScores.ContainsKey(task.Id))
            {
                // Task found in both searches - combine scores
                var existing = combinedScores[task.Id];
                combinedScores[task.Id] = (
                    Task: existing.Task,
                    CombinedScore: existing.CombinedScore + weightedScore
                );
            }
            else
            {
                // Task only found in semantic search
                combinedScores[task.Id] = (Task: task, CombinedScore: weightedScore);
            }
        }

        // Sort by combined score (descending) and convert to DTOs
        var rankedResults = combinedScores.Values
            .OrderByDescending(r => r.CombinedScore)
            .Select(r => _mapper.Map<TaskDto>(r.Task))
            .ToList();

        return rankedResults;
    }

    /// <summary>
    /// Deserializes embedding vector from JSON string format.
    /// REQUIREMENT 12.6: Embedding vectors stored as JSON arrays in nvarchar(max) columns
    /// </summary>
    private float[] DeserializeEmbedding(string embeddingJson)
    {
        try
        {
            var embedding = JsonSerializer.Deserialize<float[]>(embeddingJson);
            return embedding ?? Array.Empty<float>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to deserialize embedding vector. JSON: {Json}",
                embeddingJson.Substring(0, Math.Min(100, embeddingJson.Length)));
            
            return Array.Empty<float>();
        }
    }

    /// <summary>
    /// Generates and stores an embedding for a task.
    /// REQUIREMENT 12.5: Regenerate embeddings on task title/description change
    /// This method should be called whenever a task's title or description is updated.
    /// </summary>
    public async Task RegenerateTaskEmbedding(Guid taskId)
    {
        try
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found for embedding regeneration", taskId);
                return;
            }

            // Combine title and description for embedding
            var textToEmbed = $"{task.Title} {task.Description}";
            var embedding = await GenerateEmbedding(textToEmbed);
            var embeddingJson = JsonSerializer.Serialize(embedding);

            // Check if embedding already exists
            var existingEmbedding = await _context.TaskEmbeddings
                .FirstOrDefaultAsync(te => te.TaskItemId == taskId);

            if (existingEmbedding != null)
            {
                // Update existing embedding
                existingEmbedding.EmbeddingVector = embeddingJson;
                existingEmbedding.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new embedding
                var newEmbedding = new TaskEmbedding
                {
                    Id = Guid.NewGuid(),
                    TaskItemId = taskId,
                    EmbeddingVector = embeddingJson,
                    CreatedAt = DateTime.UtcNow
                };
                _context.TaskEmbeddings.Add(newEmbedding);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully regenerated embedding for task {TaskId}",
                taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to regenerate embedding for task {TaskId}",
                taskId);
            
            // Don't throw - embedding regeneration is not critical
        }
    }

    /// <summary>
    /// Logs AI interaction to the AiInteractionLog table for tracking and analytics.
    /// REQUIREMENT 12.7: Log semantic search requests to AiInteractionLog with FeatureType.Search
    /// </summary>
    private async Task LogAiInteraction(
        Guid userId,
        string query,
        int resultCount,
        int? tokensUsed,
        int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.Search,
                InputSummary = query.Length > 1000 ? query.Substring(0, 1000) + "..." : query,
                OutputSummary = $"Found {resultCount} results",
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
