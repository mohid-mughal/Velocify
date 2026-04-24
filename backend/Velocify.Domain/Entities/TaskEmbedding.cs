namespace Velocify.Domain.Entities;

public class TaskEmbedding
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public string EmbeddingVector { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation Property
    public TaskItem TaskItem { get; set; } = null!;
}
