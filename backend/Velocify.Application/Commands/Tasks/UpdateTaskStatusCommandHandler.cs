using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Application.Commands.Tasks;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;

    public UpdateTaskStatusCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var existingTask = await _taskRepository.GetById(request.Id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.Id} not found");
        }

        var taskDto = new TaskDto
        {
            Id = existingTask.Id,
            Title = existingTask.Title,
            Description = existingTask.Description,
            Status = request.Status,
            Priority = existingTask.Priority,
            Category = existingTask.Category,
            AssignedTo = existingTask.AssignedTo,
            CreatedBy = existingTask.CreatedBy,
            DueDate = existingTask.DueDate,
            EstimatedHours = existingTask.EstimatedHours,
            ActualHours = existingTask.ActualHours,
            Tags = existingTask.Tags,
            AiPriorityScore = existingTask.AiPriorityScore,
            PredictedCompletionProbability = existingTask.PredictedCompletionProbability,
            CreatedAt = existingTask.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            // Set CompletedAt when status changes to Completed
            CompletedAt = request.Status == TaskStatus.Completed 
                ? DateTime.UtcNow 
                : existingTask.CompletedAt
        };

        var updatedTask = await _taskRepository.Update(taskDto, request.UpdatedByUserId);
        
        return updatedTask;
    }
}
