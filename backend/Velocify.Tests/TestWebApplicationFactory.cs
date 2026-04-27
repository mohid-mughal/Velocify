using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        // IMPORTANT: We need to OVERRIDE appsettings.json which contains placeholder values like "${CORS_ALLOWED_ORIGINS}"
        // By adding our configuration LAST, it takes precedence over appsettings.json
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Read CORS_ALLOWED_ORIGINS from environment variable if available
            var corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") 
                ?? "http://localhost:3000,http://localhost:5173";
            
            // Add test-specific configuration with HIGHEST priority
            // This will override the ${CORS_ALLOWED_ORIGINS} placeholder in appsettings.json
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "InMemory",
                ["CorsSettings:AllowedOrigins"] = corsOrigins,
                ["JwtSettings:SecretKey"] = "test-secret-key-for-integration-tests-minimum-32-characters",
                ["JwtSettings:Issuer"] = "test-issuer",
                ["JwtSettings:Audience"] = "test-audience",
                ["JwtSettings:AccessTokenExpiryMinutes"] = "60",
                ["JwtSettings:RefreshTokenExpiryDays"] = "7",
                ["OpenAI:ApiKey"] = "test-api-key",
                ["OpenAI:Model"] = "gpt-4",
                ["ASPNETCORE_ENVIRONMENT"] = "Test"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the SQL Server DbContext registration added by AddInfrastructure
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<VelocifyDbContext>));
            
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
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
