namespace Velocify.API.Middleware;

/// <summary>
/// Middleware that manages correlation IDs for distributed tracing and troubleshooting.
/// 
/// Responsibilities:
/// - Extract correlation ID from X-Correlation-ID request header if present
/// - Generate a new correlation ID (GUID) if not provided by client
/// - Set the correlation ID as HttpContext.TraceIdentifier for use by other middleware and logging
/// - Include correlation ID in X-Correlation-ID response header for client-side tracing
/// 
/// This middleware ensures every request has a unique identifier that can be used to trace
/// the request through logs, exception handlers, and other middleware components.
/// 
/// Requirements: 16.2, 16.3
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // REQUIREMENT 16.2: Extract correlation ID from request header or generate new one
        var correlationId = GetOrGenerateCorrelationId(context);

        // REQUIREMENT 16.2: Add correlation ID to HTTP context as TraceIdentifier
        // This makes it available to all subsequent middleware, controllers, and logging
        context.TraceIdentifier = correlationId;

        // REQUIREMENT 16.3: Include correlation ID in response header
        // IMPORTANT: Set the header BEFORE calling next middleware to ensure it's written
        // Setting it after await _next() would be too late as response may have started
        if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
        {
            context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Try to extract correlation ID from request header
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Generate new correlation ID if not provided
        return Guid.NewGuid().ToString();
    }
}
