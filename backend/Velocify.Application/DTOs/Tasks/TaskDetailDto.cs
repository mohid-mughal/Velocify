namespace Velocify.Application.DTOs.Tasks;

public class TaskDetailDto : TaskDto
{
    public List<CommentDto> Comments { get; set; } = new();
    public List<TaskAuditLogDto> AuditLog { get; set; } = new();
    public List<TaskDto> Subtasks { get; set; } = new();
    public decimal? AverageSentiment { get; set; }
}
