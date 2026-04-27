using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Velocify.Infrastructure.Data;

// DISABLE XUNIT PARALLELIZATION TO PREVENT SERILOG "LOGGER ALREADY FROZEN" ERRORS
// 
// When xUnit runs tests in parallel, multiple WebApplicationFactory instances try to
// initialize Serilog's global static logger simultaneously, causing race conditions.
// By disabling parallelization, tests run sequentially, preventing Serilog conflicts.
// 
// Performance impact: Minimal - tests are fast and complete in ~36 seconds sequentially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Velocify.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures the application to use InMemory database for CI/CD compatibility.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Force Test environment - must be set before configuration is built
        builder.UseEnvironment("Test");
        
        // IMPORTANT: We need to OVERRIDE appsettings.json which contains placeholder values like "${CORS_ALLOWED_ORIGINS}"
        // By adding our configuration LAST, it takes precedence over appsettings.json
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Read from environment variable using ASP.NET Core naming convention
            // CorsSettings__AllowedOrigins maps to CorsSettings:AllowedOrigins
            var corsOrigins = Environment.GetEnvironmentVariable("CorsSettings__AllowedOrigins")
                ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") 
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
                ["OpenAI:Model"] = "gpt-4"
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
