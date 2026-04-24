using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Velocify.API.Controllers;
using Velocify.Application.DTOs.Dashboard;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Queries.Dashboard;
using Xunit;

namespace Velocify.Tests.API.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly DashboardController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public DashboardControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new DashboardController(_mediatorMock.Object);

        // Setup user claims for authentication
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetSummary_ShouldReturnOk_WithDashboardSummary()
    {
        // Arrange
        var expectedSummary = new DashboardSummaryDto
        {
            PendingCount = 5,
            InProgressCount = 3,
            CompletedCount = 10,
            BlockedCount = 1,
            OverdueCount = 2,
            DueTodayCount = 4
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDashboardSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _controller.GetSummary();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSummary);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetDashboardSummaryQuery>(q => q.UserId == _testUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVelocity_ShouldReturnOk_WithVelocityData()
    {
        // Arrange
        var expectedVelocity = new List<VelocityDataPoint>
        {
            new VelocityDataPoint { Date = DateTime.UtcNow.AddDays(-2), CompletedCount = 5 },
            new VelocityDataPoint { Date = DateTime.UtcNow.AddDays(-1), CompletedCount = 3 },
            new VelocityDataPoint { Date = DateTime.UtcNow, CompletedCount = 7 }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetDashboardVelocityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVelocity);

        // Act
        var result = await _controller.GetVelocity(30);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedVelocity);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetDashboardVelocityQuery>(q => q.UserId == _testUserId && q.Days == 30),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkload_ShouldReturnOk_WithWorkloadDistribution()
    {
        // Arrange
        var expectedWorkload = new List<WorkloadDistributionDto>
        {
            new WorkloadDistributionDto 
            { 
                User = new Velocify.Application.DTOs.Users.UserSummaryDto 
                { 
                    Id = Guid.NewGuid(), 
                    FirstName = "John", 
                    LastName = "Doe",
                    Email = "john@example.com"
                }, 
                TotalTaskCount = 10,
                PendingCount = 3,
                InProgressCount = 4,
                CompletedCount = 2,
                BlockedCount = 1
            },
            new WorkloadDistributionDto 
            { 
                User = new Velocify.Application.DTOs.Users.UserSummaryDto 
                { 
                    Id = Guid.NewGuid(), 
                    FirstName = "Jane", 
                    LastName = "Smith",
                    Email = "jane@example.com"
                }, 
                TotalTaskCount = 8,
                PendingCount = 2,
                InProgressCount = 3,
                CompletedCount = 3,
                BlockedCount = 0
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetWorkloadDistributionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWorkload);

        // Act
        var result = await _controller.GetWorkload();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedWorkload);

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<GetWorkloadDistributionQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOverdue_ShouldReturnOk_WithOverdueTasks()
    {
        // Arrange
        var expectedTasks = new List<TaskDto>
        {
            new TaskDto { Id = Guid.NewGuid(), Title = "Overdue Task 1", DueDate = DateTime.UtcNow.AddDays(-2) },
            new TaskDto { Id = Guid.NewGuid(), Title = "Overdue Task 2", DueDate = DateTime.UtcNow.AddDays(-1) }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOverdueTasksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTasks);

        // Act
        var result = await _controller.GetOverdue();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTasks);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetOverdueTasksQuery>(q => q.UserId == _testUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
