namespace Velocify.Application.Interfaces;

public interface ITaskHubService
{
    Task NotifyTaskAssigned(Guid userId, Guid taskId, string taskTitle);
    Task NotifyStatusChanged(Guid userId, Guid taskId, string taskTitle, string newStatus);
    Task NotifyCommentAdded(Guid userId, Guid taskId, string taskTitle, string commenterName);
    Task NotifyAiSuggestion(Guid userId, string suggestionType, string message);
}
