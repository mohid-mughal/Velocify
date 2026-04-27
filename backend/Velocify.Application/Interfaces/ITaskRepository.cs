using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Enums;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskDetailDto?> GetById(Guid id);
    Task<PagedResult<TaskDto>> GetList(
        TaskStatus? status = null,
        TaskPriority? priority = null,
        TaskCategory? category = null,
        Guid? assignedToUserId = null,
        DateTime? dueDateFrom = null,
        DateTime? dueDateTo = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20);
    Task<TaskDto> Create(TaskDto taskDto, Guid createdByUserId);
    Task<TaskDto> Update(TaskDto taskDto, Guid updatedByUserId);
    Task Delete(Guid id, Guid deletedByUserId);
    Task<List<TaskDto>> GetSubtasks(Guid parentTaskId);
    Task<List<CommentDto>> GetComments(Guid taskId);
    Task<CommentDto?> GetCommentById(Guid commentId);
    Task<CommentDto> CreateComment(Guid taskItemId, string content, Guid userId);
    Task DeleteComment(Guid commentId, Guid userId);
    Task UpdateCommentSentiment(Guid commentId, decimal sentimentScore);
    Task<List<TaskAuditLogDto>> GetAuditLog(Guid taskId);
}
