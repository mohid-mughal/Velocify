using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace Velocify.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs requests and responses using Serilog.
/// 
/// PIPELINE POSITION: This behavior MUST execute SECOND in the pipeline.
/// 
/// Order importance:
/// 1. ValidationBehavior - Validates input before any processing
/// 2. LoggingBehavior (this) - Logs only valid requests
/// 3. PerformanceBehavior - Measures only valid request execution time
/// 
/// Rationale: Logging should occur after validation passes to avoid cluttering
/// logs with invalid requests. This ensures we only log requests that have
/// passed validation and will be processed by the handler. Logging before
/// performance measurement allows us to capture the full request/response
/// context without including logging overhead in performance metrics.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Extract correlation ID from HTTP context (set by CorrelationIdMiddleware)
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? "N/A";
        
        // Extract user ID from JWT claims
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

        // Log the incoming request with correlation ID and user ID
        _logger.LogInformation(
            "Handling {RequestName} | CorrelationId: {CorrelationId} | UserId: {UserId} | Request: {Request}",
            requestName,
            correlationId,
            userId,
            JsonSerializer.Serialize(request));

        // Execute the handler
        var response = await next();

        // Log the response with correlation ID and user ID
        _logger.LogInformation(
            "Handled {RequestName} | CorrelationId: {CorrelationId} | UserId: {UserId} | Response: {Response}",
            requestName,
            correlationId,
            userId,
            JsonSerializer.Serialize(response));

        return response;
    }
}
