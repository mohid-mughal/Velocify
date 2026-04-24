using Microsoft.AspNetCore.SignalR;
using Velocify.API.Hubs;
using Velocify.Application.Interfaces;

namespace Velocify.API.Services;

/// <summary>
/// SignalR hub service for broadcasting real-time task notifications to connected clients.
/// 
/// ARCHITECTURE:
/// - Uses IHubContext to send messages to SignalR clients without being inside the hub itself
/// - Broadcasts to user-specific groups (each user has a group identified by their UserId)
/// - Supports multiple concurrent connections per user (all devices/tabs receive notifications)
/// 
/// EVENT TYPES:
/// - TaskAssigned: Notifies a user when a task is assigned to them
/// - StatusChanged: Notifies assignee and creator when task status changes
/// - CommentAdded: Notifies task assignee when a new comment is posted
/// - AiSuggestionReady: Notifies user when AI-generated suggestions are available
/// 
/// REQUIREMENTS:
/// - 6.1: Push task-assigned events to assigned user's SignalR connection
/// - 6.2: Push status-changed events to both assignee and creator
/// - 6.3: Push new-comment events to task assignee
/// - 6.4: Push ai-suggestion-ready events to relevant user
/// </summary>
public class TaskHubService : ITaskHubService
{
    private readonly IHubContext<TaskHub> _hubContext;

    public TaskHubService(IHubContext<TaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Notifies a user that a task has been assigned to them.
    /// Broadcasts to all connections in the user's group.
    /// </summary>
    /// <param name="userId">The ID of the user to notify</param>
    /// <param name="taskId">The ID of the assigned task</param>
    /// <param name="taskTitle">The title of the assigned task</param>
    public async Task NotifyTaskAssigned(Guid userId, Guid taskId, string taskTitle)
    {
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("TaskAssigned", new
            {
                TaskId = taskId,
                TaskTitle = taskTitle,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notifies relevant users when a task status changes.
    /// Broadcasts to the assignee's group.
    /// Note: If you need to notify both assignee and creator, call this method twice
    /// with different userIds, or modify to accept multiple userIds.
    /// </summary>
    /// <param name="userId">The ID of the user to notify (assignee or creator)</param>
    /// <param name="taskId">The ID of the task</param>
    /// <param name="taskTitle">The title of the task</param>
    /// <param name="newStatus">The new status of the task</param>
    public async Task NotifyStatusChanged(Guid userId, Guid taskId, string taskTitle, string newStatus)
    {
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("StatusChanged", new
            {
                TaskId = taskId,
                TaskTitle = taskTitle,
                NewStatus = newStatus,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notifies the task assignee when a new comment is added.
    /// Broadcasts to the assignee's group.
    /// </summary>
    /// <param name="userId">The ID of the task assignee to notify</param>
    /// <param name="taskId">The ID of the task</param>
    /// <param name="taskTitle">The title of the task</param>
    /// <param name="commenterName">The name of the user who added the comment</param>
    public async Task NotifyCommentAdded(Guid userId, Guid taskId, string taskTitle, string commenterName)
    {
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("CommentAdded", new
            {
                TaskId = taskId,
                TaskTitle = taskTitle,
                CommenterName = commenterName,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notifies a user when an AI-generated suggestion becomes available.
    /// Used for daily digests, workload balancing suggestions, and other AI features.
    /// </summary>
    /// <param name="userId">The ID of the user to notify</param>
    /// <param name="suggestionType">The type of AI suggestion (e.g., "DailyDigest", "WorkloadBalancing")</param>
    /// <param name="message">The suggestion message or summary</param>
    public async Task NotifyAiSuggestion(Guid userId, string suggestionType, string message)
    {
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("AiSuggestionReady", new
            {
                SuggestionType = suggestionType,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
    }
}
