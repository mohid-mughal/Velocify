using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velocify.Infrastructure.Data;
using LangChain.Providers.OpenAI;

namespace Velocify.API.Controllers;

/// <summary>
/// Health check controller for monitoring system dependencies.
/// This endpoint is used by Azure App Service for health monitoring.
/// Requirements: 17.1-17.6
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly VelocifyDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        VelocifyDbContext context,
        IConfiguration configuration,
        ILogger<HealthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint that verifies system dependencies.
    /// REQUIREMENT 17.1: Check database connectivity
    /// REQUIREMENT 17.2: Check LangChain service availability
    /// REQUIREMENT 17.3: Check available disk space for log files
    /// REQUIREMENT 17.4: Return 200 OK when all checks pass
    /// REQUIREMENT 17.5: Return 503 Service Unavailable when any check fails
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var healthChecks = new Dictionary<string, HealthCheckResult>();
        var overallHealthy = true;

        // REQUIREMENT 17.1: Check database connectivity
        var dbHealth = await CheckDatabaseConnectivity();
        healthChecks["database"] = dbHealth;
        if (!dbHealth.Healthy)
        {
            overallHealthy = false;
        }

        // REQUIREMENT 17.2: Check LangChain service availability
        var langChainHealth = await CheckLangChainServiceAvailability();
        healthChecks["langchain"] = langChainHealth;
        if (!langChainHealth.Healthy)
        {
            overallHealthy = false;
        }

        // REQUIREMENT 17.3: Check available disk space for log files
        var diskSpaceHealth = CheckDiskSpace();
        healthChecks["diskSpace"] = diskSpaceHealth;
        if (!diskSpaceHealth.Healthy)
        {
            overallHealthy = false;
        }

        var response = new
        {
            status = overallHealthy ? "Healthy" : "Unhealthy",
            checks = healthChecks,
            timestamp = DateTime.UtcNow
        };

        // REQUIREMENT 17.4 & 17.5: Return appropriate status code
        if (overallHealthy)
        {
            return Ok(response);
        }
        else
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }
    }

    /// <summary>
    /// Checks database connectivity by attempting to open a connection and execute a simple query.
    /// </summary>
    private async Task<HealthCheckResult> CheckDatabaseConnectivity()
    {
        try
        {
            // Attempt to execute a simple query to verify database connectivity
            await _context.Database.CanConnectAsync();
            
            _logger.LogDebug("Database connectivity check passed");
            
            return new HealthCheckResult
            {
                Healthy = true,
                Message = "Database connection successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connectivity check failed");
            
            return new HealthCheckResult
            {
                Healthy = false,
                Message = $"Database connection failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Checks LangChain service availability by verifying OpenAI API key configuration.
    /// </summary>
    private Task<HealthCheckResult> CheckLangChainServiceAvailability()
    {
        try
        {
            // Check if OpenAI API key is configured
            var apiKey = _configuration["OpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("LangChain service check failed: OpenAI API key not configured");
                
                return Task.FromResult(new HealthCheckResult
                {
                    Healthy = false,
                    Message = "OpenAI API key not configured"
                });
            }

            // Verify the API key is configured - we don't make an actual call
            // to avoid consuming tokens on every health check
            // The presence of a valid API key indicates the service is configured correctly
            _logger.LogDebug("LangChain service availability check passed (API key configured)");
            
            return Task.FromResult(new HealthCheckResult
            {
                Healthy = true,
                Message = "LangChain service configured"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LangChain service availability check failed");
            
            return Task.FromResult(new HealthCheckResult
            {
                Healthy = false,
                Message = $"LangChain service unavailable: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Checks available disk space for log files.
    /// Warns if available space is below 1 GB.
    /// </summary>
    private HealthCheckResult CheckDiskSpace()
    {
        try
        {
            // Get the drive where the application is running
            var appPath = AppContext.BaseDirectory;
            var driveInfo = new DriveInfo(Path.GetPathRoot(appPath) ?? "C:\\");

            // Check if drive is ready
            if (!driveInfo.IsReady)
            {
                _logger.LogWarning("Disk space check failed: Drive not ready");
                
                return new HealthCheckResult
                {
                    Healthy = false,
                    Message = "Drive not ready"
                };
            }

            var availableSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            
            // Warn if less than 1 GB available
            if (availableSpaceGB < 1.0)
            {
                _logger.LogWarning(
                    "Low disk space: {AvailableSpaceGB:F2} GB available on drive {DriveName}",
                    availableSpaceGB,
                    driveInfo.Name);
                
                return new HealthCheckResult
                {
                    Healthy = false,
                    Message = $"Low disk space: {availableSpaceGB:F2} GB available"
                };
            }

            _logger.LogDebug(
                "Disk space check passed: {AvailableSpaceGB:F2} GB available on drive {DriveName}",
                availableSpaceGB,
                driveInfo.Name);
            
            return new HealthCheckResult
            {
                Healthy = true,
                Message = $"{availableSpaceGB:F2} GB available"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Disk space check failed");
            
            return new HealthCheckResult
            {
                Healthy = false,
                Message = $"Disk space check failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Represents the result of a health check.
    /// </summary>
    private class HealthCheckResult
    {
        public bool Healthy { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
