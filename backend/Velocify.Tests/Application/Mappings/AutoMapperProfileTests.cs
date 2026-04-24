using AutoMapper;
using FluentAssertions;
using Velocify.Application.DTOs.Notifications;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Mappings;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Tests.Application.Mappings;

public class AutoMapperProfileTests
{
    private readonly IMapper _mapper;

    public AutoMapperProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
            cfg.AddProfile<TaskMappingProfile>();
            cfg.AddProfile<CommentMappingProfile>();
            cfg.AddProfile<AuditLogMappingProfile>();
            cfg.AddProfile<NotificationMappingProfile>();
        });

        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Should_Map_User_To_UserDto()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Role = UserRole.Member,
            ProductivityScore = 85.5m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        // Act
        var result = _mapper.Map<UserDto>(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role);
        result.ProductivityScore.Should().Be(user.ProductivityScore);
        result.IsActive.Should().Be(user.IsActive);
        result.CreatedAt.Should().Be(user.CreatedAt);
        result.LastLoginAt.Should().Be(user.LastLoginAt);
    }

    [Fact]
    public void Should_Map_User_To_UserSummaryDto()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com"
        };

        // Act
        var result = _mapper.Map<UserSummaryDto>(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public void Should_Map_TaskItem_To_TaskDto()
    {
        // Arrange
        var assignedUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Assigned",
            LastName = "User",
            Email = "assigned@example.com"
        };

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Creator",
            LastName = "User",
            Email = "creator@example.com"
        };

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.High,
            Category = TaskCategory.Development,
            AssignedToUserId = assignedUser.Id,
            CreatedByUserId = createdUser.Id,
            AssignedTo = assignedUser,
            CreatedBy = createdUser,
            DueDate = DateTime.UtcNow.AddDays(7),
            EstimatedHours = 8.5m,
            Tags = "backend,api",
            AiPriorityScore = 0.85m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = _mapper.Map<TaskDto>(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
        result.Description.Should().Be(task.Description);
        result.Status.Should().Be(task.Status);
        result.Priority.Should().Be(task.Priority);
        result.Category.Should().Be(task.Category);
        result.AssignedTo.Should().NotBeNull();
        result.AssignedTo.Id.Should().Be(assignedUser.Id);
        result.CreatedBy.Should().NotBeNull();
        result.CreatedBy.Id.Should().Be(createdUser.Id);
        result.DueDate.Should().Be(task.DueDate);
        result.EstimatedHours.Should().Be(task.EstimatedHours);
        result.Tags.Should().Be(task.Tags);
        result.AiPriorityScore.Should().Be(task.AiPriorityScore);
    }

    [Fact]
    public void Should_Map_TaskItem_To_TaskDetailDto_With_Comments_And_Subtasks()
    {
        // Arrange
        var assignedUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Assigned",
            LastName = "User",
            Email = "assigned@example.com"
        };

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Creator",
            LastName = "User",
            Email = "creator@example.com"
        };

        var commentUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Commenter",
            LastName = "User",
            Email = "commenter@example.com"
        };

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.High,
            Category = TaskCategory.Development,
            AssignedToUserId = assignedUser.Id,
            CreatedByUserId = createdUser.Id,
            AssignedTo = assignedUser,
            CreatedBy = createdUser,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Comments = new List<TaskComment>
            {
                new TaskComment
                {
                    Id = Guid.NewGuid(),
                    Content = "Great work!",
                    SentimentScore = 0.9m,
                    UserId = commentUser.Id,
                    User = commentUser,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new TaskComment
                {
                    Id = Guid.NewGuid(),
                    Content = "Needs improvement",
                    SentimentScore = 0.3m,
                    UserId = commentUser.Id,
                    User = commentUser,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new TaskComment
                {
                    Id = Guid.NewGuid(),
                    Content = "Deleted comment",
                    UserId = commentUser.Id,
                    User = commentUser,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = true
                }
            },
            Subtasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Subtask 1",
                    Status = TaskStatus.Completed,
                    Priority = TaskPriority.Medium,
                    Category = TaskCategory.Development,
                    AssignedToUserId = assignedUser.Id,
                    CreatedByUserId = createdUser.Id,
                    AssignedTo = assignedUser,
                    CreatedBy = createdUser,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Deleted Subtask",
                    Status = TaskStatus.Pending,
                    Priority = TaskPriority.Low,
                    Category = TaskCategory.Development,
                    AssignedToUserId = assignedUser.Id,
                    CreatedByUserId = createdUser.Id,
                    AssignedTo = assignedUser,
                    CreatedBy = createdUser,
                    IsDeleted = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            },
            AuditLogs = new List<TaskAuditLog>
            {
                new TaskAuditLog
                {
                    Id = 1,
                    FieldName = "Status",
                    OldValue = "Pending",
                    NewValue = "InProgress",
                    ChangedByUserId = createdUser.Id,
                    ChangedBy = createdUser,
                    ChangedAt = DateTime.UtcNow
                }
            }
        };

        // Act
        var result = _mapper.Map<TaskDetailDto>(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
        
        // Should only include non-deleted comments
        result.Comments.Should().HaveCount(2);
        result.Comments.Should().AllSatisfy(c => c.User.Should().NotBeNull());
        
        // Should only include non-deleted subtasks
        result.Subtasks.Should().HaveCount(1);
        result.Subtasks[0].Title.Should().Be("Subtask 1");
        
        // Should include audit logs
        result.AuditLog.Should().HaveCount(1);
        result.AuditLog[0].FieldName.Should().Be("Status");
        result.AuditLog[0].ChangedBy.Should().NotBeNull();
        
        // Should calculate average sentiment (0.9 + 0.3) / 2 = 0.6
        result.AverageSentiment.Should().Be(0.6m);
    }

    [Fact]
    public void Should_Map_TaskComment_To_CommentDto()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskItemId = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Content = "This is a comment",
            SentimentScore = 0.75m,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = _mapper.Map<CommentDto>(comment);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(comment.Id);
        result.TaskItemId.Should().Be(comment.TaskItemId);
        result.Content.Should().Be(comment.Content);
        result.SentimentScore.Should().Be(comment.SentimentScore);
        result.CreatedAt.Should().Be(comment.CreatedAt);
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public void Should_Map_TaskAuditLog_To_TaskAuditLogDto()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com"
        };

        var auditLog = new TaskAuditLog
        {
            Id = 1,
            TaskItemId = Guid.NewGuid(),
            ChangedByUserId = user.Id,
            ChangedBy = user,
            FieldName = "Priority",
            OldValue = "Low",
            NewValue = "High",
            ChangedAt = DateTime.UtcNow
        };

        // Act
        var result = _mapper.Map<TaskAuditLogDto>(auditLog);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(auditLog.Id);
        result.TaskItemId.Should().Be(auditLog.TaskItemId);
        result.FieldName.Should().Be(auditLog.FieldName);
        result.OldValue.Should().Be(auditLog.OldValue);
        result.NewValue.Should().Be(auditLog.NewValue);
        result.ChangedAt.Should().Be(auditLog.ChangedAt);
        result.ChangedBy.Should().NotBeNull();
        result.ChangedBy.Id.Should().Be(user.Id);
    }

    [Fact]
    public void Should_Map_Notification_To_NotificationDto()
    {
        // Arrange
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = NotificationType.Assigned,
            Message = "You have been assigned a new task",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            TaskItemId = Guid.NewGuid()
        };

        // Act
        var result = _mapper.Map<NotificationDto>(notification);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(notification.Id);
        result.UserId.Should().Be(notification.UserId);
        result.Type.Should().Be(notification.Type);
        result.Message.Should().Be(notification.Message);
        result.IsRead.Should().Be(notification.IsRead);
        result.CreatedAt.Should().Be(notification.CreatedAt);
        result.TaskItemId.Should().Be(notification.TaskItemId);
    }

    [Fact]
    public void Should_Calculate_Null_AverageSentiment_When_No_Comments_With_Sentiment()
    {
        // Arrange
        var assignedUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "User",
            LastName = "One",
            Email = "user@example.com"
        };

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "User",
            LastName = "Two",
            Email = "user2@example.com"
        };

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task without sentiment",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Low,
            Category = TaskCategory.Development,
            AssignedToUserId = assignedUser.Id,
            CreatedByUserId = createdUser.Id,
            AssignedTo = assignedUser,
            CreatedBy = createdUser,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Comments = new List<TaskComment>
            {
                new TaskComment
                {
                    Id = Guid.NewGuid(),
                    Content = "Comment without sentiment",
                    SentimentScore = null,
                    UserId = assignedUser.Id,
                    User = assignedUser,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            }
        };

        // Act
        var result = _mapper.Map<TaskDetailDto>(task);

        // Assert
        result.Should().NotBeNull();
        result.AverageSentiment.Should().BeNull();
    }
}
