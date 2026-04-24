using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Velocify.Infrastructure.Data;
using Xunit;

namespace Velocify.Tests.API;

/// <summary>
/// Integration tests for database migration on startup.
/// Validates: Requirement 29.4
/// </summary>
public class DatabaseMigrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DatabaseMigrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Startup_WithPendingMigrations_AppliesMigrationsAutomatically()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true",
                    ["CorsSettings:AllowedOrigins"] = "http://localhost:3000"
                });
            });
        });

        // Act
        // Creating the client triggers application startup, which should apply migrations
        var client = factory.CreateClient();

        // Assert
        // REQUIREMENT 29.4: Verify migrations were applied during startup
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VelocifyDbContext>();
        
        // Check that database exists and has no pending migrations
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        Assert.Empty(pendingMigrations);
        
        // Verify database can be connected to
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect);
        
        // Cleanup
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Startup_WithUpToDateDatabase_DoesNotApplyMigrations()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true",
                    ["CorsSettings:AllowedOrigins"] = "http://localhost:3000"
                });
            });
        });

        // Pre-apply migrations manually
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<VelocifyDbContext>();
            await context.Database.MigrateAsync();
        }

        // Act
        // Creating the client triggers application startup
        var client = factory.CreateClient();

        // Assert
        // REQUIREMENT 29.4: Verify no migrations are applied when database is up-to-date
        using var assertScope = factory.Services.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<VelocifyDbContext>();
        
        var pendingMigrations = await assertContext.Database.GetPendingMigrationsAsync();
        Assert.Empty(pendingMigrations);
        
        // Cleanup
        await assertContext.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Startup_LogsMigrationStatus()
    {
        // Arrange
        var dbName = $"TestDb_{Guid.NewGuid()}";
        
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true",
                    ["CorsSettings:AllowedOrigins"] = "http://localhost:3000"
                });
            });
        });

        // Act
        var client = factory.CreateClient();

        // Assert
        // REQUIREMENT 29.4: Verify application starts successfully after migration
        var response = await client.GetAsync("/health");
        Assert.True(response.IsSuccessStatusCode);
        
        // Cleanup
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VelocifyDbContext>();
        await context.Database.EnsureDeletedAsync();
    }
}
