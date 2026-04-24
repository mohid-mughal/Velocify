using Velocify.Application.DTOs.Users;

namespace Velocify.Application.DTOs.Tasks;

public class TaskAuditLogDto
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public UserSummaryDto ChangedBy { get; set; } = null!;
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
}
