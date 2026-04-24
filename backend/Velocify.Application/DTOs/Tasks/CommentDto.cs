using Velocify.Application.DTOs.Users;

namespace Velocify.Application.DTOs.Tasks;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public UserSummaryDto User { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public decimal? SentimentScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
