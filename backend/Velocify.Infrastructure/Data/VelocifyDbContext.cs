using Microsoft.EntityFrameworkCore;
using Velocify.Domain.Entities;

namespace Velocify.Infrastructure.Data;

public class VelocifyDbContext : DbContext
{
    public VelocifyDbContext(DbContextOptions<VelocifyDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TaskItem> TaskItems { get; set; } = null!;
    public DbSet<TaskComment> TaskComments { get; set; } = null!;
    public DbSet<TaskAuditLog> TaskAuditLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<AiInteractionLog> AiInteractionLogs { get; set; } = null!;
    public DbSet<TaskEmbedding> TaskEmbeddings { get; set; } = null!;
    public DbSet<UserTaskSummary> UserTaskSummaries { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Note: Query splitting behavior should be configured when registering the DbContext
        // in the DI container (e.g., in Program.cs or DependencyInjection.cs) using:
        // services.AddDbContext<VelocifyDbContext>(options => 
        //     options.UseSqlServer(connectionString, sqlOptions => 
        //         sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
        //
        // CARTESIAN EXPLOSION EXPLANATION:
        // When EF Core includes multiple collection navigations in a single query (e.g., TaskItem with Comments and AuditLogs),
        // it generates a SQL JOIN that produces a Cartesian product. For example, if a task has 10 comments and 20 audit logs,
        // the result set contains 10 × 20 = 200 rows, even though we only need 31 entities (1 task + 10 comments + 20 audit logs).
        // This wastes bandwidth, memory, and processing time.
        //
        // SPLIT QUERY SOLUTION:
        // SplitQuery mode tells EF Core to execute separate SQL queries for each collection navigation.
        // Instead of one large JOIN, it runs multiple smaller queries:
        //   1. SELECT * FROM TaskItems WHERE Id = @id
        //   2. SELECT * FROM TaskComments WHERE TaskItemId = @id
        //   3. SELECT * FROM TaskAuditLogs WHERE TaskItemId = @id
        // This eliminates duplication and significantly reduces data transfer, especially for entities with multiple collections.
        //
        // TRADE-OFF:
        // Split queries execute multiple round-trips to the database, which can increase latency in high-latency environments.
        // However, for Azure SQL Database with low latency and high bandwidth costs, split queries are almost always better.

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from IEntityTypeConfiguration classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VelocifyDbContext).Assembly);

        // Configure soft delete global query filters
        modelBuilder.Entity<TaskItem>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<TaskComment>().HasQueryFilter(c => !c.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}
