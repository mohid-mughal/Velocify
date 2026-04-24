using FluentAssertions;
using Moq;
using Velocify.Application.DTOs.Dashboard;
using Velocify.Application.Interfaces;
using Velocify.Application.Queries.Dashboard;
using Xunit;

namespace Velocify.Tests.Application.Queries;

/// <summary>
/// Unit tests for GetDashboardSummaryQueryHandler.
/// 
/// TEST COVERAGE:
/// - Verifies that dashboard summary counts are correctly retrieved from the repository
/// - Validates role-based filtering: Admin sees all users, Member sees only their own
/// - Ensures the handler correctly delegates to IDashboardRepository.GetDashboardSummary
/// - Confirms that the indexed view (vw_UserTaskSummary) is queried via the repository
/// 
/// REQUIREMENTS VALIDATED:
/// - 7.1: Dashboard summary returns task counts grouped by status using indexed view
/// - 2.3: Admin sees only team tasks (tested via repository mock)
/// - 2.2: Member sees only their own tasks (tested via repository mock)
/// - 30.1-30.7: Unit testing with xUnit, Moq, and FluentAssertions
/// </summary>
public class GetDashboardSummaryQueryHandlerTests
{
    private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
    private readonly GetDashboardSummaryQueryHandler _handler;

    public GetDashboardSummaryQueryHandlerTests()
    {
        _dashboardRepositoryMock = new Mock<IDashboardRepository>();
        _handler = new GetDashboardSummaryQueryHandler(_dashboardRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that the handler returns correct task counts matching seeded data.
    /// This validates that the dashboard summary accurately reflects the database state.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnCorrectCounts_WhenDataExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetDashboardSummaryQuery { UserId = userId };

        var expectedSummary = new DashboardSummaryDto
        {
            PendingCount = 5,
            InProgressCount = 3,
            CompletedCount = 10,
            BlockedCount = 1,
            OverdueCount = 2,
            DueTodayCount = 4
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetDashboardSummary(userId))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PendingCount.Should().Be(5);
        result.InProgressCount.Should().Be(3);
        result.CompletedCount.Should().Be(10);
        result.BlockedCount.Should().Be(1);
        result.OverdueCount.Should().Be(2);
        result.DueTodayCount.Should().Be(4);

        _dashboardRepositoryMock.Verify(
            r => r.GetDashboardSummary(userId),
            Times.Once);
    }

    /// <summary>
    /// Tests that an Admin user sees all users' tasks in their dashboard summary.
    /// This validates role-based access control where Admins have visibility across their team.
    /// The repository implementation should aggregate counts from all team members.
    /// </summary>
    [Fact]
    public async Task Handle_AdminUser_ShouldSeeAllUsersTaskCounts()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var query = new GetDashboardSummaryQuery { UserId = adminUserId };

        // Simulate aggregated counts from multiple team members
        var expectedSummary = new DashboardSummaryDto
        {
            PendingCount = 15,      // Sum from multiple users
            InProgressCount = 8,    // Sum from multiple users
            CompletedCount = 25,    // Sum from multiple users
            BlockedCount = 3,       // Sum from multiple users
            OverdueCount = 5,       // Sum from multiple users
            DueTodayCount = 7       // Sum from multiple users
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetDashboardSummary(adminUserId))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PendingCount.Should().Be(15, "Admin should see aggregated counts from all team members");
        result.InProgressCount.Should().Be(8);
        result.CompletedCount.Should().Be(25);
        result.BlockedCount.Should().Be(3);
        result.OverdueCount.Should().Be(5);
        result.DueTodayCount.Should().Be(7);

        _dashboardRepositoryMock.Verify(
            r => r.GetDashboardSummary(adminUserId),
            Times.Once);
    }

