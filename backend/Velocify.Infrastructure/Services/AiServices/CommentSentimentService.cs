using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Velocify.Application.Interfaces;
using Velocify.Infrastructure.Data;

namespace Velocify.Infrastructure.Services.AiServices;

/// <summary>
/// Service for AI-powered sentiment analysis of task comments using LangChain.NET.
/// Implements retry policy with Polly for resilience against transient AI service failures.
/// Requirements: 14.1-14.5
/// </summary>
public class CommentSentimentService : ICommentSentimentService
{
    private readonly VelocifyDbContext _context;
    private readonly ILogger<CommentSentimentService> _logger;
    private readonly IConfiguration _configuration;
    private readonly AsyncRetryPolicy _retryPolicy;

    public CommentSentimentService(
        VelocifyDbContext context,
        ILogger<CommentSentimentService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;

        // RETRY POLICY EXPLANATION:
        // AI services can experience transient failures due to rate limiting, network issues, or temporary service degradation.
        // Without retries, sentiment analysis would fail for requests that could succeed on a second attempt.
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
                        "Sentiment analysis failed on attempt {RetryCount}. Retrying after {RetryDelay}ms. Error: {ErrorMessage}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Analyzes the sentiment of a comment asynchronously (non-blocking).
    /// REQUIREMENT 14.1: Asynchronously analyze sentiment using LangChain
    /// REQUIREMENT 14.2: Store a score between 0.0 and 1.0 in the SentimentScore column
    /// REQUIREMENT 14.5: Treat sentiment analysis as a non-blocking operation
    /// </summary>
    /// <param name="content">Comment text to analyze</param>
    /// <returns>Sentiment score between 0.0 (negative) and 1.0 (positive)</returns>
    public async Task<decimal> AnalyzeSentiment(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("Attempted to analyze sentiment of empty or whitespace content");
            return 0.5m; // Neutral score for empty content
        }

        try
        {
            _logger.LogInformation("Starting sentiment analysis for comment content (length: {Length})", content.Length);

            // Execute AI sentiment analysis with retry policy
            var sentimentScore = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await AnalyzeWithLangChain(content);
            });

            _logger.LogInformation(
                "Successfully analyzed sentiment. Score: {Score}",
                sentimentScore);

            return sentimentScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to analyze sentiment after all retry attempts for content: {ContentPreview}",
                content.Length > 50 ? content.Substring(0, 50) + "..." : content);

            // Return neutral score on failure to not block comment creation
            // REQUIREMENT 14.5: Non-blocking operation
            return 0.5m;
        }
    }

    /// <summary>
    /// Performs the actual AI sentiment analysis using LangChain.
    /// Uses OpenAI GPT model to analyze the emotional tone of the comment.
    /// REQUIREMENT 14.2: Return score between 0.0 (negative) and 1.0 (positive)
    /// </summary>
    private async Task<decimal> AnalyzeWithLangChain(string content)
    {
        // Get OpenAI API key from configuration
        var apiKey = _configuration["OpenAI:ApiKey"] 
            ?? throw new InvalidOperationException("OpenAI API key not configured");

        // Initialize OpenAI provider and chat model
        // Using gpt-3.5-turbo for fast, cost-effective sentiment analysis
        var provider = new OpenAiProvider(apiKey);
        var model = new OpenAiChatModel(provider, id: "gpt-3.5-turbo");

        // LANGCHAIN SENTIMENT ANALYSIS PROMPT:
        // We use a carefully crafted prompt to instruct the model to analyze sentiment
        // and return a numeric score between 0.0 and 1.0
        //
        // REQUIREMENT 14.2: Score between 0.0 (negative) and 1.0 (positive)
        var prompt = $@"You are a sentiment analysis assistant. Analyze the sentiment of the following comment and return ONLY a numeric score.

Comment:
{content}

Instructions:
- Return a sentiment score between 0.0 and 1.0
- 0.0 = Very negative (angry, frustrated, disappointed)
- 0.3 = Somewhat negative (concerned, worried, critical)
- 0.5 = Neutral (factual, informational, balanced)
- 0.7 = Somewhat positive (satisfied, pleased, optimistic)
- 1.0 = Very positive (excited, enthusiastic, delighted)
- Consider the overall emotional tone, word choice, and context
- Return ONLY the numeric score (e.g., 0.75), no additional text or explanation

Score:";

        var response = await model.GenerateAsync(prompt);
        var scoreText = response.LastMessageContent?.Trim() ?? "0.5";

        // Parse the response to extract the sentiment score
        if (decimal.TryParse(scoreText, out var score))
        {
            // Clamp the score between 0.0 and 1.0 to ensure it's within valid range
            score = Math.Max(0.0m, Math.Min(1.0m, score));
            return score;
        }

        _logger.LogWarning(
            "Failed to parse sentiment score from AI response: {Response}. Returning neutral score.",
            scoreText);

        // Return neutral score if parsing fails
        return 0.5m;
    }
}
