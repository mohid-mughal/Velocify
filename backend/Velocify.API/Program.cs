using Velocify.Infrastructure;
using Velocify.API.Middleware;
using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;

// SERILOG TWO-STAGE INITIALIZATION
// 
// Stage 1: Bootstrap Logger
// Create a minimal logger that can capture startup errors before the full configuration is loaded.
// This ensures we can log any configuration errors or startup failures that occur before
// the application is fully initialized. Without this, startup errors would be lost.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting Velocify API");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Stage 2: Full Logger Configuration
    // Replace the bootstrap logger with the full configuration from appsettings.json.
    // This includes all enrichers (MachineName, Environment, CorrelationId, UserId),
    // sinks (Console with JSON format, rolling file), and configuration from appsettings.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProperty("Application", "Velocify.API")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Information)
        .WriteTo.File(
            path: "logs/velocify-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Information));

    // Add services to the container.
    builder.Services.AddInfrastructure(builder.Configuration);

    // JWT AUTHENTICATION CONFIGURATION
    // 
    // Token Configuration:
    // - Access Token: 15-minute expiration (short-lived for security)
    // - Refresh Token: 7-day expiration (long-lived, stored as SHA-256 hash in database)
    // 
    // Token Flow:
    // 1. User logs in → receives access token (JWT) and refresh token
    // 2. Access token included in Authorization header for API requests
    // 3. When access token expires → client calls /auth/refresh with refresh token
    // 4. Backend validates refresh token hash → issues new access token
    // 5. Old refresh token is invalidated (rotation) to prevent reuse
    // 
    // Security:
    // - Access tokens are stateless (JWT) and validated using secret key
    // - Refresh tokens are stored as SHA-256 hashes (never plain text)
    // - Token rotation prevents replay attacks
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
            ClockSkew = TimeSpan.Zero // Remove default 5-minute clock skew for precise expiration
        };

        // Configure JWT authentication for SignalR
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                // If the request is for SignalR hub, read token from query string
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

    // ROLE-BASED AUTHORIZATION CONFIGURATION
    // 
    // Three roles with hierarchical permissions:
    // - SuperAdmin: Full system access, can manage all users and tasks
    // - Admin: Team management, can view team tasks and manage team members
    // - Member: Basic access, can only view and manage their own tasks
    // 
    // Authorization is enforced at the controller level using [Authorize(Roles = "...")] attributes
    // and in business logic using User.IsInRole() checks.
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
        options.AddPolicy("AdminOrAbove", policy => policy.RequireRole("SuperAdmin", "Admin"));
        options.AddPolicy("AllRoles", policy => policy.RequireRole("SuperAdmin", "Admin", "Member"));
    });

    // MEDIATR CONFIGURATION WITH PIPELINE BEHAVIORS
    // 
    // Pipeline execution order (CRITICAL - order matters):
    // 1. ValidationBehavior: Validates request using FluentValidation rules
    //    - Throws ValidationException if validation fails
    //    - Prevents invalid data from reaching handlers
    // 
    // 2. LoggingBehavior: Logs request and response with Serilog
    //    - Includes correlation ID and user ID for traceability
    //    - Only logs requests that passed validation
    // 
    // 3. PerformanceBehavior: Measures handler execution time
    //    - Logs warning if handler exceeds 500ms threshold
    //    - Measures only handler time (excludes validation and logging overhead)
    // 
    // Why order matters:
    // - Validation must run first to reject invalid requests early
    // - Logging runs after validation to avoid cluttering logs with invalid requests
    // - Performance measurement runs last to measure only business logic execution
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Velocify.Application.AssemblyReference).Assembly);
        
        // Register pipeline behaviors in execution order
        cfg.AddOpenBehavior(typeof(Velocify.Application.Behaviors.ValidationBehavior<,>));
        cfg.AddOpenBehavior(typeof(Velocify.Application.Behaviors.LoggingBehavior<,>));
        cfg.AddOpenBehavior(typeof(Velocify.Application.Behaviors.PerformanceBehavior<,>));
    });

    // Register HTTP context accessor for accessing user claims in behaviors
    builder.Services.AddHttpContextAccessor();

    // Register SignalR
    builder.Services.AddSignalR();

    // Register TaskHubService
    builder.Services.AddScoped<Velocify.Application.Interfaces.ITaskHubService, Velocify.API.Services.TaskHubService>();

    // Register Global Exception Handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // API VERSIONING CONFIGURATION
    // 
    // Versioning Strategy:
    // - URL Path versioning: /api/v1/tasks, /api/v2/tasks
    // - Default version: v1.0 (used when client omits version)
    // - Version reporting: Includes supported versions in response headers
    // 
    // Configuration:
    // - DefaultApiVersion: Specifies the default API version (1.0)
    // - AssumeDefaultVersionWhenUnspecified: Uses default version when client doesn't specify
    // - ReportApiVersions: Adds api-supported-versions header to responses
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    // HEALTH CHECKS CONFIGURATION
    // 
    // Azure App Service uses the /health endpoint for health monitoring.
    // The HealthController implements custom health checks for:
    // - Database connectivity (Requirement 17.1)
    // - LangChain service availability (Requirement 17.2)
    // - Disk space for log files (Requirement 17.3)
    // 
    // Returns:
    // - 200 OK when all checks pass (Requirement 17.4)
    // - 503 Service Unavailable when any check fails (Requirement 17.5)
    builder.Services.AddHealthChecks();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // CORS CONFIGURATION
    // 
    // Reads allowed origins from environment variable for flexibility across environments.
    // In development: typically http://localhost:3000, http://localhost:5173
    // In production: the deployed frontend URL (e.g., https://your-app.vercel.app)
    // 
    // Configuration:
    // - AllowedOrigins: Semicolon or comma-separated list of allowed origins
    // - AllowCredentials: Required for JWT authentication and SignalR
    // - AllowAnyHeader: Permits all headers (Authorization, Content-Type, etc.)
    // - AllowAnyMethod: Permits all HTTP methods (GET, POST, PUT, DELETE, PATCH)
    // 
    // Security:
    // - Never use AllowAnyOrigin in production (security risk)
    // - Always specify exact origins from configuration
    // - AllowCredentials requires specific origins (cannot be used with wildcard)
    var corsOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Value;
    if (!string.IsNullOrWhiteSpace(corsOrigins))
    {
        var origins = corsOrigins.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(o => o.Trim())
                                 .ToArray();
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(origins)
                      .AllowCredentials()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
        
        Log.Information("CORS configured with allowed origins: {Origins}", string.Join(", ", origins));
    }
    else
    {
        Log.Warning("CORS not configured - CorsSettings:AllowedOrigins is empty");
    }

    var app = builder.Build();

    // DATABASE MIGRATION ON STARTUP
    // 
    // Requirement 29.4: Run pending EF Core migrations automatically on startup
    // This ensures the database schema is always up-to-date without manual intervention.
    // 
    // Implementation:
    // - Runs synchronously during startup (not as background job)
    // - Creates a scope to resolve the DbContext
    // - Checks for pending migrations using GetPendingMigrationsAsync()
    // - Applies migrations if any are pending using MigrateAsync()
    // - Logs migration status for observability
    // - Handles errors gracefully with detailed logging
    // 
    // Why synchronous startup:
    // - Ensures database is ready before accepting requests
    // - Prevents race conditions where requests arrive before schema is updated
    // - Fails fast if migration errors occur, preventing partial deployments
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<Velocify.Infrastructure.Data.VelocifyDbContext>();
            
            // Skip migrations for InMemory database (used in tests)
            var isInMemory = context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
            
            if (!isInMemory)
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                
                if (pendingMigrations.Any())
                {
                    Log.Information("Applying {Count} pending database migrations: {Migrations}", 
                        pendingMigrations.Count(), 
                        string.Join(", ", pendingMigrations));
                    
                    await context.Database.MigrateAsync();
                    
                    Log.Information("Database migrations applied successfully");
                }
                else
                {
                    Log.Information("Database is up-to-date, no pending migrations");
                }
            }
            else
            {
                Log.Information("Using InMemory database, skipping migrations");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while applying database migrations");
            throw; // Fail fast - don't start the application if migrations fail
        }
    }

    // Configure the HTTP request pipeline.
    // Correlation ID middleware must be first to ensure TraceIdentifier is set for all subsequent middleware
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Exception handler must be registered early in the pipeline
    app.UseExceptionHandler();

    // Register request logging middleware after exception handler
    // This ensures exceptions are handled before logging completes
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Apply CORS policy before authentication and authorization
    // This ensures preflight requests are handled correctly
    app.UseCors("AllowFrontend");

    // Authentication must come before Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    app.MapGet("/weatherforecast", () =>
    {
        var forecast =  Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

    // Map API controllers
    app.MapControllers();

    // HEALTH CHECK ENDPOINT MAPPING
    // 
    // Maps /health endpoint for Azure App Service health monitoring (Requirement 17.6)
    // The HealthController at /health implements custom checks for:
    // - Database connectivity
    // - LangChain service availability  
    // - Disk space for log files
    // 
    // Note: We use MapControllers() which automatically maps the HealthController
    // at the /health route defined by its [Route("health")] attribute.
    // The AddHealthChecks() service registration enables health check infrastructure.
    app.MapHealthChecks("/health");

    // SIGNALR HUB CONFIGURATION
    // 
    // Hub Endpoint: /hubs/tasks
    // - Clients connect to this endpoint to receive real-time task notifications
    // - Connection is authenticated using JWT token (configured in AddJwtBearer above)
    // - Token can be provided in Authorization header or access_token query parameter
    // 
    // Authentication Flow:
    // 1. Client includes JWT in connection request
    // 2. OnMessageReceived event (configured above) extracts token from query string for SignalR
    // 3. JWT middleware validates token and populates User claims
    // 4. [Authorize] attribute on TaskHub ensures only authenticated users can connect
    // 5. OnConnectedAsync adds connection to user-specific group (UserId)
    // 
    // Real-Time Events:
    // - TaskAssigned: Pushed when a task is assigned to a user
    // - StatusChanged: Pushed when task status changes
    // - CommentAdded: Pushed when a comment is added to a task
    // - AiSuggestionReady: Pushed when AI digest or suggestion is available
    // 
    // Requirements:
    // - 6.5: Authenticate connections using JWT token
    // - 6.6: Add connections to user-specific groups
    app.MapHub<Velocify.API.Hubs.TaskHub>("/hubs/tasks");

    Log.Information("Velocify API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make Program class accessible for integration tests
public partial class Program { }