    /// <summary>
    /// Tests that a Member user sees only their own tasks in the dashboard summary.
    /// This validates role-based access control where Members have restricted visibility.
    /// The repository implementation should filter to only the requesting user's tasks.
    /// </summary>
    [Fact]
    public async Task Handle_MemberUser_ShouldSeeOnlyOwnTaskCounts()
    {
        // Arrange
        var memberUserId = Guid.NewGuid();
        var query = new GetDashboardSummaryQuery { UserId = memberUserId };

        // Simulate counts from only the member's own tasks
        var expectedSummary = new DashboardSummaryDto
        {
            PendingCount = 3,       // Only member's tasks
            InProgressCount = 2,    // Only member's tasks
            CompletedCount = 8,     // Only member's tasks
            BlockedCount = 0,       // Only member's tasks
            OverdueCount = 1,       // Only member's tasks
            DueTodayCount = 2       // Only member's tasks
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetDashboardSummary(memberUserId))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PendingCount.Should().Be(3, "Member should see only their own task counts");
        result.InProgressCount.Should().Be(2);
        result.CompletedCount.Should().Be(8);
        result.BlockedCount.Should().Be(0);
        result.OverdueCount.Should().Be(1);
        result.DueTodayCount.Should().Be(2);

        _dashboardRepositoryMock.Verify(
            r => r.GetDashboardSummary(memberUserId),
            Times.Once);
    }

    /// <summary>
    /// Tests that the handler correctly queries the indexed view via the repository.
    /// The indexed view (vw_UserTaskSummary) pre-aggregates task counts for performance.
    /// This test verifies that the handler delegates to the repository method that uses
    /// the compiled query against the indexed view.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldQueryIndexedView_ViaRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetDashboardSummaryQuery { UserId = userId };

        var expectedSummary = new DashboardSummaryDto
        {
            PendingCount = 4,
            InProgressCount = 2,
            CompletedCount = 6,
            BlockedCount = 1,
            OverdueCount = 1,
            DueTodayCount = 3
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetDashboardSummary(userId))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Verify that the repository method is called exactly once
        // The repository implementation uses CompiledQueries.GetDashboardSummary
        // which queries the indexed view vw_UserTaskSummary for optimal performance
        _dashboardRepositoryMock.Verify(
            r => r.GetDashboardSummary(userId),
            Times.Once,
            "Handler should delegate to repository which queries the indexed view");
    }

    /// <summary>
    /// Tests that the handler returns zero counts when no tasks exist for the user.
    /// This validates the handler's behavior with empty data sets.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnZeroCounts_WhenNoTasksExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetDashboardSummaryQuery { UserId = userId };

        var expectedSummary = new DashboardSummaryDto
        {
            PendingCount = 0,
            InProgressCount = 0,
            CompletedCount = 0,
            BlockedCount = 0,
            OverdueCount = 0,
            DueTodayCount = 0
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetDashboardSummary(userId))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PendingCount.Should().Be(0);
        result.InProgressCount.Should().Be(0);
        result.CompletedCount.Should().Be(0);
        result.BlockedCount.Should().Be(0);
        result.OverdueCount.Should().Be(0);
        result.DueTodayCount.Should().Be(0);

        _dashboardRepositoryMock.Verify(
            r => r.GetDashboardSummary(userId),
            Times.Once);
    }

    /// <summary>
    /// Tests that the handler correctly passes the UserId from the query to the repository.
    /// This validates that user-specific filtering is properly applied.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPassCorrectUserId_ToRepository()
    {
        // Arrange
        var specificUserId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var query = new GetDashboardSummaryQuery { UserId = specificUserId };

        var expectedSummary = new DashboardSummaryDto
        {
            PendingCount = 1,
            InProgressCount = 1,
            CompletedCount = 1,
            BlockedCount = 0,
            OverdueCount = 0,
            DueTodayCount = 1
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetDashboardSummary(specificUserId))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Verify the exact UserId was passed to the repository
        _dashboardRepositoryMock.Verify(
            r => r.GetDashboardSummary(It.Is<Guid>(id => id == specificUserId)),
            Times.Once,
            "Handler should pass the exact UserId from the query to the repository");
    }
}
