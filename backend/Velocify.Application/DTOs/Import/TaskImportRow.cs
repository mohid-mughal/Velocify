using Velocify.Domain.Enums;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Application.DTOs.Import;

public class TaskImportRow
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskCategory Category { get; set; }
    public string AssignedToEmail { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public string Tags { get; set; } = string.Empty;
}
