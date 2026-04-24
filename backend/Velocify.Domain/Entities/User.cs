using Velocify.Domain.Enums;

namespace Velocify.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public decimal ProductivityScore { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<TaskItem> TasksAssigned { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> TasksCreated { get; set; } = new List<TaskItem>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<TaskAuditLog> AuditLogs { get; set; } = new List<TaskAuditLog>();
    public ICollection<AiInteractionLog> AiInteractionLogs { get; set; } = new List<AiInteractionLog>();

    // Business methods
    public string CalculateFullName()
    {
        return $"{FirstName} {LastName}".Trim();
    }

    public bool IsInRole(UserRole role)
    {
        return Role == role;
    }

    public bool CanAccessTask(TaskItem task)
    {
        // SuperAdmin can access all tasks
        if (Role == UserRole.SuperAdmin)
        {
            return true;
        }

        // Admin can access tasks assigned to them or created by them
        if (Role == UserRole.Admin)
        {
            return task.AssignedToUserId == Id || task.CreatedByUserId == Id;
        }

        // Member can only access their own assigned tasks
        return task.AssignedToUserId == Id;
    }
}
