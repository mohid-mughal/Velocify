using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;

namespace Velocify.Application.Commands.Tasks;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;

    public UpdateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var existingTask = await _taskRepository.GetById(request.Id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.Id} not found");
        }

        var taskDto = new TaskDto
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            Status = existingTask.Status,
            Priority = request.Priority,
            Category = request.Category,
            AssignedTo = new UserSummaryDto { Id = request.AssignedToUserId },
            CreatedBy = existingTask.CreatedBy,
            DueDate = request.DueDate,
            EstimatedHours = request.EstimatedHours,
            ActualHours = request.ActualHours,
            Tags = request.Tags,
            CompletedAt = existingTask.CompletedAt,
            AiPriorityScore = existingTask.AiPriorityScore,
            PredictedCompletionProbability = existingTask.PredictedCompletionProbability,
            CreatedAt = existingTask.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        // Concurrency handling is done in the repository layer (Infrastructure)
        var updatedTask = await _taskRepository.Update(taskDto, request.UpdatedByUserId);
        
        return updatedTask;
    }
}
