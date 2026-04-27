using Microsoft.AspNetCore.Http;
using Moq;
using Velocify.API.Middleware;
using Xunit;

namespace Velocify.Tests.API.Middleware;

/// <summary>
/// Unit tests for CorrelationIdMiddleware.
/// Validates: Requirements 16.2, 16.3
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_ExtractsCorrelationId_FromRequestHeader()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(_mockNext.Object);
        var context = new DefaultHttpContext();
        var expectedCorrelationId = "client-provided-correlation-id";
        context.Request.Headers.Append(CorrelationIdHeaderName, expectedCorrelationId);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // REQUIREMENT 16.2: Verify correlation ID is set as TraceIdentifier
        Assert.Equal(expectedCorrelationId, context.TraceIdentifier);

        // Verify next middleware was called
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_GeneratesNewCorrelationId_WhenNotProvidedInRequest()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(_mockNext.Object);
        var context = new DefaultHttpContext();
        // No correlation ID header provided

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // REQUIREMENT 16.2: Verify a correlation ID was generated and set as TraceIdentifier
        Assert.NotNull(context.TraceIdentifier);
        Assert.NotEmpty(context.TraceIdentifier);
        
        // Verify it's a valid GUID format
        Assert.True(Guid.TryParse(context.TraceIdentifier, out _));

        // Verify next middleware was called
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_IncludesCorrelationId_InResponseHeader()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(_mockNext.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Add a writable stream
        var expectedCorrelationId = "test-correlation-id";
        context.Request.Headers.Append(CorrelationIdHeaderName, expectedCorrelationId);

        // Simulate response being written by writing to the body
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(async (HttpContext ctx) =>
            {
                // Write to response body to trigger OnStarting callbacks
                await ctx.Response.WriteAsync("test");
            });

        // Act
        await middleware.InvokeAsync(context);
        
        // Manually trigger OnStarting callbacks by starting the response
        await context.Response.StartAsync();

        // Assert
        // REQUIREMENT 16.3: Verify correlation ID is included in response header
        Assert.True(context.Response.Headers.ContainsKey(CorrelationIdHeaderName));
        Assert.Equal(expectedCorrelationId, context.Response.Headers[CorrelationIdHeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_GeneratesCorrelationId_WhenHeaderIsEmpty()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(_mockNext.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers.Append(CorrelationIdHeaderName, string.Empty);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Verify a new correlation ID was generated (not using the empty string)
        Assert.NotNull(context.TraceIdentifier);
        Assert.NotEmpty(context.TraceIdentifier);
        Assert.True(Guid.TryParse(context.TraceIdentifier, out _));
    }

    [Fact]
    public async Task InvokeAsync_GeneratesCorrelationId_WhenHeaderIsWhitespace()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(_mockNext.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers.Append(CorrelationIdHeaderName, "   ");

        _mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Verify a new correlation ID was generated (not using the whitespace)
        Assert.NotNull(context.TraceIdentifier);
        Assert.NotEmpty(context.TraceIdentifier);
        Assert.True(Guid.TryParse(context.TraceIdentifier, out _));
    }

    [Fact]
    public async Task InvokeAsync_PreservesExistingResponseHeader_IfAlreadySet()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(_mockNext.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Add a writable stream
        var requestCorrelationId = "request-correlation-id";
        var existingResponseCorrelationId = "existing-response-correlation-id";
        
        context.Request.Headers.Append(CorrelationIdHeaderName, requestCorrelationId);
        context.Response.Headers.Append(CorrelationIdHeaderName, existingResponseCorrelationId);

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(async (HttpContext ctx) =>
            {
                // Write to response body
                await ctx.Response.WriteAsync("test");
            });

        // Act
        await middleware.InvokeAsync(context);
        
        // Manually trigger OnStarting callbacks by starting the response
        await context.Response.StartAsync();

        // Assert
        // Verify the existing response header was not overwritten
        Assert.Equal(existingResponseCorrelationId, context.Response.Headers[CorrelationIdHeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_SetsTraceIdentifier_BeforeCallingNextMiddleware()
    {
        // Arrange
        var middleware = new CorrelationIdMiddleware(_mockNext.Object);
        var context = new DefaultHttpContext();
        var expectedCorrelationId = "test-correlation-id";
        context.Request.Headers.Append(CorrelationIdHeaderName, expectedCorrelationId);

        string? capturedTraceIdentifier = null;
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns((HttpContext ctx) =>
            {
                // Capture the TraceIdentifier at the time next middleware is called
                capturedTraceIdentifier = ctx.TraceIdentifier;
                return Task.CompletedTask;
            });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Verify TraceIdentifier was set before next middleware was called
        Assert.Equal(expectedCorrelationId, capturedTraceIdentifier);
    }
}
