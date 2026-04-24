namespace Velocify.Application.Interfaces;

/// <summary>
/// Service for AI-powered sentiment analysis of task comments.
/// Requirements: 14.1-14.5
/// </summary>
public interface ICommentSentimentService
{
    /// <summary>
    /// Analyzes the sentiment of a comment asynchronously (non-blocking).
    /// Returns a score between 0.0 (negative) and 1.0 (positive).
    /// Stores result in TaskComment.SentimentScore.
    /// </summary>
    /// <param name="content">Comment text to analyze</param>
    /// <returns>Sentiment score between 0.0 and 1.0</returns>
    Task<decimal> AnalyzeSentiment(string content);
}
