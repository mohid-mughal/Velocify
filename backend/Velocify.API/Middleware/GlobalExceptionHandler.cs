using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Velocify.Domain.Exceptions;

namespace Velocify.API.Middleware;

/// <summary>
/// Global exception handler that implements IExceptionHandler to process all unhandled exceptions.
/// 
/// Responsibilities:
/// - Map domain exceptions to appropriate HTTP status codes
/// - Return standardized ProblemDetails responses
/// - Log exceptions with correlation ID for troubleshooting
/// - Never expose stack traces or internal implementation details to clients
/// 
/// Requirements: 19.1-19.6
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Extract correlation ID from HTTP context for tracing
        var correlationId = httpContext.TraceIdentifier;
        
        // Extract user ID from JWT claims if authenticated
        var userId = httpContext.User?.FindFirst("sub")?.Value 
                     ?? httpContext.User?.FindFirst("userId")?.Value;

        // Log exception with full details (stack trace only in logs, never exposed to client)
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, UserId: {UserId}, Path: {Path}, Method: {Method}",
            correlationId,
            userId ?? "Anonymous",
            httpContext.Request.Path,
            httpContext.Request.Method);

        // Map exception to ProblemDetails response
        var problemDetails = MapExceptionToProblemDetails(exception, correlationId);

        // Set response status code
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        // Write ProblemDetails as JSON response
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to indicate the exception was handled
        return true;
    }

    private ProblemDetails MapExceptionToProblemDetails(Exception exception, string correlationId)
    {
        return exception switch
        {
            // FluentValidation exceptions - 400 Bad Request with field-level errors
            ValidationException validationException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Please check the errors property for details.",
                Instance = correlationId,
                Extensions =
                {
                    ["errors"] = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray())
                }
            },

            // Domain exception: BadRequestException - 400 Bad Request
            BadRequestException badRequestException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = badRequestException.Message,
                Instance = correlationId
            },

            // Domain exception: UnauthorizedException - 401 Unauthorized
            UnauthorizedException unauthorizedException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = unauthorizedException.Message,
                Instance = correlationId
            },

            // Domain exception: ForbiddenException - 403 Forbidden
            ForbiddenException forbiddenException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = forbiddenException.Message,
                Instance = correlationId
            },

            // Domain exception: NotFoundException - 404 Not Found
            NotFoundException notFoundException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = notFoundException.Message,
                Instance = correlationId
            },

            // Domain exception: ConflictException - 409 Conflict (concurrency conflicts)
            ConflictException conflictException => CreateConflictProblemDetails(conflictException, correlationId),

            // All other exceptions - 500 Internal Server Error
            // CRITICAL: Never expose stack trace or internal details to client
            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred. Please contact support with the correlation ID.",
                Instance = correlationId
            }
        };
    }

    private static ProblemDetails CreateConflictProblemDetails(ConflictException conflictException, string correlationId)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Title = "Conflict",
            Status = StatusCodes.Status409Conflict,
            Detail = conflictException.Message,
            Instance = correlationId
        };

        if (conflictException.CurrentValues != null)
        {
            problemDetails.Extensions["currentValues"] = conflictException.CurrentValues;
        }

        return problemDetails;
    }
}
