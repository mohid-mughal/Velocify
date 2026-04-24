using Velocify.Domain.Enums;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Domain.Entities;

public class TaskItem
{
    // Properties
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskCategory Category { get; set; }
    public Guid AssignedToUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public string Tags { get; set; } = string.Empty;
    public decimal? AiPriorityScore { get; set; }
    public decimal? PredictedCompletionProbability { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation Properties
    public User AssignedTo { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskAuditLog> AuditLogs { get; set; } = new List<TaskAuditLog>();
    public ICollection<TaskItem> Subtasks { get; set; } = new List<TaskItem>();
    public TaskItem? ParentTask { get; set; }
    public TaskEmbedding? Embedding { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    // Business Methods
    public bool IsOverdue()
    {
        return DueDate.HasValue && 
               DueDate.Value < DateTime.UtcNow && 
               Status != TaskStatus.Completed;
    }

    public bool CanBeEditedBy(User user)
    {
        return user.Id == AssignedToUserId || 
               user.Id == CreatedByUserId || 
               user.Role == UserRole.Admin || 
               user.Role == UserRole.SuperAdmin;
    }

    public void MarkAsCompleted()
    {
        Status = TaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
