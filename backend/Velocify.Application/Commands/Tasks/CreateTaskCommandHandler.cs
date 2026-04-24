using MediatR;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Application.Commands.Tasks;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskDto = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = TaskStatus.Pending,
            Priority = request.Priority,
            Category = request.Category,
            AssignedTo = new UserSummaryDto { Id = request.AssignedToUserId },
            CreatedBy = new UserSummaryDto { Id = request.CreatedByUserId },
            DueDate = request.DueDate,
            EstimatedHours = request.EstimatedHours,
            Tags = request.Tags,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdTask = await _taskRepository.Create(taskDto, request.CreatedByUserId);
        
        return createdTask;
    }
}
