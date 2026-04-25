using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Velocify.Application.DTOs.Dashboard;
using Velocify.Application.DTOs.Tasks;
using Velocify.Application.Interfaces;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;
using TaskStatus = Velocify.Domain.Enums.TaskStatus;

namespace Velocify.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for dashboard-related queries.
/// 
/// DESIGN PRINCIPLES:
/// - Uses AsReadOnly() extension for all read operations to optimize memory and performance
/// - Aggregates data from TaskItem table for dashboard statistics
/// - Optimized queries to minimize database round trips
/// 
/// PERFORMANCE OPTIMIZATIONS:
/// - Uses AsNoTracking() for read-only queries (reduces memory by 30-50%)
/// - Filters by IsDeleted = false to use filtered indexes
/// - Groups and aggregates data in database rather than in memory
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly VelocifyDbContext _context;
    private readonly IMapper _mapper;

    public DashboardRepository(VelocifyDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummary(Guid userId)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        var tasks = await _context.TaskItems
            .AsReadOnly()
            .Where(t => t.AssignedToUserId == userId && !t.IsDeleted)
            .ToListAsync();

        return new DashboardSummaryDto
        {
            PendingCount = tasks.Count(t => t.Status == TaskStatus.Pending),
            InProgressCount = tasks.Count(t => t.Status == TaskStatus.InProgress),
            CompletedCount = tasks.Count(t => t.Status == TaskStatus.Completed),
            BlockedCount = tasks.Count(t => t.Status == TaskStatus.Blocked),
            OverdueCount = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < now && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled),
            DueTodayCount = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date == today && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled)
        };
    }

    public async Task<List<VelocityDataPoint>> GetDashboardVelocity(Guid userId, int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days).Date;

        var completedTasks = await _context.TaskItems
            .AsReadOnly()
            .Where(t => t.AssignedToUserId == userId 
                && !t.IsDeleted 
                && t.Status == TaskStatus.Completed 
                && t.CompletedAt.HasValue 
                && t.CompletedAt.Value >= startDate)
            .GroupBy(t => t.CompletedAt!.Value.Date)
            .Select(g => new VelocityDataPoint
            {
                Date = g.Key,
                CompletedCount = g.Count()
            })
            .OrderBy(v => v.Date)
            .ToListAsync();

        return completedTasks;
    }

    public async Task<List<WorkloadDistributionDto>> GetWorkloadDistribution()
    {
        var workloadData = await _context.TaskItems
            .AsReadOnly()
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.AssignedToUserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalTaskCount = g.Count(),
                PendingCount = g.Count(t => t.Status == TaskStatus.Pending),
                InProgressCount = g.Count(t => t.Status == TaskStatus.InProgress),
                CompletedCount = g.Count(t => t.Status == TaskStatus.Completed),
                BlockedCount = g.Count(t => t.Status == TaskStatus.Blocked)
            })
            .ToListAsync();

        var userIds = workloadData.Select(w => w.UserId).ToList();
        var users = await _context.Users
            .AsReadOnly()
            .Where(u => userIds.Contains(u.Id) && u.IsActive)
            .ToListAsync();

        var result = workloadData
            .Select(w =>
            {
                var user = users.FirstOrDefault(u => u.Id == w.UserId);
                if (user == null) return null;

                return new WorkloadDistributionDto
                {
                    User = _mapper.Map<Application.DTOs.Users.UserSummaryDto>(user),
                    TotalTaskCount = w.TotalTaskCount,
                    PendingCount = w.PendingCount,
                    InProgressCount = w.InProgressCount,
                    CompletedCount = w.CompletedCount,
                    BlockedCount = w.BlockedCount
                };
            })
            .Where(w => w != null)
            .Cast<WorkloadDistributionDto>()
            .ToList();

        return result;
    }

    public async Task<List<TaskDto>> GetOverdueTasks(Guid userId)
    {
        var now = DateTime.UtcNow;

        var overdueTasks = await _context.TaskItems
            .AsReadOnly()
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => t.AssignedToUserId == userId 
                && !t.IsDeleted 
                && t.DueDate.HasValue 
                && t.DueDate.Value < now 
                && t.Status != TaskStatus.Completed 
                && t.Status != TaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return _mapper.Map<List<TaskDto>>(overdueTasks);
    }
}
