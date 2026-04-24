using Velocify.Domain.Enums;

namespace Velocify.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? TaskItemId { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public TaskItem? TaskItem { get; set; }

    // Business Methods
    public void MarkAsRead()
    {
        IsRead = true;
    }
}
