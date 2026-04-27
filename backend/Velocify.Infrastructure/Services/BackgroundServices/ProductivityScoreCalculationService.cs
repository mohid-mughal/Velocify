using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Velocify.Infrastructure.Data;

namespace Velocify.Infrastructure.Services.BackgroundServices;

/// <summary>
/// Background service that recalculates productivity scores for all active users.
/// Runs every 6 hours and calls the stored procedure usp_RecalculateUserProductivityScores.
/// Requirements: 7.6, 15.10
/// </summary>
public class ProductivityScoreCalculationService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductivityScoreCalculationService> _logger;
    private Timer? _timer;

    // REQUIREMENT 7.6: "THE Backend SHALL calculate productivity score using the stored procedure usp_RecalculateUserProductivityScores every 6 hours"
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromHours(6);

    public ProductivityScoreCalculationService(
        IServiceProvider serviceProvider,
        ILogger<ProductivityScoreCalculationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Starts the hosted service and schedules productivity score recalculation.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Productivity Score Calculation Service is starting. Scheduled to run every {IntervalHours} hours",
            ExecutionInterval.TotalHours);

        // Start the timer to run immediately and then every 6 hours
        _timer = new Timer(
            callback: async _ => await ExecuteProductivityScoreCalculation(),
            state: null,
            dueTime: TimeSpan.Zero, // Run immediately on startup
            period: ExecutionInterval); // Repeat every 6 hours

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the hosted service.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Productivity Score Calculation Service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the productivity score recalculation for all users.
    /// Calls the stored procedure usp_RecalculateUserProductivityScores.
    /// </summary>
    private async Task ExecuteProductivityScoreCalculation()
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting productivity score recalculation for all users");

        try
        {
            // Create a new scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<VelocifyDbContext>();

            // Guard: InMemory database doesn't support raw SQL (used in tests)
            if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                _logger.LogInformation("Skipping productivity score calculation - using InMemory database provider (test environment)");
                return;
            }

            // REQUIREMENT 7.6: Call stored procedure usp_RecalculateUserProductivityScores
            // This stored procedure calculates productivity score as:
            // (Sum of weighted completed-on-time tasks) / (Total assigned tasks)
            // Priority weights: Critical=4.0, High=3.0, Medium=2.0, Low=1.0
            var affectedRows = await context.Database.ExecuteSqlRawAsync(
                "EXEC usp_RecalculateUserProductivityScores");

            stopwatch.Stop();

            _logger.LogInformation(
                "Productivity score recalculation completed successfully. Affected rows: {AffectedRows}, Duration: {ElapsedMs}ms",
                affectedRows,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Failed to recalculate productivity scores. Duration: {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
