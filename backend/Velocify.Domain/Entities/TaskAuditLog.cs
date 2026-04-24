namespace Velocify.Domain.Entities;

public class TaskAuditLog
{
    public long Id { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation Properties
    public TaskItem TaskItem { get; set; } = null!;
    public User ChangedBy { get; set; } = null!;
}
