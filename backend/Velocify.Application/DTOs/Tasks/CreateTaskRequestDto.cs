using Velocify.Domain.Enums;

namespace Velocify.Application.DTOs.Tasks;

/// <summary>
/// DTO for creating a task from the frontend.
/// Includes parentTaskId for backward compatibility with deployed frontend.
/// </summary>
public class CreateTaskRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public TaskCategory Category { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public string Tags { get; set; } = string.Empty;
    
    // For backward compatibility with deployed frontend
    // This field is accepted but ignored
    public Guid? ParentTaskId { get; set; }
}
