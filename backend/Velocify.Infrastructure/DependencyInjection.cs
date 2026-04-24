using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Velocify.Infrastructure.Data;

namespace Velocify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure DbContext with SQL Server and connection pooling
        // 
        // CONNECTION POOLING FOR SERVERLESS DATABASE:
        // Azure SQL Database Serverless automatically pauses after inactivity and resumes on first connection.
        // Connection pooling keeps a minimum number of connections alive to reduce cold start latency.
        // 
        // Min Pool Size = 2: Maintains 2 warm connections to prevent the database from auto-pausing during low traffic.
        //                    This ensures the first request after idle period doesn't experience the ~5-10 second resume delay.
        // 
        // Max Pool Size = 100: Limits concurrent connections to prevent overwhelming the database during traffic spikes.
        //                      Azure SQL Database has connection limits based on service tier, and exceeding them causes errors.
        //                      100 is a safe limit for F1/Basic tier while allowing reasonable concurrency.
        //
        // Connection pooling is configured in the connection string with parameters:
        // "Min Pool Size=2;Max Pool Size=100;Pooling=true"
        services.AddDbContext<VelocifyDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        });

        // Register repositories
        services.AddScoped<Application.Interfaces.ITaskRepository, Repositories.TaskRepository>();
        services.AddScoped<Application.Interfaces.IUserRepository, Repositories.UserRepository>();
        services.AddScoped<Application.Interfaces.INotificationService, Repositories.NotificationRepository>();

        // Register services
        services.AddScoped<Application.Interfaces.IJwtTokenService, Services.JwtTokenService>();
        services.AddScoped<Application.Interfaces.IAuthService, Services.AuthService>();

        // Register AI services
        services.AddScoped<Application.Interfaces.INaturalLanguageTaskService, Services.AiServices.NaturalLanguageTaskService>();
        services.AddScoped<Application.Interfaces.ITaskDecompositionService, Services.AiServices.TaskDecompositionService>();
        services.AddScoped<Application.Interfaces.IWorkloadBalancingService, Services.AiServices.WorkloadBalancingService>();
        services.AddScoped<Application.Interfaces.ISemanticSearchService, Services.AiServices.SemanticSearchService>();
        services.AddScoped<Application.Interfaces.IAiImportService, Services.AiServices.AiImportService>();
        services.AddScoped<Application.Interfaces.ICommentSentimentService, Services.AiServices.CommentSentimentService>();
        
        // Register DailyDigestService as both IDailyDigestService and IHostedService
        // We use AddSingleton because IHostedService must be singleton
        // The same instance serves both as the background service and the on-demand digest generator
        services.AddSingleton<Services.AiServices.DailyDigestService>();
        services.AddSingleton<Application.Interfaces.IDailyDigestService>(sp => 
            sp.GetRequiredService<Services.AiServices.DailyDigestService>());
        services.AddHostedService(sp => 
            sp.GetRequiredService<Services.AiServices.DailyDigestService>());

        // Register ProductivityScoreCalculationService as IHostedService
        // Runs every 6 hours to recalculate productivity scores for all users
        // Requirements: 7.6, 15.10
        services.AddHostedService<Services.BackgroundServices.ProductivityScoreCalculationService>();

        return services;
    }
}
