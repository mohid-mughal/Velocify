# AutoMapper Profiles

This folder contains AutoMapper profiles that map between Domain entities and Application DTOs.

## Profiles

### UserMappingProfile
- **User → UserDto**: Maps all user properties including role, productivity score, and timestamps
- **User → UserSummaryDto**: Maps basic user information (Id, FirstName, LastName, Email)

### TaskMappingProfile
- **TaskItem → TaskDto**: Maps task properties with navigation properties (AssignedTo, CreatedBy)
- **TaskItem → TaskDetailDto**: Extends TaskDto mapping with:
  - Comments (filtered to exclude deleted comments)
  - AuditLog (all audit log entries)
  - Subtasks (filtered to exclude deleted subtasks)
  - AverageSentiment (calculated from non-deleted comments with sentiment scores)

### CommentMappingProfile
- **TaskComment → CommentDto**: Maps comment properties with User navigation property

### AuditLogMappingProfile
- **TaskAuditLog → TaskAuditLogDto**: Maps audit log properties with ChangedBy navigation property

### NotificationMappingProfile
- **Notification → NotificationDto**: Maps all notification properties

## Registration

AutoMapper is registered in the `DependencyInjection.cs` file using:

```csharp
services.AddAutoMapper(Assembly.GetExecutingAssembly());
```

This automatically discovers and registers all Profile classes in the Application assembly.

## Usage in Handlers

To use AutoMapper in MediatR handlers, inject `IMapper` via constructor:

```csharp
public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDetailDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    public async Task<TaskDetailDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id);
        return _mapper.Map<TaskDetailDto>(task);
    }
}
```

## Special Mappings

### AverageSentiment Calculation
The TaskDetailDto includes an `AverageSentiment` property that is calculated during mapping:
- Only includes non-deleted comments
- Only includes comments with non-null SentimentScore
- Returns null if no comments have sentiment scores
- Returns the average of all sentiment scores otherwise

### Soft Delete Filtering
The following collections are automatically filtered to exclude soft-deleted items:
- Comments: `Where(c => !c.IsDeleted)`
- Subtasks: `Where(s => !s.IsDeleted)`

## Testing

Unit tests for all mapping profiles are located in:
`backend/Velocify.Tests/Application/Mappings/AutoMapperProfileTests.cs`

The tests verify:
- Configuration validity (all mappings are properly configured)
- Property mappings (all properties are correctly mapped)
- Navigation property mappings (related entities are mapped)
- Calculated properties (AverageSentiment is correctly calculated)
- Filtering logic (soft-deleted items are excluded)
