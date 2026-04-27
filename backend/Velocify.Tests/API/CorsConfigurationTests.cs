using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;

namespace Velocify.Tests.API;

/// <summary>
/// Integration tests for CORS configuration.
/// Validates: Requirement 29.5
/// </summary>
public class CorsConfigurationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CorsConfigurationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PreflightRequest_WithAllowedOrigin_ReturnsSuccessWithCorsHeaders()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CorsSettings:AllowedOrigins"] = "http://localhost:3000"
                });
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/health");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // REQUIREMENT 29.5: Verify CORS preflight request succeeds
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent);
        
        // Verify CORS headers are present
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://localhost:3000", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task Request_WithAllowedOrigin_IncludesCorsHeaders()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CorsSettings:AllowedOrigins"] = "http://localhost:3000;http://localhost:5173"
                });
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:5173");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // REQUIREMENT 29.5: Verify CORS headers are included in response
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task Request_WithMultipleAllowedOrigins_AcceptsAllConfiguredOrigins()
    {
        // Arrange
        // Use the same origins configured in CI pipeline (CorsSettings__AllowedOrigins)
        var allowedOrigins = new[] { "http://localhost:3000", "http://localhost:5173" };
        
        // Create client once outside the loop to avoid Serilog conflicts
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CorsSettings:AllowedOrigins"] = string.Join(";", allowedOrigins)
                });
            });
        }).CreateClient();
        
        foreach (var origin in allowedOrigins)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/health");
            request.Headers.Add("Origin", origin);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            // REQUIREMENT 29.5: Verify each configured origin is accepted
            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"), 
                $"Expected Access-Control-Allow-Origin header for origin: {origin}");
            Assert.Equal(origin, response.Headers.GetValues("Access-Control-Allow-Origin").First());
        }
    }

    [Fact]
    public async Task Request_WithCommaSeparatedOrigins_ParsesCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CorsSettings:AllowedOrigins"] = "http://localhost:3000,http://localhost:5173"
                });
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:3000");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // REQUIREMENT 29.5: Verify comma-separated origins are parsed correctly
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://localhost:3000", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task Request_WithOriginsContainingWhitespace_TrimsCorrectly()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CorsSettings:AllowedOrigins"] = " http://localhost:3000 ; http://localhost:5173 "
                });
            });
        }).CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:3000");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // REQUIREMENT 29.5: Verify whitespace is trimmed from origins
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://localhost:3000", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }
}
