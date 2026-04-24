namespace Velocify.Application.DTOs.AI;

public class WorkloadSuggestion
{
    public Guid TaskId { get; set; }
    public Guid SuggestedAssigneeId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
