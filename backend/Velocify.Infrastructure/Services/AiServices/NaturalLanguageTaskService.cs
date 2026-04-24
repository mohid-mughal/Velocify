using System.Diagnostics;
using System.Security.Claims;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Velocify.Application.Interfaces;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;

namespace Velocify.Infrastructure.Services.AiServices;

/// <summary>
/// Service for parsing natural language text into structured task data using LangChain.NET.
/// Implements retry policy with Polly for resilience against transient AI service failures.
/// Requirements: 8.1-8.7
/// </summary>
public class NaturalLanguageTaskService : INaturalLanguageTaskService
{
    private readonly VelocifyDbContext _context;
    private readonly ILogger<NaturalLanguageTaskService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly AsyncRetryPolicy _retryPolicy;

    public NaturalLanguageTaskService(
        VelocifyDbContext context,
        ILogger<NaturalLanguageTaskService> logger,
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
        //
        // REQUIREMENT 8.4: "WHEN the AI Engine call fails THEN the Backend SHALL retry up to 3 times with exponential backoff using Polly"
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Natural language task parsing failed on attempt {RetryCount}. Retrying after {RetryDelay}ms. Error: {ErrorMessage}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Parses natural language text and extracts task information using LangChain structured output parser.
    /// REQUIREMENT 8.1: Extract title, description, priority, category, assignee email, and due date
    /// REQUIREMENT 8.4: Retry up to 3 times with exponential backoff using Polly
    /// REQUIREMENT 8.6: Log all AI interactions to AiInteractionLog
    /// </summary>
    public async Task<ParsedTaskResult> ParseTaskFromText(string input)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = GetCurrentUserId();

        try
        {
            _logger.LogInformation(
                "Starting natural language task parsing for user {UserId}. Input length: {InputLength} characters",
                userId,
                input.Length);

            // Execute AI parsing with retry policy
            var result = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await ParseWithLangChain(input);
            });

            stopwatch.Stop();

            // REQUIREMENT 8.6: Log all AI interactions to AiInteractionLog
            await LogAiInteraction(
                userId,
                input,
                result,
                tokensUsed: null, // LangChain 0.13.0 may not expose token counts directly
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            _logger.LogInformation(
                "Successfully parsed natural language task for user {UserId} in {ElapsedMs}ms. Title: {Title}",
                userId,
                stopwatch.ElapsedMilliseconds,
                result.Title ?? "(none)");

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // REQUIREMENT 8.5: When all retry attempts fail, return an error response with a user-friendly message
            _logger.LogError(
                ex,
                "Failed to parse natural language task after all retry attempts for user {UserId}. Elapsed: {ElapsedMs}ms",
                userId,
                stopwatch.ElapsedMilliseconds);

            // Log failed interaction
            await LogAiInteraction(
                userId,
                input,
                null,
                tokensUsed: null,
                latencyMs: (int)stopwatch.ElapsedMilliseconds);

            throw new InvalidOperationException(
                "Unable to parse task from natural language input. Please try again or use the manual form.",
                ex);
        }
    }

    /// <summary>
    /// Performs the actual AI parsing using LangChain structured output parser.
    /// Uses OpenAI GPT model to extract structured task information from natural language.
    /// </summary>
    private async Task<ParsedTaskResult> ParseWithLangChain(string input)
    {
        // Get OpenAI API key from configuration
        var apiKey = _configuration["OpenAI:ApiKey"] 
            ?? throw new InvalidOperationException("OpenAI API key not configured");

        // Initialize OpenAI provider and chat model
        // Using gpt-3.5-turbo for fast, cost-effective parsing
        var provider = new OpenAiProvider(apiKey);
        var model = new OpenAiChatModel(provider, id: "gpt-3.5-turbo");

        // LANGCHAIN STRUCTURED OUTPUT PARSER:
        // We use a carefully crafted prompt to instruct the model to extract specific fields
        // and return them in a structured JSON format that we can parse reliably.
        //
        // REQUIREMENT 8.1: Extract title, description, priority, category, assignee email, and due date
        // REQUIREMENT 8.3: Return partial results with null values for unparsed fields
        var prompt = $@"You are a task parsing assistant. Extract the following information from the user's natural language input and return it as JSON.

Fields to extract:
- title: A concise task title (required, max 200 characters)
- description: Detailed task description (optional)
- priority: One of: Critical, High, Medium, Low (optional, default to Medium if unclear)
- category: One of: Development, Design, Marketing, Operations, Research, Other (optional, default to Other if unclear)
- assigneeEmail: Email address of the person to assign the task to (optional)
- dueDate: Due date in ISO 8601 format (optional, e.g., 2024-12-31T23:59:59Z)

Rules:
- If a field cannot be determined, set it to null
- For priority and category, use exact enum values listed above
- For dueDate, parse relative dates like ""tomorrow"", ""next week"", ""in 3 days"" into absolute ISO dates
- Return ONLY valid JSON, no additional text

User input:
{input}

Return JSON in this exact format:
{{
  ""title"": ""string or null"",
  ""description"": ""string or null"",
  ""priority"": ""Critical|High|Medium|Low or null"",
  ""category"": ""Development|Design|Marketing|Operations|Research|Other or null"",
  ""assigneeEmail"": ""string or null"",
  ""dueDate"": ""ISO 8601 string or null""
}}";

        var response = await model.GenerateAsync(prompt);
        var jsonResponse = response.LastMessageContent ?? "{}";

        // Parse the JSON response into ParsedTaskResult
        var result = System.Text.Json.JsonSerializer.Deserialize<ParsedTaskResult>(
            jsonResponse,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            }) ?? new ParsedTaskResult();

        return result;
    }

    /// <summary>
    /// Logs AI interaction to the AiInteractionLog table for tracking and analytics.
    /// REQUIREMENT 8.6: Log all AI interactions including input summary, output summary, tokens used, and latency
    /// </summary>
    private async Task LogAiInteraction(
        Guid userId,
        string input,
        ParsedTaskResult? result,
        int? tokensUsed,
        int latencyMs)
    {
        try
        {
            var log = new AiInteractionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FeatureType = AiFeatureType.TaskCreation,
                InputSummary = input.Length > 1000 ? input.Substring(0, 1000) + "..." : input,
                OutputSummary = result != null
                    ? $"Title: {result.Title ?? "null"}, Priority: {result.Priority?.ToString() ?? "null"}, Category: {result.Category?.ToString() ?? "null"}"
                    : "Failed to parse",
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
