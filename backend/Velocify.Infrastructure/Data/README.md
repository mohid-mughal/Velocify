# VelocifyDbContext

## Overview

The `VelocifyDbContext` is the main Entity Framework Core database context for the Velocify platform. It manages all entity sets and configures database behavior for optimal performance.

## Key Features

### 1. Split Query Behavior

The DbContext is configured with `UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)` to prevent Cartesian explosion when loading entities with multiple collection navigations.

**Why Split Queries?**
- When EF Core includes multiple collections (e.g., TaskItem with Comments and AuditLogs), a single JOIN query creates a Cartesian product
- Example: A task with 10 comments and 20 audit logs produces 200 rows (10 × 20) instead of 31 entities
- Split queries execute separate SQL statements for each collection, eliminating duplication
- This significantly reduces data transfer and memory usage, especially important for Azure SQL Database

### 2. Connection Pooling

Connection pooling is configured in `DependencyInjection.cs` with:
- **Min Pool Size = 2**: Keeps 2 warm connections to prevent Azure SQL Serverless from auto-pausing
- **Max Pool Size = 100**: Limits concurrent connections to prevent overwhelming the database

### 3. Soft Delete Query Filters

Global query filters automatically exclude soft-deleted records:
- `TaskItem` entities where `IsDeleted = true` are filtered out
- `TaskComment` entities where `IsDeleted = true` are filtered out

This means all queries automatically respect soft deletes without explicit WHERE clauses.

### 4. Entity Configuration

All entity configurations are applied via `IEntityTypeConfiguration<T>` classes using:
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(VelocifyDbContext).Assembly);
```

This keeps the DbContext clean and separates entity configuration into dedicated files.

## Registered Entities

- **User**: User accounts with authentication and profile information
- **TaskItem**: Work items with status, priority, and assignment tracking
- **TaskComment**: Comments on tasks with sentiment analysis
- **TaskAuditLog**: Change history for tasks (partitioned by month)
- **Notification**: User notifications for real-time updates
- **UserSession**: Refresh token storage for authentication
- **AiInteractionLog**: AI feature usage tracking and metrics
- **TaskEmbedding**: Cached embedding vectors for semantic search

## Usage

The DbContext is registered in the DI container via the `AddInfrastructure` extension method:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

Connection string is read from `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=VelocifyDb;Min Pool Size=2;Max Pool Size=100"
  }
}
```

## Next Steps

After creating the DbContext, you need to:
1. Create entity configurations (Task 7.2-7.3)
2. Create initial migration (Task 7.4)
3. Apply optimization migrations (Task 8.1-8.5)
