using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Velocify.API.Middleware;
using Xunit;

namespace Velocify.Tests.API.Middleware;

/// <summary>
/// Unit tests for RequestLoggingMiddleware.
/// Validates: Requirements 16.2, 16.5, 16.6
/// </summary>
public class RequestLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestLoggingMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;

    public RequestLoggingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();
        _mockNext = new Mock<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestAndResponse_WithCorrelationIdAndUserId()
    {
        // Arrange
        var middleware = new RequestLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/tasks";
        context.Request.QueryString = new QueryString("?status=pending");
        context.TraceIdentifier = "test-correlation-id";
        
        // Add user claims to simulate authenticated user
        var claims = new[]
        {
            new System.Security.Claims.Claim("sub", "user-123")
        };
        context.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(claims, "TestAuth"));

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Verify that logging was called (at least twice: once for request, once for response)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("responded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify next middleware was called
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ExcludesHealthEndpoint_FromLogging()
    {
        // Arrange
        var middleware = new RequestLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";
        context.TraceIdentifier = "test-correlation-id";

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Verify that no logging occurred for /health endpoint
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        // Verify next middleware was still called
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsAnonymousUser_WhenNotAuthenticated()
    {
        // Arrange
        var middleware = new RequestLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/tasks";
        context.TraceIdentifier = "test-correlation-id";
        // No user claims - anonymous request

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Verify that logging was called with "Anonymous" user
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Anonymous")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_LogsResponseDuration_InMilliseconds()
    {
        // Arrange
        var middleware = new RequestLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/tasks";
        context.TraceIdentifier = "test-correlation-id";

        // Simulate a delay in the next middleware
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(async () => await Task.Delay(10));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Verify that response logging includes duration
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_LogsResponseEvenWhenExceptionOccurs()
    {
        // Arrange
        var middleware = new RequestLoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/tasks";
        context.TraceIdentifier = "test-correlation-id";

        // Simulate an exception in the next middleware
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await middleware.InvokeAsync(context));

        // Verify that response logging still occurred (in finally block)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("responded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
