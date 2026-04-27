using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Velocify.Infrastructure.Data;

namespace Velocify.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures the application to use InMemory database for CI/CD compatibility.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "InMemory",
                ["CorsSettings:AllowedOrigins"] = "http://localhost:3000",
                ["Jwt:SecretKey"] = "test-secret-key-for-integration-tests-minimum-32-characters",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:ExpiryMinutes"] = "60",
                ["OpenAI:ApiKey"] = "test-api-key",
                ["OpenAI:Model"] = "gpt-4",
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<VelocifyDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database for testing
            services.AddDbContext<VelocifyDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
                options.EnableSensitiveDataLogging();
            });
        });
    }
}
