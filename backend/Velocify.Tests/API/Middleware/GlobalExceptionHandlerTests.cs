using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Text.Json;
using Velocify.API.Middleware;
using Velocify.Domain.Exceptions;
using Xunit;

namespace Velocify.Tests.API.Middleware;

/// <summary>
/// Unit tests for GlobalExceptionHandler middleware.
/// 
/// Test Coverage:
/// - Unhandled exceptions return 500 with ProblemDetails (Requirement 19.1)
/// - Domain exceptions map to appropriate HTTP status codes (Requirement 19.2)
/// - Validation exceptions return 400 with field-level errors (Requirement 19.3)
/// - Authorization exceptions return 403 without internal details (Requirement 19.4)
/// - Stack traces are never exposed to clients (Requirement 19.5)
/// - All exceptions are logged with correlation ID (Requirement 19.6)
/// 
/// Validates: Requirements 19.1-19.6, 30.1-30.7
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_mockLogger.Object);
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_Returns500WithProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong internally");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(500);
        problemDetails.Title.Should().Be("Internal Server Error");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
        problemDetails.Instance.Should().Be(context.TraceIdentifier);
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_DoesNotExposeStackTrace()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Internal error with sensitive details");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        
        // REQUIREMENT 19.5: Verify stack trace is not exposed
        problemDetails!.Detail.Should().NotContain("stack");
        problemDetails.Detail.Should().NotContain("InvalidOperationException");
        problemDetails.Detail.Should().NotContain("Internal error with sensitive details");
        problemDetails.Detail.Should().Be("An unexpected error occurred. Please contact support with the correlation ID.");
    }

    [Fact]
    public async Task TryHandleAsync_LogsExceptionWithCorrelationId()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "test-correlation-123";
        context.TraceIdentifier = correlationId;
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        // REQUIREMENT 19.6: Verify exception is logged with correlation ID
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(correlationId)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_LogsExceptionWithUserId_WhenAuthenticated()
    {
        // Arrange
        var context = CreateHttpContext();
        var userId = "user-123";
        var claims = new List<Claim>
        {
            new Claim("sub", userId)
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userId)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_LogsAnonymous_WhenNotAuthenticated()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Anonymous")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_BadRequestException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new BadRequestException("Invalid input data");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(400);
        problemDetails.Title.Should().Be("Bad Request");
        problemDetails.Detail.Should().Be("Invalid input data");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedException_Returns401()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new UnauthorizedException("Invalid credentials");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(401);
        problemDetails.Title.Should().Be("Unauthorized");
        problemDetails.Detail.Should().Be("Invalid credentials");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
    }

    [Fact]
    public async Task TryHandleAsync_ForbiddenException_Returns403WithoutInternalDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ForbiddenException("Access denied to resource");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(403);
        problemDetails.Title.Should().Be("Forbidden");
        problemDetails.Detail.Should().Be("Access denied to resource");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.3");
        
        // REQUIREMENT 19.4: Verify no internal details are exposed
        problemDetails.Detail.Should().NotContain("internal");
        problemDetails.Detail.Should().NotContain("database");
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Task", 123);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(404);
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Detail.Should().Contain("Task");
        problemDetails.Detail.Should().Contain("123");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_Returns409()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ConflictException("Concurrency conflict detected");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(409);
        problemDetails.Title.Should().Be("Conflict");
        problemDetails.Detail.Should().Be("Concurrency conflict detected");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.8");
    }

    [Fact]
    public async Task TryHandleAsync_ConflictExceptionWithCurrentValues_IncludesCurrentValuesInResponse()
    {
        // Arrange
        var context = CreateHttpContext();
        var currentValues = new { Title = "Updated Title", Status = "Completed" };
        var exception = new ConflictException("Concurrency conflict", currentValues);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("currentValues");
        
        var currentValuesJson = JsonSerializer.Serialize(problemDetails.Extensions["currentValues"]);
        currentValuesJson.Should().Contain("Updated Title");
        currentValuesJson.Should().Contain("Completed");
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400WithFieldErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required"),
            new ValidationFailure("Title", "Title must be at least 3 characters"),
            new ValidationFailure("DueDate", "DueDate must be in the future")
        };
        var exception = new ValidationException(validationFailures);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(400);
        problemDetails.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Detail.Should().Be("Please check the errors property for details.");
        
        // REQUIREMENT 19.3: Verify field-level error details are included
        problemDetails.Extensions.Should().ContainKey("errors");
        var errors = problemDetails.Extensions["errors"] as JsonElement?;
        errors.Should().NotBeNull();
        
        var errorsDict = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errors.ToString()!);
        errorsDict.Should().ContainKey("Title");
        errorsDict!["Title"].Should().HaveCount(2);
        errorsDict["Title"].Should().Contain("Title is required");
        errorsDict["Title"].Should().Contain("Title must be at least 3 characters");
        errorsDict.Should().ContainKey("DueDate");
        errorsDict["DueDate"].Should().HaveCount(1);
        errorsDict["DueDate"].Should().Contain("DueDate must be in the future");
    }

    [Fact]
    public async Task TryHandleAsync_IncludesCorrelationIdInProblemDetailsInstance()
    {
        // Arrange
        var context = CreateHttpContext();
        var correlationId = "correlation-xyz-789";
        context.TraceIdentifier = correlationId;
        var exception = new InvalidOperationException("Test");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails.Should().NotBeNull();
        problemDetails!.Instance.Should().Be(correlationId);
    }

    [Fact]
    public async Task TryHandleAsync_LogsRequestPathAndMethod()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Path = "/api/v1/tasks";
        context.Request.Method = "POST";
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("/api/v1/tasks") && 
                    v.ToString()!.Contains("POST")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_AlwaysReturnsTrue()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Any exception");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        // Handler should always return true to indicate exception was handled
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_SetsContentTypeToApplicationJson()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.ContentType.Should().Contain("application/json");
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = Guid.NewGuid().ToString();
        return context;
    }

    private static async Task<ProblemDetails?> DeserializeProblemDetails(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        if (string.IsNullOrEmpty(responseBody))
            return null;

        return JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
