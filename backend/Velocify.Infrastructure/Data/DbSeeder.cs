using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Infrastructure.Data;

/// <summary>
/// Database seeder for creating initial admin user and sample data.
/// This ensures the application has a default admin user and demo tasks for first-time setup.
/// </summary>
public class DbSeeder
{
    private readonly VelocifyDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(VelocifyDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with initial admin user and sample tasks if no users exist.
    /// 
    /// Default Admin Credentials:
    /// Email: admin@velocify.com
    /// Password: Admin@123
    /// Role: SuperAdmin
    /// 
    /// SECURITY NOTE: Change this password immediately after first login in production!
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Check if any users exist
            var userExists = await _context.Users.AnyAsync();
            
            if (!userExists)
            {
                _logger.LogInformation("No users found in database. Creating default admin user and sample data...");
                
                // Create default admin user
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@velocify.com",
                    // Password: Admin@123 (hashed with BCrypt)
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRole.SuperAdmin,
                    ProductivityScore = 85,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLoginAt = null
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Default admin user created successfully");
                
                // Create sample member users
                var memberUser1 = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@velocify.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
                    Role = UserRole.Member,
                    ProductivityScore = 75,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLoginAt = null
                };

                var memberUser2 = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@velocify.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
                    Role = UserRole.Member,
                    ProductivityScore = 90,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLoginAt = null
                };

                _context.Users.AddRange(memberUser1, memberUser2);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Sample member users created successfully");

                // Create sample tasks for admin user
                var now = DateTime.UtcNow;
                var sampleTasks = new List<TaskItem>
                {
                    // Pending tasks
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Review Q1 Performance Metrics",
                        Description = "Analyze team performance metrics for Q1 and prepare summary report for stakeholders.",
                        Status = TaskStatus.Pending,
                        Priority = TaskPriority.High,
                        Category = TaskCategory.Operations,
                        AssignedToUserId = adminUser.Id,
                        CreatedByUserId = adminUser.Id,
                        DueDate = now.AddDays(3),
                        EstimatedHours = 4,
                        Tags = "reporting,metrics,quarterly",
                        CreatedAt = now.AddDays(-2),
                        UpdatedAt = now.AddDays(-2),
                        IsDeleted = false
                    },
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Update API Documentation",
                        Description = "Update REST API documentation with new endpoints and authentication changes.",
                        Status = TaskStatus.Pending,
                        Priority = TaskPriority.Medium,
                        Category = TaskCategory.Development,
                        AssignedToUserId = memberUser1.Id,
                        CreatedByUserId = adminUser.Id,
                        DueDate = now.AddDays(5),
                        EstimatedHours = 3,
                        Tags = "documentation,api,technical",
                        CreatedAt = now.AddDays(-1),
                        UpdatedAt = now.AddDays(-1),
                        IsDeleted = false
                    },
                    // In Progress tasks
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Implement User Dashboard",
                        Description = "Build responsive dashboard with task statistics, velocity charts, and AI digest.",
                        Status = TaskStatus.InProgress,
                        Priority = TaskPriority.High,
                        Category = TaskCategory.Development,
                        AssignedToUserId = adminUser.Id,
                        CreatedByUserId = adminUser.Id,
                        DueDate = now.AddDays(7),
                        EstimatedHours = 16,
                        ActualHours = 8,
                        Tags = "frontend,dashboard,react",
                        CreatedAt = now.AddDays(-5),
                        UpdatedAt = now.AddHours(-2),
                        IsDeleted = false
                    },
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Design Marketing Campaign",
                        Description = "Create visual assets and copy for Q2 product launch campaign.",
                        Status = TaskStatus.InProgress,
                        Priority = TaskPriority.Medium,
                        Category = TaskCategory.Marketing,
                        AssignedToUserId = memberUser2.Id,
                        CreatedByUserId = adminUser.Id,
                        DueDate = now.AddDays(10),
                        EstimatedHours = 12,
                        ActualHours = 5,
                        Tags = "marketing,design,campaign",
                        CreatedAt = now.AddDays(-3),
                        UpdatedAt = now.AddHours(-1),
                        IsDeleted = false
                    },
                    // Completed tasks
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Setup CI/CD Pipeline",
                        Description = "Configure GitHub Actions for automated testing and deployment to Azure.",
                        Status = TaskStatus.Completed,
                        Priority = TaskPriority.High,
                        Category = TaskCategory.Development,
                        AssignedToUserId = adminUser.Id,
                        CreatedByUserId = adminUser.Id,
                        DueDate = now.AddDays(-2),
                        CompletedAt = now.AddDays(-1),
                        EstimatedHours = 6,
                        ActualHours = 7,
                        Tags = "devops,ci-cd,automation",
                        CreatedAt = now.AddDays(-7),
                        UpdatedAt = now.AddDays(-1),
                        IsDeleted = false
                    },
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Database Performance Optimization",
                        Description = "Add indexes and optimize slow queries identified in production monitoring.",
                        Status = TaskStatus.Completed,
                        Priority = TaskPriority.Critical,
                        Category = TaskCategory.Development,
                        AssignedToUserId = memberUser1.Id,
                        CreatedByUserId = adminUser.Id,
                        DueDate = now.AddDays(-5),
                        CompletedAt = now.AddDays(-4),
                        EstimatedHours = 8,
                        ActualHours = 6,
                        Tags = "database,performance,optimization",
                        CreatedAt = now.AddDays(-10),
                        UpdatedAt = now.AddDays(-4),
                        IsDeleted = false
                    },
                    // Overdue task
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Security Audit Report",
                        Description = "Complete security audit and document findings with remediation recommendations.",
                        Status = TaskStatus.InProgress,
                        Priority = TaskPriority.Critical,
                        Category = TaskCategory.Operations,
                        AssignedToUserId = adminUser.Id,
                        CreatedByUserId = adminUser.Id,
                        DueDate = now.AddDays(-2),
                        EstimatedHours = 10,
                        ActualHours = 6,
                        Tags = "security,audit,compliance",
                        CreatedAt = now.AddDays(-14),
                        UpdatedAt = now.AddHours(-3),
                        IsDeleted = false
                    },
                    // Blocked task
                    new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Integrate Payment Gateway",
                        Description = "Integrate Stripe payment processing for subscription billing.",
                        Status = TaskStatus.Blocked,
                        Priority = TaskPriority.High,
                        Category = TaskCategory.Development,
                        AssignedToUserId = memberUser1.Id,
                        CreatedByUserId = adminUser.Id,
                        DueDate = now.AddDays(14),
                        EstimatedHours = 12,
                        ActualHours = 3,
                        Tags = "payment,integration,stripe",
                        CreatedAt = now.AddDays(-6),
                        UpdatedAt = now.AddDays(-1),
                        IsDeleted = false
                    }
                };

                _context.TaskItems.AddRange(sampleTasks);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Sample tasks created successfully");
                _logger.LogInformation("===========================================");
                _logger.LogInformation("DEFAULT CREDENTIALS:");
                _logger.LogInformation("Admin - Email: admin@velocify.com, Password: Admin@123");
                _logger.LogInformation("Member 1 - Email: john.doe@velocify.com, Password: Member@123");
                _logger.LogInformation("Member 2 - Email: jane.smith@velocify.com, Password: Member@123");
                _logger.LogInformation("===========================================");
                _logger.LogWarning("SECURITY WARNING: Please change these default passwords after first login!");
            }
            else
            {
                _logger.LogInformation("Users already exist in database. Skipping seeding.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }
}
