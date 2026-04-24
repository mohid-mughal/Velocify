using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Velocify.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that measures handler execution time and logs warnings for slow operations.
/// 
/// PIPELINE POSITION: This behavior MUST execute THIRD in the pipeline.
/// 
/// Order importance:
/// 1. ValidationBehavior - Validates input before any processing
/// 2. LoggingBehavior - Logs only valid requests
/// 3. PerformanceBehavior (this) - Measures only handler execution time
/// 
/// Rationale: Performance measurement should occur after validation and logging to ensure
/// we only measure the actual handler execution time without including validation overhead
/// or logging overhead. This provides accurate metrics for handler performance optimization.
/// By executing last in the pipeline, we measure only the business logic execution time,
/// which is the most relevant metric for identifying performance bottlenecks in handlers.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int PerformanceThresholdMs = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Start measuring execution time
        var stopwatch = Stopwatch.StartNew();

        // Execute the handler
        var response = await next();

        // Stop measuring
        stopwatch.Stop();

        // Log warning if execution time exceeds threshold
        if (stopwatch.ElapsedMilliseconds > PerformanceThresholdMs)
        {
            _logger.LogWarning(
                "Long Running Request: {RequestName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMs}ms)",
                requestName,
                stopwatch.ElapsedMilliseconds,
                PerformanceThresholdMs);
        }

        return response;
    }
}
