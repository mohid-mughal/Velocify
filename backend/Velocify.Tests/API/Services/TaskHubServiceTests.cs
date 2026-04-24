using Microsoft.AspNetCore.SignalR;
using Moq;
using Velocify.API.Hubs;
using Velocify.API.Services;
using Xunit;

namespace Velocify.Tests.API.Services;

public class TaskHubServiceTests
{
    private readonly Mock<IHubContext<TaskHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly TaskHubService _service;

    public TaskHubServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<TaskHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _service = new TaskHubService(_mockHubContext.Object);
    }

    [Fact]
    public async Task NotifyTaskAssigned_ShouldSendMessageToUserGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskTitle = "Test Task";

        // Act
        await _service.NotifyTaskAssigned(userId, taskId, taskTitle);

        // Assert
        _mockClients.Verify(
            c => c.Group(userId.ToString()),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "TaskAssigned",
                It.Is<object[]>(o => o.Length == 1),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyStatusChanged_ShouldSendMessageToUserGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskTitle = "Test Task";
        var newStatus = "InProgress";

        // Act
        await _service.NotifyStatusChanged(userId, taskId, taskTitle, newStatus);

        // Assert
        _mockClients.Verify(
            c => c.Group(userId.ToString()),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "StatusChanged",
                It.Is<object[]>(o => o.Length == 1),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyCommentAdded_ShouldSendMessageToUserGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskTitle = "Test Task";
        var commenterName = "John Doe";

        // Act
        await _service.NotifyCommentAdded(userId, taskId, taskTitle, commenterName);

        // Assert
        _mockClients.Verify(
            c => c.Group(userId.ToString()),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "CommentAdded",
                It.Is<object[]>(o => o.Length == 1),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyAiSuggestion_ShouldSendMessageToUserGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var suggestionType = "DailyDigest";
        var message = "You have 5 tasks due today";

        // Act
        await _service.NotifyAiSuggestion(userId, suggestionType, message);

        // Assert
        _mockClients.Verify(
            c => c.Group(userId.ToString()),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "AiSuggestionReady",
                It.Is<object[]>(o => o.Length == 1),
                default),
            Times.Once);
    }
}
