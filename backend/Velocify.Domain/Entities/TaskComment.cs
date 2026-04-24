using Velocify.Domain.Enums;

namespace Velocify.Domain.Entities;

public class TaskComment
{
    // Properties
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public decimal? SentimentScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation Properties
    public TaskItem TaskItem { get; set; } = null!;
    public User User { get; set; } = null!;

    // Business Methods
    public bool CanBeDeletedBy(User user)
    {
        // User can delete their own comment
        if (user.Id == UserId)
        {
            return true;
        }

        // Admin and SuperAdmin can delete any comment
        return user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin;
    }
}
