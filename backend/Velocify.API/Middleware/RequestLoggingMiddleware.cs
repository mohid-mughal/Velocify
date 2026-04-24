using System.Diagnostics;

namespace Velocify.API.Middleware;

/// <summary>
/// Middleware that logs HTTP request and response information for monitoring and troubleshooting.
/// 
/// Responsibilities:
/// - Log request method, path, and query string at Information level
/// - Log response status code and duration at Information level
/// - Include correlation ID (TraceIdentifier) for request tracing
/// - Include user ID from JWT claims when authenticated
/// - Exclude /health endpoint to reduce log noise
/// 
/// Requirements: 16.2, 16.5, 16.6
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // REQUIREMENT 16.6: Exclude /health endpoint from logging to reduce noise
        if (context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // REQUIREMENT 16.2: Extract correlation ID from HTTP context for tracing
        var correlationId = context.TraceIdentifier;

        // REQUIREMENT 16.2: Extract user ID from JWT claims if authenticated
        var userId = context.User?.FindFirst("sub")?.Value 
                     ?? context.User?.FindFirst("userId")?.Value 
                     ?? "Anonymous";

        // Start timing the request
        var stopwatch = Stopwatch.StartNew();

        // Log incoming request
        _logger.LogInformation(
            "HTTP {Method} {Path}{QueryString} started. CorrelationId: {CorrelationId}, UserId: {UserId}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            correlationId,
            userId);

        try
        {
            // Call the next middleware in the pipeline
            await _next(context);
        }
        finally
        {
            // Stop timing
            stopwatch.Stop();

            // Log response with duration
            _logger.LogInformation(
                "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {Duration}ms. CorrelationId: {CorrelationId}, UserId: {UserId}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                userId);
        }
    }
}
