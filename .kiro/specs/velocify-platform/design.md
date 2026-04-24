# Design Document

## Overview

Velocify is a production-grade, AI-augmented task management platform built with clean architecture principles and designed for independent deployment of its three core components: backend API, frontend application, and database. The system leverages LangChain.NET for intelligent features, SignalR for real-time collaboration, and advanced database optimizations for performance at scale.

The architecture follows a strict separation of concerns with four backend layers (Domain, Application, Infrastructure, API) implementing CQRS pattern via MediatR. The frontend uses a feature-sliced architecture with Zustand for client state and TanStack Query for server state management. All components communicate via RESTful APIs and WebSocket connections, with JWT-based authentication and role-based authorization.

Key design principles:
- Independent deployability: Each component can be deployed, scaled, and maintained separately
- Performance-first: Database optimizations including indexed views, compiled queries, and table partitioning
- AI-native: Seven distinct AI features powered by LangChain.NET with structured output parsing
- Real-time collaboration: SignalR hub broadcasting events to connected clients
- Observability: Structured logging with Serilog, health checks, and comprehensive metrics
- Security: JWT with refresh token rotation, role-based access control, and hashed token storage

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Frontend                             │
│  React 18 + Vite + Zustand + TanStack Query + SignalR      │
│                      (Vercel)                                │
└────────────┬────────────────────────────────┬───────────────┘
             │ HTTPS/REST                     │ WebSocket
             │                                │
┌────────────▼────────────────────────────────▼───────────────┐
│                      Backend API                             │
│   ASP.NET Core 8 + MediatR + SignalR + LangChain.NET       │
│         (Azure App Service F1 Tier)                         │
└────────────┬─────────────────────────────────────────────────┘
             │ EF Core 8
             │
┌────────────▼─────────────────────────────────────────────────┐
│                   Azure SQL Database                         │
│        Serverless with Partitioning & Indexed Views         │
└──────────────────────────────────────────────────────────────┘
```

### Backend Layer Architecture

The backend follows Clean Architecture with strict dependency rules:

```
┌─────────────────────────────────────────────────────────────┐
│                    Velocify.API                              │
│  Controllers, Middleware, SignalR Hubs, Program.cs          │
│  Dependencies: Application, Infrastructure                   │
└────────────┬────────────────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────────────────┐
│               Velocify.Application                           │
│  Commands, Queries, DTOs, Interfaces, Validators            │
│  MediatR Handlers, Pipeline Behaviors                        │
│  Dependencies: Domain only                                   │
└────────────┬────────────────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────────────────┐
│               Velocify.Infrastructure                        │
│  DbContext, Repositories, AI Services, Migrations           │
│  Dependencies: Application, Domain                           │
└────────────┬────────────────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────────────────┐
│                 Velocify.Domain                              │
│  Entities, Enums, Value Objects, Domain Logic               │
│  Dependencies: None (pure domain)                            │
└──────────────────────────────────────────────────────────────┘
```

### CQRS Pattern with MediatR

Every operation is either a Command (write) or Query (read):

- Commands: CreateTaskCommand, UpdateTaskCommand, DeleteTaskCommand, etc.
- Queries: GetTaskByIdQuery, GetTaskListQuery, GetDashboardSummaryQuery, etc.

Pipeline behaviors execute in order:
1. ValidationBehavior: Validates using FluentValidation rules
2. LoggingBehavior: Logs request/response with Serilog
3. PerformanceBehavior: Warns if handler exceeds 500ms

### Authentication Flow

```
1. User submits credentials → POST /api/v1/auth/login
2. Backend validates credentials
3. Backend generates:
   - Access Token (JWT, 15min TTL, stored in memory on client)
   - Refresh Token (7day TTL, hashed with SHA-256, stored in DB)
4. Client receives both tokens
5. Client includes Access Token in Authorization header
6. When Access Token expires:
   - Client calls POST /api/v1/auth/refresh with Refresh Token
   - Backend validates hash, generates new Access Token
   - Backend invalidates old Refresh Token (rotation)
7. On logout: Backend revokes Refresh Token
```

### Real-Time Communication Flow

```
1. User logs in → Frontend establishes SignalR connection
2. Backend authenticates connection via JWT
3. Backend adds connection to user-specific group (UserId)
4. Events occur (task assigned, status changed, comment added)
5. Backend broadcasts to relevant groups
6. Frontend receives event → Invalidates TanStack Query cache
7. UI automatically refetches and updates
```

## Components and Interfaces

### Domain Layer Components

#### Entities

**User Entity**
- Properties: Id, FirstName, LastName, Email, PasswordHash, Role, ProductivityScore, IsActive, CreatedAt, UpdatedAt, LastLoginAt
- Navigation: TasksAssigned, TasksCreated, Comments, Sessions, Notifications
- Business Logic: CalculateFullName(), IsInRole(), CanAccessTask()

**TaskItem Entity**
- Properties: Id, Title, Description, Status, Priority, Category, AssignedToUserId, CreatedByUserId, ParentTaskId, DueDate, CompletedAt, EstimatedHours, ActualHours, Tags, AiPriorityScore, PredictedCompletionProbability, IsDeleted, CreatedAt, UpdatedAt, RowVersion
- Navigation: AssignedTo, CreatedBy, Comments, AuditLogs, Subtasks, ParentTask, Embedding
- Business Logic: IsOverdue(), CanBeEditedBy(), MarkAsCompleted(), SoftDelete()

**TaskComment Entity**
- Properties: Id, TaskItemId, UserId, Content, SentimentScore, CreatedAt, IsDeleted
- Navigation: TaskItem, User
- Business Logic: CanBeDeletedBy()

**TaskAuditLog Entity**
- Properties: Id, TaskItemId, ChangedByUserId, FieldName, OldValue, NewValue, ChangedAt
- Navigation: TaskItem, ChangedBy
- Note: Partitioned by month on ChangedAt column

**Notification Entity**
- Properties: Id, UserId, Type, Message, IsRead, CreatedAt, TaskItemId
- Navigation: User, TaskItem
- Business Logic: MarkAsRead()

**UserSession Entity**
- Properties: Id, UserId, RefreshToken (hashed), ExpiresAt, IsRevoked, CreatedAt, IpAddress
- Navigation: User
- Business Logic: IsValid(), Revoke()

**AiInteractionLog Entity**
- Properties: Id, UserId, FeatureType, InputSummary, OutputSummary, TokensUsed, LatencyMs, CreatedAt
- Navigation: User
- Purpose: Track AI feature adoption and performance metrics

**TaskEmbedding Entity**
- Properties: Id, TaskItemId, EmbeddingVector (JSON array as nvarchar), CreatedAt
- Navigation: TaskItem
- Purpose: Cache embeddings for semantic search

#### Enums

```csharp
public enum UserRole { SuperAdmin, Admin, Member }
public enum TaskStatus { Pending, InProgress, Completed, Cancelled, Blocked }
public enum TaskPriority { Critical, High, Medium, Low }
public enum TaskCategory { Development, Design, Marketing, Operations, Research, Other }
public enum NotificationType { DueSoon, Overdue, Assigned, StatusChanged, AiSuggestion }
public enum AiFeatureType { TaskCreation, Decomposition, Digest, Prioritization, Search, Import }
```

### Application Layer Components

#### Commands

**CreateTaskCommand**
- Properties: Title, Description, Priority, Category, AssignedToUserId, DueDate, EstimatedHours, Tags
- Handler: CreateTaskCommandHandler
- Validation: Title required (max 200), Priority valid enum, Category valid enum
- Returns: TaskDto

**UpdateTaskCommand**
- Properties: Id, Title, Description, Priority, Category, AssignedToUserId, DueDate, EstimatedHours, ActualHours, Tags
- Handler: UpdateTaskCommandHandler
- Validation: All fields validated, handles optimistic concurrency
- Returns: TaskDto

**UpdateTaskStatusCommand**
- Properties: Id, Status
- Handler: UpdateTaskStatusCommandHandler
- Business Logic: Sets CompletedAt when status changes to Completed
- Returns: TaskDto

**DeleteTaskCommand**
- Properties: Id
- Handler: DeleteTaskCommandHandler
- Business Logic: Soft delete (sets IsDeleted = true)
- Returns: Unit

**CreateCommentCommand**
- Properties: TaskItemId, Content
- Handler: CreateCommentCommandHandler
- Side Effect: Triggers async sentiment analysis
- Returns: CommentDto

**RegisterUserCommand**
- Properties: FirstName, LastName, Email, Password
- Handler: RegisterUserCommandHandler
- Validation: Email unique, password strength requirements
- Returns: AuthResponseDto

**LoginCommand**
- Properties: Email, Password
- Handler: LoginCommandHandler
- Returns: AuthResponseDto (with tokens)

**RefreshTokenCommand**
- Properties: RefreshToken
- Handler: RefreshTokenCommandHandler
- Business Logic: Validates token, rotates to new token
- Returns: AuthResponseDto

#### Queries

**GetTaskListQuery**
- Properties: Status, Priority, Category, AssignedToUserId, DueDateFrom, DueDateTo, SearchTerm, Page, PageSize
- Handler: GetTaskListQueryHandler
- Optimization: Uses compiled query, AsNoTracking, filtered indexes
- Returns: PagedResult<TaskDto>

**GetTaskByIdQuery**
- Properties: Id
- Handler: GetTaskByIdQueryHandler
- Returns: TaskDetailDto (includes comments, audit log, subtasks)

**GetDashboardSummaryQuery**
- Properties: UserId (from claims)
- Handler: GetDashboardSummaryQueryHandler
- Optimization: Queries indexed view vw_UserTaskSummary
- Returns: DashboardSummaryDto

**GetDashboardVelocityQuery**
- Properties: UserId, Days (default 30)
- Handler: GetDashboardVelocityQueryHandler
- Returns: List<VelocityDataPoint>

**GetUserProductivityQuery**
- Properties: UserId
- Handler: GetUserProductivityQueryHandler
- Returns: ProductivityDto (score + historical trend)

#### DTOs

**TaskDto**: Id, Title, Description, Status, Priority, Category, AssignedTo (UserSummaryDto), CreatedBy (UserSummaryDto), DueDate, CompletedAt, EstimatedHours, ActualHours, Tags, AiPriorityScore, PredictedCompletionProbability, CreatedAt, UpdatedAt

**TaskDetailDto**: Extends TaskDto, adds Comments, AuditLog, Subtasks, AverageSentiment

**UserDto**: Id, FirstName, LastName, Email, Role, ProductivityScore, IsActive, CreatedAt, LastLoginAt

**AuthResponseDto**: AccessToken, RefreshToken, User (UserDto), ExpiresIn

**DashboardSummaryDto**: PendingCount, InProgressCount, CompletedCount, BlockedCount, OverdueCount, DueTodayCount

**CommentDto**: Id, TaskItemId, User (UserSummaryDto), Content, SentimentScore, CreatedAt

#### Interfaces

**IAuthService**: Register, Login, RefreshToken, Logout, RevokeAllSessions

**ITaskRepository**: GetById, GetList, Create, Update, Delete, GetSubtasks, GetComments, GetAuditLog

**IUserRepository**: GetById, GetByEmail, GetList, Create, Update, Delete, GetProductivityHistory

**INaturalLanguageTaskService**: ParseTaskFromText(input) → CreateTaskCommand

**ITaskDecompositionService**: DecomposeTask(taskId) → List<SubtaskSuggestion>

**IDailyDigestService**: GenerateDigest(userId) → DigestDto

**IWorkloadBalancingService**: GetSuggestions() → List<WorkloadSuggestion>

**ISemanticSearchService**: SearchTasks(query) → List<TaskDto>

**IAiImportService**: NormalizeImport(csvData) → List<TaskImportRow>

**ICommentSentimentService**: AnalyzeSentiment(content) → decimal

**INotificationService**: CreateNotification, GetUserNotifications, MarkAsRead, MarkAllAsRead

**ITaskHubService**: NotifyTaskAssigned, NotifyStatusChanged, NotifyCommentAdded, NotifyAiSuggestion

### Infrastructure Layer Components

#### VelocifyDbContext

Configuration:
- UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery) to prevent Cartesian explosion
- Connection pooling: Min=2, Max=100
- Entities configured via IEntityTypeConfiguration classes
- Audit log partitioning configured
- Indexed view vw_UserTaskSummary defined
- Soft delete global query filter on TaskItem and TaskComment

#### Repositories

**TaskRepository**
- Implements ITaskRepository
- Uses AsReadOnly() extension for read operations
- Handles DbUpdateConcurrencyException for updates
- Records audit log entries on changes

**UserRepository**
- Implements IUserRepository
- Uses compiled query for GetByEmail
- Hashes passwords with BCrypt or ASP.NET Core Identity

#### CompiledQueries Static Class

```csharp
// Serves GET /api/v1/tasks endpoint which is called on every page load
// Compilation eliminates repeated LINQ-to-SQL translation overhead
public static readonly Func<VelocifyDbContext, Guid, Task<TaskItem>> GetTaskById = 
    EF.CompileAsyncQuery((VelocifyDbContext context, Guid id) => 
        context.TaskItems.FirstOrDefault(t => t.Id == id));

// Serves GET /api/v1/dashboard/summary which is the landing page
// Pre-compilation saves ~50ms per request on cold starts
public static readonly Func<VelocifyDbContext, Guid, IEnumerable<UserTaskSummary>> GetDashboardSummary = 
    EF.CompileQuery((VelocifyDbContext context, Guid userId) => 
        context.UserTaskSummaries.Where(s => s.UserId == userId));

// Serves authentication flow which happens on every login
// Compilation matters here because auth is latency-sensitive
public static readonly Func<VelocifyDbContext, string, Task<User>> GetUserByEmail = 
    EF.CompileAsyncQuery((VelocifyDbContext context, string email) => 
        context.Users.FirstOrDefault(u => u.Email == email));
```

#### Query Extensions

```csharp
public static class QueryExtensions
{
    // Wraps AsNoTracking and AsSplitQuery for read-only operations
    // AsNoTracking prevents EF from holding entities in memory for change detection
    // which reduces allocations significantly on high-traffic read endpoints
    // AsSplitQuery prevents Cartesian explosion when including multiple collections
    public static IQueryable<T> AsReadOnly<T>(this IQueryable<T> query) where T : class
    {
        return query.AsNoTracking().AsSplitQuery();
    }
}
```

#### AI Services (Infrastructure/AiServices)

**NaturalLanguageTaskService**
- Uses LangChain structured output parser
- Polly retry policy: 3 attempts, exponential backoff (1s, 2s, 4s)
- Logs to AiInteractionLog with FeatureType.TaskCreation
- Extracts: Title, Description, Priority, Category, AssigneeEmail, DueDate

**TaskDecompositionService**
- Prompt instructs model to return JSON array of subtasks
- Caps at 8 subtasks
- Returns: Title, EstimatedHours for each subtask
- Logs to AiInteractionLog with FeatureType.Decomposition

**DailyDigestService**
- Implemented as IHostedService running at 8 AM daily
- Queries tasks due today and overdue for each active user
- Uses LangChain summarization chain
- Creates Notification with Type.AiSuggestion
- Pushes via SignalR when user connects

**WorkloadBalancingService**
- Admin/SuperAdmin only
- Analyzes: task count per user, productivity scores, due dates
- Provides structured JSON to LangChain
- Returns: TaskId, SuggestedAssigneeId, Reason
- Logs to AiInteractionLog with FeatureType.Prioritization

**SemanticSearchService**
- Runs SQL LIKE search and embedding search in parallel using Task.WhenAll
- Generates query embedding with LangChain
- Compares against TaskEmbedding table (cosine similarity)
- Merges and ranks results by combined score
- Regenerates embeddings on task title/description change
- Logs to AiInteractionLog with FeatureType.Search

**AiImportService**
- Analyzes CSV headers with LangChain
- Maps non-standard columns to schema fields
- Normalizes enum values (e.g., "very high" → Priority.Critical)
- Returns normalized list for user review
- Logs to AiInteractionLog with FeatureType.Import

**CommentSentimentService**
- Async analysis (non-blocking)
- Returns score 0.0 (negative) to 1.0 (positive)
- Stores in TaskComment.SentimentScore
- Aggregated per task for team health monitoring

#### Background Services

**ProductivityScoreCalculationService** (IHostedService)
- Runs every 6 hours
- Calls stored procedure usp_RecalculateUserProductivityScores
- Updates User.ProductivityScore for all users
- Logs execution time and affected rows

**DailyDigestService** (IHostedService)
- Runs daily at 8 AM
- Generates digest for each active user
- Creates notifications and pushes via SignalR

#### Migrations

**Initial Migration**: Creates all tables with proper indexes and constraints

**Optimization Migration**: Adds filtered indexes, composite indexes, indexed view

**Partitioning Migration**: Creates partition function and scheme for TaskAuditLog

### API Layer Components

#### Controllers

All controllers inherit from ApiController base class with [ApiVersion("1.0")] attribute.

**AuthController**: Register, Login, Refresh, Logout, RevokeAllSessions

**UsersController**: GetMe, UpdateMe, GetUsers, GetUserById, UpdateUserRole, DeleteUser, GetProductivity

**TasksController**: GetTasks, GetTaskById, CreateTask, UpdateTask, UpdateTaskStatus, DeleteTask, GetTaskHistory, GetComments, CreateComment, DeleteComment, GetSubtasks, ExportTasks, ImportTasks

**DashboardController**: GetSummary, GetVelocity, GetWorkload, GetOverdue

**AiController**: ParseTask, DecomposeTask, SearchTasks, GetWorkloadSuggestions, NormalizeImport, GetMyDigest

**NotificationsController**: GetNotifications, MarkAsRead, MarkAllAsRead

**HealthController**: GetHealth

#### SignalR Hub

**TaskHub**
- Methods: OnConnectedAsync (joins user group), OnDisconnectedAsync
- Events: TaskAssigned, StatusChanged, CommentAdded, AiSuggestionReady
- Authentication: JWT from query string or header
- Groups: One per UserId

#### Middleware

**ExceptionHandlingMiddleware** (implements IExceptionHandler)
- Maps domain exceptions to HTTP status codes
- Returns ProblemDetails response
- Logs with correlation ID
- Never exposes stack traces to client

**RequestLoggingMiddleware**
- Logs request/response at Information level
- Excludes /health endpoint
- Includes correlation ID, user ID, duration

#### MediatR Pipeline Behaviors

**ValidationBehavior<TRequest, TResponse>**
- Executes first in pipeline
- Runs FluentValidation validators
- Throws ValidationException if validation fails
- Order matters: validation must happen before any business logic

**LoggingBehavior<TRequest, TResponse>**
- Executes second in pipeline
- Logs request and response
- Includes correlation ID and user ID
- Order matters: we want to log after validation passes

**PerformanceBehavior<TRequest, TResponse>**
- Executes third in pipeline
- Measures handler execution time
- Logs warning if exceeds 500ms
- Order matters: measures only handler time, not validation or logging overhead

## Data Models

### Database Schema

#### Users Table
```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role INT NOT NULL,
    ProductivityScore DECIMAL(5,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2 NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

CREATE INDEX IX_Users_Email ON Users(Email);
```

#### TaskItems Table
```sql
CREATE TABLE TaskItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status INT NOT NULL,
    Priority INT NOT NULL,
    Category INT NOT NULL,
    AssignedToUserId UNIQUEIDENTIFIER NOT NULL,
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    ParentTaskId UNIQUEIDENTIFIER NULL,
    DueDate DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    EstimatedHours DECIMAL(5,2) NULL,
    ActualHours DECIMAL(5,2) NULL,
    Tags NVARCHAR(500) NULL,
    AiPriorityScore DECIMAL(5,2) NULL,
    PredictedCompletionProbability DECIMAL(5,2) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_TaskItems_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    CONSTRAINT FK_TaskItems_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_TaskItems_Parent FOREIGN KEY (ParentTaskId) REFERENCES TaskItems(Id)
);

-- Filtered index for soft deletes: only indexes active records
CREATE INDEX IX_TaskItem_AssignedTo_Active ON TaskItems(AssignedToUserId) WHERE IsDeleted = 0;

-- Composite indexes for common query patterns
CREATE INDEX IX_TaskItem_Dashboard ON TaskItems(AssignedToUserId, Status, IsDeleted);
CREATE INDEX IX_TaskItem_Overdue ON TaskItems(DueDate, Priority, IsDeleted);
CREATE INDEX IX_TaskItem_CreatedBy ON TaskItems(CreatedByUserId, CreatedAt DESC, IsDeleted);
CREATE INDEX IX_TaskItem_Tags ON TaskItems(Tags);
```

#### TaskComments Table
```sql
CREATE TABLE TaskComments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TaskItemId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    SentimentScore DECIMAL(3,2) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_TaskComments_Task FOREIGN KEY (TaskItemId) REFERENCES TaskItems(Id),
    CONSTRAINT FK_TaskComments_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_TaskComments_Task_Active ON TaskComments(TaskItemId) WHERE IsDeleted = 0;
```

#### TaskAuditLog Table (Partitioned)
```sql
-- Partition function: monthly partitions for the last 12 months
CREATE PARTITION FUNCTION PF_AuditLog_Monthly (DATETIME2)
AS RANGE RIGHT FOR VALUES (
    '2024-01-01', '2024-02-01', '2024-03-01', '2024-04-01',
    '2024-05-01', '2024-06-01', '2024-07-01', '2024-08-01',
    '2024-09-01', '2024-10-01', '2024-11-01', '2024-12-01'
);

CREATE PARTITION SCHEME PS_AuditLog_Monthly
AS PARTITION PF_AuditLog_Monthly
ALL TO ([PRIMARY]);

CREATE TABLE TaskAuditLog (
    Id BIGINT IDENTITY(1,1) NOT NULL,
    TaskItemId UNIQUEIDENTIFIER NOT NULL,
    ChangedByUserId UNIQUEIDENTIFIER NOT NULL,
    FieldName NVARCHAR(100) NOT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_TaskAuditLog PRIMARY KEY (Id, ChangedAt)
) ON PS_AuditLog_Monthly(ChangedAt);

-- Audit tables grow unboundedly and queries against recent audit data
-- should never scan historical partitions. Partitioning by month keeps
-- the last 12 months hot and allows efficient archival of older data.
CREATE INDEX IX_TaskAuditLog_Task ON TaskAuditLog(TaskItemId, ChangedAt);
```

#### Notifications Table
```sql
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type INT NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TaskItemId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Notifications_Task FOREIGN KEY (TaskItemId) REFERENCES TaskItems(Id)
);

CREATE INDEX IX_Notifications_User ON Notifications(UserId, IsRead, CreatedAt DESC);
```

#### UserSessions Table
```sql
CREATE TABLE UserSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    RefreshToken NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(50) NULL,
    CONSTRAINT FK_UserSessions_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_UserSessions_User ON UserSessions(UserId);
CREATE INDEX IX_UserSessions_Token ON UserSessions(RefreshToken);
```

#### AiInteractionLog Table
```sql
CREATE TABLE AiInteractionLog (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    FeatureType INT NOT NULL,
    InputSummary NVARCHAR(1000) NOT NULL,
    OutputSummary NVARCHAR(1000) NOT NULL,
    TokensUsed INT NULL,
    LatencyMs INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AiInteractionLog_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_AiInteractionLog_User ON AiInteractionLog(UserId, CreatedAt DESC);
CREATE INDEX IX_AiInteractionLog_Feature ON AiInteractionLog(FeatureType, CreatedAt DESC);
```

#### TaskEmbedding Table
```sql
CREATE TABLE TaskEmbedding (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TaskItemId UNIQUEIDENTIFIER NOT NULL,
    EmbeddingVector NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TaskEmbedding_Task FOREIGN KEY (TaskItemId) REFERENCES TaskItems(Id),
    CONSTRAINT UQ_TaskEmbedding_Task UNIQUE (TaskItemId)
);
```

#### Indexed View for Dashboard
```sql
-- Pre-aggregates task counts per user per status
-- Dashboard queries this view instead of running COUNT(*) GROUP BY on every request
-- SCHEMABINDING ensures the view stays in sync with underlying tables
CREATE VIEW vw_UserTaskSummary WITH SCHEMABINDING
AS
SELECT 
    AssignedToUserId AS UserId,
    Status,
    COUNT_BIG(*) AS TaskCount
FROM dbo.TaskItems
WHERE IsDeleted = 0
GROUP BY AssignedToUserId, Status;

CREATE UNIQUE CLUSTERED INDEX IX_UserTaskSummary ON vw_UserTaskSummary(UserId, Status);
```

#### Stored Procedure for Productivity Score
```sql
-- Productivity score is a complex metric: tasks completed on time divided by
-- total assigned, weighted by priority. This calculation involves multiple
-- aggregations and joins. Running it in the database is more efficient than
-- pulling all data to the application layer, especially for batch updates.
CREATE PROCEDURE usp_RecalculateUserProductivityScores
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE u
    SET ProductivityScore = ISNULL(
        (
            SELECT 
                CAST(SUM(CASE 
                    WHEN t.Status = 2 AND (t.CompletedAt IS NULL OR t.CompletedAt <= t.DueDate) 
                    THEN CASE t.Priority 
                        WHEN 0 THEN 4.0  -- Critical
                        WHEN 1 THEN 3.0  -- High
                        WHEN 2 THEN 2.0  -- Medium
                        WHEN 3 THEN 1.0  -- Low
                    END
                    ELSE 0
                END) / NULLIF(COUNT(*), 0) AS DECIMAL(5,2))
            FROM TaskItems t
            WHERE t.AssignedToUserId = u.Id AND t.IsDeleted = 0
        ), 0)
    FROM Users u
    WHERE u.IsActive = 1;
END;
```

### Entity Relationships

```
User 1──────────* TaskItem (AssignedTo)
User 1──────────* TaskItem (CreatedBy)
User 1──────────* TaskComment
User 1──────────* UserSession
User 1──────────* Notification
User 1──────────* AiInteractionLog

TaskItem 1──────* TaskComment
TaskItem 1──────* TaskAuditLog
TaskItem 1──────* Notification
TaskItem 1──────1 TaskEmbedding
TaskItem 1──────* TaskItem (Subtasks via ParentTaskId)
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*


### Property Reflection

After analyzing all acceptance criteria, I've identified the following areas where properties can be consolidated:

- Authentication properties (1.1-1.8) cover token generation, rotation, and validation - these are distinct and valuable
- Authorization properties (2.1-2.6) cover role-based access - can be consolidated into fewer comprehensive properties
- Task CRUD properties (3.1-3.8) cover creation, updates, soft deletes, and audit logging - each provides unique validation
- Filtering properties (4.1-4.8) all follow the same pattern but test different fields - can be consolidated
- Comment properties (5.1-5.6) cover CRUD and sentiment - these are distinct
- Real-time properties (6.1-6.6) are mostly integration-level, keeping only authentication properties
- Dashboard properties (7.1-7.5) cover different metrics - these are distinct
- AI feature properties (8.1-14.5) each test different AI capabilities - these are distinct

Consolidated approach:
- Combine similar filtering properties into a single comprehensive filter property
- Combine role-based access properties into fewer comprehensive authorization properties
- Keep all AI feature properties as they test distinct capabilities
- Keep all audit and concurrency properties as they test critical correctness guarantees

### Correctness Properties

Property 1: User registration with valid data creates account
*For any* valid user registration data (unique email, valid password), the system should create a user account with hashed password and return success
**Validates: Requirements 1.1**

Property 2: Token generation produces valid JWT structure
*For any* successful login, the system should generate an access token with 15-minute expiration and a refresh token with 7-day expiration, both with valid JWT structure
**Validates: Requirements 1.3**

Property 3: Refresh token rotation invalidates old tokens
*For any* valid refresh token, using it to obtain a new access token should invalidate the old refresh token, preventing its reuse
**Validates: Requirements 1.4**

Property 4: Logout revokes refresh tokens
*For any* active user session, logging out should revoke the refresh token and prevent subsequent use
**Validates: Requirements 1.5**

Property 5: Expired tokens are rejected
*For any* expired access token, API requests should be rejected with 401 Unauthorized status
**Validates: Requirements 1.6**

Property 6: Refresh tokens are stored as hashes
*For any* refresh token stored in the database, it should be stored as a SHA-256 hash, not in plain text
**Validates: Requirements 1.8**

Property 7: Role assignment updates permissions
*For any* user and any valid role, when a SuperAdmin assigns the role, the user's permissions should immediately reflect the new role
**Validates: Requirements 2.1**

Property 8: Members cannot access other users' tasks
*For any* Member user and any task not assigned to them, attempts to access the task should be rejected with 403 Forbidden
**Validates: Requirements 2.2**

Property 9: Admins see only team tasks
*For any* Admin user, task queries should return only tasks belonging to their team members, never tasks from other teams
**Validates: Requirements 2.3**

Property 10: SuperAdmins see all tasks
*For any* SuperAdmin user, task queries should return all tasks across all users without restriction
**Validates: Requirements 2.4**

Property 11: Non-SuperAdmins cannot change roles
*For any* non-SuperAdmin user, attempts to change user roles should be rejected with 403 Forbidden
**Validates: Requirements 2.5**

Property 12: Task creation assigns unique identifier
*For any* valid task data, creating a task should persist it with a unique GUID identifier and return the created task
**Validates: Requirements 3.1**

Property 13: Task updates modify timestamp
*For any* task update, the UpdatedAt timestamp should be greater than the previous UpdatedAt value
**Validates: Requirements 3.2**

Property 14: Completing tasks sets completion timestamp
*For any* task status change to Completed, the CompletedAt timestamp should be set to the current time
**Validates: Requirements 3.3**

Property 15: Task deletion is soft delete
*For any* task deletion, the database record should remain with IsDeleted set to true, and the task should not appear in subsequent queries
**Validates: Requirements 3.4, 3.5**

Property 16: Task field changes create audit log entries
*For any* task field modification, an entry should be created in TaskAuditLog with the field name, old value, new value, timestamp, and user identifier
**Validates: Requirements 3.8**

Property 17: Invalid task data is rejected
*For any* task data that violates validation rules (e.g., title exceeds 200 characters, invalid enum values), the system should reject the request before persistence
**Validates: Requirements 3.7**

Property 18: Task filters return matching results
*For any* task query with filters (status, priority, category, assignee, due date range), all returned tasks should match the specified filter criteria
**Validates: Requirements 4.1, 4.2, 4.3, 4.4, 4.5**

Property 19: Search returns matching tasks
*For any* search term, all returned tasks should contain the term in either the title or tags (case-insensitive)
**Validates: Requirements 4.6**

Property 20: Pagination respects page size limits
*For any* page request, the returned results should not exceed the specified page size, and should never exceed 100 items regardless of requested size
**Validates: Requirements 4.7, 4.8**

Property 21: Comments are persisted with metadata
*For any* comment posted on a task, the comment should be persisted with the correct TaskItemId, UserId, Content, and CreatedAt timestamp
**Validates: Requirements 5.1**

Property 22: Comment queries exclude deleted comments
*For any* task, requesting comments should return only comments where IsDeleted is false, ordered by CreatedAt ascending
**Validates: Requirements 5.2**

Property 23: Comment deletion is soft delete
*For any* user's own comment, deletion should set IsDeleted to true without removing the database record
**Validates: Requirements 5.3**

Property 24: Sentiment scores are within valid range
*For any* comment with sentiment analysis completed, the SentimentScore should be between 0.0 and 1.0 inclusive
**Validates: Requirements 14.2**

Property 25: Task details include average sentiment
*For any* task with comments that have sentiment scores, the task details should include the correctly calculated average sentiment
**Validates: Requirements 5.5, 14.3**

Property 26: Non-admins cannot delete others' comments
*For any* Member user, attempts to delete comments created by other users should be rejected with 403 Forbidden
**Validates: Requirements 5.6**

Property 27: SignalR connections require authentication
*For any* SignalR connection attempt, the connection should only be established if a valid JWT token is provided
**Validates: Requirements 6.5**

Property 28: Authenticated connections join user groups
*For any* authenticated SignalR connection, the connection should be added to a group identified by the user's ID
**Validates: Requirements 6.6**

Property 29: Dashboard summary reflects actual task distribution
*For any* user, the dashboard summary counts should match the actual number of tasks in each status category
**Validates: Requirements 7.1**

Property 30: Velocity data reflects completion history
*For any* user and any time period, the velocity data should accurately reflect the number of tasks completed per day
**Validates: Requirements 7.2**

Property 31: Overdue tasks meet criteria
*For any* overdue task query, all returned tasks should have DueDate before current time and Status not equal to Completed
**Validates: Requirements 7.5**

Property 32: Natural language parsing extracts structured fields
*For any* natural language input to the parse-task endpoint, the AI should attempt to extract title, description, priority, category, assignee email, and due date, returning null for fields that cannot be parsed
**Validates: Requirements 8.1, 8.2, 8.3**

Property 33: AI interactions are logged
*For any* AI feature invocation, an entry should be created in AiInteractionLog with UserId, FeatureType, InputSummary, OutputSummary, TokensUsed, LatencyMs, and CreatedAt
**Validates: Requirements 8.6, 9.6, 11.6, 12.7, 13.7**

Property 34: Task decomposition generates limited subtasks
*For any* task decomposition request, the AI should generate between 0 and 8 subtask suggestions, each with a title and estimated hours
**Validates: Requirements 9.1, 9.2**

Property 35: Accepted subtasks link to parent
*For any* accepted subtask suggestion, the created TaskItem should have ParentTaskId set to the original task's ID
**Validates: Requirements 9.3**

Property 36: Subtask queries return all children
*For any* task, requesting subtasks should return all TaskItems where ParentTaskId equals the task's ID
**Validates: Requirements 9.4**

Property 37: Digests include required components
*For any* generated digest, it should include tasks due today, overdue tasks, priority recommendations, and an encouraging message
**Validates: Requirements 10.2**

Property 38: Digests are stored as notifications
*For any* generated digest, a Notification should be created with Type set to AiSuggestion
**Validates: Requirements 10.3**

Property 39: Digest queries return most recent
*For any* user requesting their digest, the system should return the most recent digest notification
**Validates: Requirements 10.5**

Property 40: Workload suggestions include required fields
*For any* workload balancing suggestion, it should include TaskId, SuggestedAssigneeId, and Reason
**Validates: Requirements 11.2**

Property 41: Accepted suggestions reassign tasks
*For any* accepted workload suggestion, the specified task should be reassigned to the suggested user
**Validates: Requirements 11.3**

Property 42: Members cannot request workload suggestions
*For any* Member user, requests to the workload suggestions endpoint should be rejected with 403 Forbidden
**Validates: Requirements 11.4**

Property 43: Semantic search generates embeddings
*For any* semantic search query, the system should generate an embedding vector for the query
**Validates: Requirements 12.2**

Property 44: Search results are ranked by relevance
*For any* search query (standard or semantic), results should be ordered by relevance score in descending order
**Validates: Requirements 12.4**

Property 45: Task changes regenerate embeddings
*For any* task where title or description is modified, the embedding vector in TaskEmbedding table should be regenerated
**Validates: Requirements 12.5**

Property 46: CSV import maps columns
*For any* CSV file upload, the AI should analyze headers and map them to internal schema fields (Title, Description, Priority, Category, etc.)
**Validates: Requirements 13.1**

Property 47: Import normalizes non-standard values
*For any* CSV with non-standard enum values, the AI should normalize them to valid Priority, Status, and Category enum values
**Validates: Requirements 13.2**

Property 48: Confirmed imports create tasks
*For any* confirmed import with N rows, the system should create N TaskItems
**Validates: Requirements 13.4**

Property 49: Invalid import data returns errors
*For any* CSV with invalid data that cannot be normalized, the system should return validation errors identifying the problematic row numbers
**Validates: Requirements 13.5**

Property 50: Comment sentiment analysis is asynchronous
*For any* comment creation, the sentiment analysis should not block the comment creation response
**Validates: Requirements 5.4, 14.1**

## Error Handling

### Error Categories and HTTP Status Codes

The system maps domain exceptions to appropriate HTTP status codes:

**400 Bad Request**
- ValidationException: FluentValidation failures with field-level error details
- InvalidOperationException: Business rule violations (e.g., cannot complete a cancelled task)
- FormatException: Malformed input data

**401 Unauthorized**
- TokenExpiredException: Access token has expired
- InvalidTokenException: Token signature invalid or token malformed
- MissingTokenException: No authorization header provided

**403 Forbidden**
- InsufficientPermissionsException: User lacks required role for operation
- ResourceAccessDeniedException: User cannot access resource due to ownership rules

**404 Not Found**
- EntityNotFoundException: Requested entity does not exist or is soft-deleted

**409 Conflict**
- DbUpdateConcurrencyException: Optimistic concurrency violation (RowVersion mismatch)
- DuplicateEntityException: Unique constraint violation (e.g., duplicate email)

**500 Internal Server Error**
- All unhandled exceptions
- Database connection failures
- AI service failures after retry exhaustion

### Global Exception Handler

The GlobalExceptionHandler implements IExceptionHandler and processes exceptions in this order:

1. Check if exception is a known domain exception → map to appropriate status code
2. Log exception with Serilog at Error level including:
   - Full stack trace
   - Correlation ID from HTTP context
   - User ID from JWT claims (if authenticated)
   - Request path and method
3. Return ProblemDetails response with:
   - Status code
   - Title (exception type name)
   - Detail (user-friendly message, never stack trace)
   - Instance (correlation ID for tracing)
   - Extensions (field-level errors for validation failures)

### Retry Policies with Polly

AI service calls use Polly retry policies:

```csharp
// Handles transient AI API failures which happen more often than you might expect,
// especially during peak hours or when the model is being updated.
// Three attempts with exponential backoff (1s, 2s, 4s) has worked well in practice.
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            _logger.LogWarning(
                "AI service call failed. Retry {RetryCount} after {Delay}ms. Exception: {Exception}",
                retryCount, timeSpan.TotalMilliseconds, exception.Message);
        });
```

### Validation Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["Title is required", "Title must not exceed 200 characters"],
    "Priority": ["Priority must be a valid enum value"]
  },
  "traceId": "00-abc123-def456-00"
}
```

### Concurrency Conflict Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Concurrency conflict",
  "status": 409,
  "detail": "The task was modified by another user. Please refresh and try again.",
  "currentValues": {
    "title": "Updated title from other user",
    "status": "InProgress",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  "traceId": "00-abc123-def456-00"
}
```

## Testing Strategy

### Dual Testing Approach

The system uses both unit testing and property-based testing to ensure comprehensive correctness:

**Unit Tests** verify:
- Specific examples that demonstrate correct behavior
- Edge cases (empty inputs, boundary values, null handling)
- Error conditions and exception handling
- Integration points between components
- Authentication and authorization flows

**Property-Based Tests** verify:
- Universal properties that should hold across all inputs
- Invariants that must be maintained (e.g., soft delete never removes records)
- Round-trip properties (e.g., token refresh rotation)
- Metamorphic properties (e.g., filtering reduces result set size)
- Business rules that apply to all entities (e.g., audit logging on all changes)

Together, unit tests catch concrete bugs while property tests verify general correctness.

### Property-Based Testing Configuration

**Library**: The system uses **FsCheck** for .NET property-based testing (NuGet package: FsCheck.Xunit)

**Configuration**: Each property-based test runs a minimum of 100 iterations to ensure statistical confidence in the property validation

**Tagging**: Each property-based test includes a comment explicitly referencing the correctness property from this design document using the format:
```csharp
// Feature: velocify-platform, Property 15: Task deletion is soft delete
// Validates: Requirements 3.4, 3.5
[Property(MaxTest = 100)]
public Property TaskDeletion_ShouldBeSoftDelete()
{
    // Property test implementation
}
```

### Unit Testing Strategy

**Framework**: xUnit with Moq for mocking and FluentAssertions for readable assertions

**Test Database**: In-memory SQLite provider for repository tests to ensure isolation

**Coverage Goals**:
- All MediatR command and query handlers
- All repository methods
- Authentication service (registration, login, token refresh, logout)
- Authorization logic (role-based access control)
- AI service interfaces (with mocked LangChain calls)
- Exception handling middleware
- SignalR hub methods

**Test Organization**:
- One test class per handler/service
- Descriptive test names following pattern: MethodName_Scenario_ExpectedBehavior
- Arrange-Act-Assert structure
- Each test class has a comment describing its purpose and covered edge cases

### Example Unit Tests

**AuthServiceTests**: Tests registration with duplicate email returns conflict, login with wrong password returns unauthorized, refresh token rotation invalidates old token, logout revokes token

**CreateTaskCommandHandlerTests**: Tests task creation with valid data, validation failures for invalid data, default values assignment, audit log creation

**UpdateTaskCommandHandlerTests**: Tests successful update, optimistic concurrency conflict handling, UpdatedAt timestamp modification, status change to Completed sets CompletedAt

**GetTaskListQueryHandlerTests**: Tests filtering by status/priority/category, pagination, search functionality, role-based access control (Member sees only own tasks, Admin sees team tasks, SuperAdmin sees all)

**DashboardQueryHandlerTests**: Tests counts match seeded data, Admin sees team metrics, Member sees only own metrics, indexed view is queried

**NaturalLanguageTaskServiceTests**: Tests successful parsing returns structured command, partial parsing returns nulls for missing fields, retry policy triggers on failure, AI interaction is logged

**WorkloadBalancingServiceTests**: Tests suggestions are generated for imbalanced teams, suggestions include required fields, Member access is denied

**ExceptionHandlingMiddlewareTests**: Tests unhandled exception returns 500 with ProblemDetails, stack trace is not exposed, exception is logged with correlation ID

### Example Property-Based Tests

**Property Test 1: Task deletion is soft delete**
```csharp
// Feature: velocify-platform, Property 15: Task deletion is soft delete
// Validates: Requirements 3.4, 3.5
[Property(MaxTest = 100)]
public Property TaskDeletion_ShouldBeSoftDelete()
{
    return Prop.ForAll(
        Arb.From<TaskItem>(),
        task =>
        {
            // Arrange: Create task in database
            var createdTask = _repository.Create(task);
            
            // Act: Delete task
            _repository.Delete(createdTask.Id);
            
            // Assert: Record exists with IsDeleted = true
            var deletedTask = _dbContext.TaskItems.IgnoreQueryFilters()
                .FirstOrDefault(t => t.Id == createdTask.Id);
            
            return deletedTask != null && 
                   deletedTask.IsDeleted == true &&
                   !_repository.GetById(createdTask.Id).HasValue; // Not in normal queries
        });
}
```

**Property Test 2: Refresh token rotation invalidates old tokens**
```csharp
// Feature: velocify-platform, Property 3: Refresh token rotation invalidates old tokens
// Validates: Requirements 1.4
[Property(MaxTest = 100)]
public Property RefreshToken_ShouldInvalidateOldToken()
{
    return Prop.ForAll(
        Arb.From<User>(),
        user =>
        {
            // Arrange: Create user and generate initial tokens
            var loginResult = _authService.Login(user.Email, "password");
            var oldRefreshToken = loginResult.RefreshToken;
            
            // Act: Use refresh token to get new tokens
            var refreshResult = _authService.RefreshToken(oldRefreshToken);
            
            // Assert: Old token is invalidated
            var exception = Record.Exception(() => 
                _authService.RefreshToken(oldRefreshToken));
            
            return exception is InvalidTokenException &&
                   refreshResult.AccessToken != loginResult.AccessToken &&
                   refreshResult.RefreshToken != oldRefreshToken;
        });
}
```

**Property Test 3: Task filters return matching results**
```csharp
// Feature: velocify-platform, Property 18: Task filters return matching results
// Validates: Requirements 4.1, 4.2, 4.3, 4.4, 4.5
[Property(MaxTest = 100)]
public Property TaskFilters_ShouldReturnMatchingResults()
{
    return Prop.ForAll(
        Arb.From<TaskStatus>(),
        Arb.From<TaskPriority>(),
        (status, priority) =>
        {
            // Arrange: Seed database with random tasks
            var tasks = GenerateRandomTasks(50);
            _dbContext.TaskItems.AddRange(tasks);
            _dbContext.SaveChanges();
            
            // Act: Query with filters
            var query = new GetTaskListQuery 
            { 
                Status = status, 
                Priority = priority 
            };
            var results = _handler.Handle(query, CancellationToken.None).Result;
            
            // Assert: All results match filter criteria
            return results.Items.All(t => 
                t.Status == status && t.Priority == priority);
        });
}
```

**Property Test 4: AI interactions are logged**
```csharp
// Feature: velocify-platform, Property 33: AI interactions are logged
// Validates: Requirements 8.6, 9.6, 11.6, 12.7, 13.7
[Property(MaxTest = 100)]
public Property AiInteractions_ShouldBeLogged()
{
    return Prop.ForAll(
        Arb.From<string>(), // Natural language input
        Arb.From<AiFeatureType>(),
        (input, featureType) =>
        {
            // Arrange: Mock AI service
            var userId = Guid.NewGuid();
            
            // Act: Call AI service
            var startTime = DateTime.UtcNow;
            _aiService.ProcessRequest(userId, featureType, input);
            
            // Assert: Log entry created
            var logEntry = _dbContext.AiInteractionLog
                .FirstOrDefault(l => l.UserId == userId && 
                                     l.FeatureType == featureType &&
                                     l.CreatedAt >= startTime);
            
            return logEntry != null &&
                   !string.IsNullOrEmpty(logEntry.InputSummary) &&
                   !string.IsNullOrEmpty(logEntry.OutputSummary) &&
                   logEntry.LatencyMs >= 0;
        });
}
```

### Integration Testing

While unit and property tests cover business logic, integration tests verify:
- End-to-end API flows (register → login → create task → update task → delete task)
- SignalR hub connectivity and event broadcasting
- Database migrations and schema correctness
- Health check endpoints
- Authentication middleware
- CORS configuration

Integration tests use TestServer from Microsoft.AspNetCore.Mvc.Testing and a real SQL Server test database (or LocalDB).

### Performance Testing

Performance tests verify the database optimizations:
- Indexed view query performance vs. raw aggregation
- Compiled query performance vs. standard LINQ
- Filtered index performance for soft delete queries
- Partition pruning for audit log queries
- Connection pooling under load

Performance tests use BenchmarkDotNet and assert that P95 latency remains under 500ms for critical endpoints.

### Test Execution

Tests run in CI/CD pipeline on every push to develop and main branches:
1. Unit tests (fast, run first)
2. Property-based tests (slower due to 100 iterations each)
3. Integration tests (require database)
4. SonarQube analysis (code quality and coverage)

All tests must pass before merge to main branch.


## Frontend Architecture

### Overview

The frontend is a React 18 single-page application built with Vite, following a feature-sliced architecture pattern. Each feature is self-contained with its own components, hooks, and API services. The application uses Zustand for client state (auth, notifications) and TanStack Query for server state management with automatic caching and background refetching.

### Technology Stack

- React 18 with Vite (fast build tool and dev server)
- React Router v6 (client-side routing with lazy loading)
- Zustand (lightweight state management for auth and notifications)
- TanStack Query (server state, caching, automatic refetching)
- Axios (HTTP client with interceptors)
- React Hook Form + Zod (form validation)
- Tailwind CSS (utility-first styling)
- Recharts (data visualization)
- @microsoft/signalr (real-time WebSocket connection)

### Folder Structure

```
frontend/
├── src/
│   ├── api/
│   │   ├── axios.ts                 (Axios instance with interceptors)
│   │   ├── auth.service.ts          (Auth API calls)
│   │   ├── tasks.service.ts         (Task API calls)
│   │   ├── dashboard.service.ts     (Dashboard API calls)
│   │   ├── ai.service.ts            (AI feature API calls)
│   │   └── notifications.service.ts (Notification API calls)
│   ├── components/
│   │   ├── ui/                      (Reusable UI components)
│   │   ├── layout/                  (Layout components: Header, Sidebar, Footer)
│   │   └── common/                  (Common components: Loading, Error, Toast)
│   ├── features/
│   │   ├── auth/
│   │   │   ├── components/          (LoginForm, RegisterForm)
│   │   │   ├── hooks/               (useAuth, useLogin, useRegister)
│   │   │   └── store/               (authStore.ts - Zustand)
│   │   ├── tasks/
│   │   │   ├── components/          (TaskList, TaskCard, TaskForm, TaskFilters)
│   │   │   ├── hooks/               (useTasks, useTaskMutations)
│   │   │   └── types/               (Task types)
│   │   ├── dashboard/
│   │   │   ├── components/          (StatCard, VelocityChart, WorkloadChart)
│   │   │   └── hooks/               (useDashboard, useVelocity)
│   │   ├── ai/
│   │   │   ├── components/          (AiAssistantDrawer, NaturalLanguageInput, SemanticSearch)
│   │   │   └── hooks/               (useAiParse, useAiDecompose, useSemanticSearch)
│   │   └── notifications/
│   │       ├── components/          (NotificationsPanel, NotificationItem)
│   │       ├── hooks/               (useNotifications, useSignalR)
│   │       └── store/               (notificationStore.ts - Zustand)
│   ├── hooks/
│   │   ├── useDebounce.ts           (Debounce hook for search)
│   │   ├── useInfiniteScroll.ts     (Infinite scroll pagination)
│   │   └── useToast.ts              (Toast notifications)
│   ├── pages/
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── DashboardPage.tsx
│   │   ├── TaskListPage.tsx
│   │   ├── TaskDetailPage.tsx
│   │   ├── TaskFormPage.tsx
│   │   ├── UserProfilePage.tsx
│   │   ├── AdminPage.tsx
│   │   └── NotFoundPage.tsx
│   ├── store/
│   │   ├── authStore.ts             (Zustand: user, tokens, login, logout)
│   │   └── notificationStore.ts     (Zustand: unread count, notification list)
│   ├── utils/
│   │   ├── constants.ts             (API URLs, enum mappings)
│   │   ├── formatters.ts            (Date, number, status formatters)
│   │   ├── validators.ts            (Zod schemas)
│   │   └── helpers.ts               (Utility functions)
│   ├── routes/
│   │   ├── index.tsx                (Route configuration)
│   │   └── PrivateRoute.tsx         (Auth guard component)
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── public/
│   └── index.html
├── vite.config.ts
├── tailwind.config.js
├── package.json
└── vercel.json
```

### State Management Strategy

**Zustand (Client State)**
- Auth state: user object, access token (memory only), role, isAuthenticated
- Notification state: unread count, notification list for UI display
- Persisted to sessionStorage for auth state (except tokens)

**TanStack Query (Server State)**
- All API data: tasks, dashboard metrics, user lists, AI results
- Automatic caching with stale-while-revalidate strategy
- Background refetching on window focus
- Optimistic updates for mutations
- Query invalidation on SignalR events

**Why this split?**
- Zustand handles ephemeral UI state that doesn't come from the server
- TanStack Query handles all server data with built-in caching, refetching, and synchronization
- This prevents state duplication and keeps server data fresh

### Routing and Navigation

**Route Configuration**
```tsx
const routes = [
  { path: '/login', element: <LoginPage />, public: true },
  { path: '/register', element: <RegisterPage />, public: true },
  { path: '/', element: <DashboardPage />, roles: ['Member', 'Admin', 'SuperAdmin'] },
  { path: '/tasks', element: <TaskListPage />, roles: ['Member', 'Admin', 'SuperAdmin'] },
  { path: '/tasks/:id', element: <TaskDetailPage />, roles: ['Member', 'Admin', 'SuperAdmin'] },
  { path: '/tasks/new', element: <TaskFormPage />, roles: ['Member', 'Admin', 'SuperAdmin'] },
  { path: '/profile', element: <UserProfilePage />, roles: ['Member', 'Admin', 'SuperAdmin'] },
  { path: '/admin', element: <AdminPage />, roles: ['Admin', 'SuperAdmin'] },
];
```

**PrivateRoute Component**
- Checks authStore.isAuthenticated
- Redirects to /login if not authenticated
- Checks user role against route.roles
- Shows AccessDenied component if insufficient permissions
- Lazy loads page components with React.lazy and Suspense

### API Layer

**Axios Configuration**
```typescript
// Request interceptor: adds Authorization header with access token
axios.interceptors.request.use(config => {
  const token = authStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor: handles token refresh on 401
axios.interceptors.response.use(
  response => response,
  async error => {
    if (error.response?.status === 401 && !error.config._retry) {
      error.config._retry = true;
      try {
        await authService.refreshToken();
        return axios(error.config);
      } catch {
        authStore.getState().logout();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);
```

**Service Modules**
Each service module exports functions that return Axios promises:
- authService: register, login, refreshToken, logout
- tasksService: getTasks, getTaskById, createTask, updateTask, deleteTask, getComments, createComment
- dashboardService: getSummary, getVelocity, getWorkload
- aiService: parseTask, decomposeTask, searchTasks, getWorkloadSuggestions
- notificationsService: getNotifications, markAsRead, markAllAsRead

### TanStack Query Configuration

**Global Configuration**
```typescript
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30000, // 30 seconds
      cacheTime: 300000, // 5 minutes
      refetchOnWindowFocus: true,
      retry: 1,
    },
    mutations: {
      onError: (error) => {
        if (error.response?.status === 403) {
          toast.error('You do not have permission to perform this action');
        } else if (error.response?.status >= 500) {
          toast.error('Server error. Please try again later.');
        }
      },
    },
  },
});
```

**Query Keys Convention**
```typescript
// Hierarchical query keys for easy invalidation
const queryKeys = {
  tasks: ['tasks'],
  taskList: (filters) => ['tasks', 'list', filters],
  taskDetail: (id) => ['tasks', 'detail', id],
  taskComments: (id) => ['tasks', id, 'comments'],
  dashboard: ['dashboard'],
  dashboardSummary: ['dashboard', 'summary'],
  dashboardVelocity: ['dashboard', 'velocity'],
};
```

**Example Query Hook**
```typescript
export function useTasks(filters) {
  return useQuery({
    queryKey: queryKeys.taskList(filters),
    queryFn: () => tasksService.getTasks(filters),
    keepPreviousData: true, // For pagination
  });
}
```

**Example Mutation Hook**
```typescript
export function useCreateTask() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: tasksService.createTask,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks });
      toast.success('Task created successfully');
    },
  });
}
```

### SignalR Integration

**Connection Management**
```typescript
// Establish connection on login
useEffect(() => {
  if (authStore.isAuthenticated) {
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/tasks`, {
        accessTokenFactory: () => authStore.getState().accessToken,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0s, 2s, 10s, 30s, then 30s
          return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
        },
      })
      .build();

    connection.start();
    
    // Event handlers
    connection.on('TaskAssigned', (task) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks });
      toast.info(`New task assigned: ${task.title}`);
    });
    
    connection.on('StatusChanged', (taskId, newStatus) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.taskDetail(taskId) });
    });
    
    connection.on('CommentAdded', (taskId, comment) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.taskComments(taskId) });
    });
    
    connection.on('AiSuggestionReady', (notification) => {
      notificationStore.getState().addNotification(notification);
    });

    return () => connection.stop();
  }
}, [authStore.isAuthenticated]);
```

### Page Components

**DashboardPage**
- Four stat cards showing task counts by status (Pending, InProgress, Completed, Blocked)
- Line chart (Recharts LineChart) showing task completion velocity over last 30 days
- Bar chart (Recharts BarChart) showing tasks by priority
- AI digest card displaying today's personalized digest
- Overdue tasks alert section with count and link to filtered task list
- Admin users see additional donut chart (Recharts PieChart) showing workload distribution

**TaskListPage**
- Filter panel: Status (multi-select), Priority (multi-select), Category (multi-select), Assignee (searchable dropdown), Due Date range (date pickers)
- Search input with 300ms debounce
- Semantic search toggle that switches between standard and AI-powered search
- Task cards showing: title, priority badge (color-coded), status badge, assignee avatar, due date (red if overdue, orange if due today), AI completion probability badge
- Pagination controls or infinite scroll
- Admin users see bulk action toolbar: change status, reassign, delete (appears when tasks selected)

**TaskDetailPage**
- Task information panel: all fields editable inline
- Status dropdown with immediate update on change
- Comment thread with sentiment indicators (emoji or color) per comment
- Comment input with real-time sentiment preview
- Audit history timeline showing all field changes with timestamps and user names
- Subtasks section with add/complete/remove actions
- AI decomposition button that opens modal with suggested subtasks

**TaskFormPage (New/Edit)**
- Two modes: Natural Language and Manual Form
- Natural Language mode: textarea + Parse button → fills form fields automatically
- Manual Form mode: all fields with validation
- Fields: Title (text), Description (textarea), Assignee (searchable user dropdown), Priority (select), Category (select), Due Date (date picker), Estimated Hours (number), Tags (multi-input chip field)
- Form validation with React Hook Form + Zod
- Submit button disabled until validation passes

**UserProfilePage**
- User info card: name, email, role, account creation date
- Productivity score gauge chart (Recharts RadialBarChart) showing score as percentage
- Velocity line chart showing tasks completed per week for last 12 weeks
- Edit profile form (name, email, password change)
- Logout button

**AdminPage**
- User management table: columns for name, email, role, status, actions
- Role assignment dropdown (SuperAdmin only)
- Workload balancing panel: AI suggestions with task details, suggested assignee, reason, Accept button
- System metrics cards: total tasks, active users, AI feature usage counts
- AI adoption metrics: charts showing natural language task creation rate, semantic search usage, average AI latency

**NotificationsPanel**
- Slide-in drawer from right side
- Notification list ordered by creation time (newest first)
- Each notification shows: icon (based on type), message, timestamp, read/unread indicator
- Mark as read button per notification
- Mark all as read button at top
- Real-time updates via SignalR (new notifications appear instantly)
- Unread count badge on notification bell icon in header

**AiAssistantDrawer**
- Slide-in panel from right side
- Floating action button (AI icon) accessible from all pages
- Natural language task input with Parse button
- Semantic search input
- Today's digest display
- Quick actions: decompose current task, get workload suggestions (admin only)

### Form Validation with Zod

**Example Task Form Schema**
```typescript
const taskFormSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
  description: z.string().optional(),
  priority: z.enum(['Critical', 'High', 'Medium', 'Low']),
  category: z.enum(['Development', 'Design', 'Marketing', 'Operations', 'Research', 'Other']),
  assignedToUserId: z.string().uuid('Invalid user ID'),
  dueDate: z.date().optional(),
  estimatedHours: z.number().min(0).max(999).optional(),
  tags: z.array(z.string()).optional(),
});
```

### Styling with Tailwind CSS

**Design System**
- Color palette: Primary (blue), Success (green), Warning (orange), Danger (red), Neutral (gray)
- Typography: Inter font family, responsive font sizes
- Spacing: 4px base unit (space-1 = 4px, space-2 = 8px, etc.)
- Components: consistent border radius (rounded-lg), shadows (shadow-md), transitions

**Priority Badge Colors**
- Critical: bg-red-100 text-red-800 border-red-200
- High: bg-orange-100 text-orange-800 border-orange-200
- Medium: bg-yellow-100 text-yellow-800 border-yellow-200
- Low: bg-gray-100 text-gray-800 border-gray-200

**Status Badge Colors**
- Pending: bg-gray-100 text-gray-800
- InProgress: bg-blue-100 text-blue-800
- Completed: bg-green-100 text-green-800
- Cancelled: bg-red-100 text-red-800
- Blocked: bg-orange-100 text-orange-800

### Performance Optimizations

**Code Splitting**
- Lazy load all page components with React.lazy
- Suspense boundaries with loading spinners
- Route-based code splitting reduces initial bundle size

**Memoization**
- Use React.memo for expensive list item components
- Use useMemo for expensive computations (filtering, sorting)
- Use useCallback for event handlers passed to child components

**Debouncing**
- Search input debounced to 300ms to reduce API calls
- Filter changes debounced to 500ms

**Infinite Scroll**
- Task list uses infinite scroll instead of traditional pagination
- Loads 20 items at a time
- TanStack Query's useInfiniteQuery handles page management

**Image Optimization**
- User avatars lazy loaded with loading="lazy"
- Avatar images served from CDN with appropriate sizes

### Error Handling

**Global Error Boundary**
- Catches React rendering errors
- Displays user-friendly error page
- Logs error to console (could integrate with error tracking service)

**API Error Handling**
- 401: Attempt token refresh, redirect to login if refresh fails
- 403: Show toast "You do not have permission"
- 404: Show "Resource not found" message
- 409: Show conflict message with server values for user to resolve
- 5xx: Show "Server error, please try again" toast

**Form Validation Errors**
- Field-level errors displayed below each input
- Error messages from Zod schemas
- Backend validation errors mapped to form fields

### Accessibility

**Keyboard Navigation**
- All interactive elements accessible via Tab key
- Modal dialogs trap focus
- Escape key closes modals and drawers

**ARIA Labels**
- Buttons have aria-label when icon-only
- Form inputs have associated labels
- Status badges have aria-label describing status

**Color Contrast**
- All text meets WCAG AA contrast requirements
- Status indicators use both color and text/icons

**Screen Reader Support**
- Semantic HTML elements (nav, main, article, aside)
- Live regions for toast notifications (aria-live="polite")
- Loading states announced to screen readers

### Environment Configuration

**Environment Variables**
```
VITE_API_BASE_URL=https://api.velocify.com
VITE_SIGNALR_HUB_URL=https://api.velocify.com/hubs/tasks
```

**Vercel Configuration (vercel.json)**
```json
{
  "rewrites": [
    { "source": "/(.*)", "destination": "/index.html" }
  ],
  "env": {
    "VITE_API_BASE_URL": "@api-base-url",
    "VITE_SIGNALR_HUB_URL": "@signalr-hub-url"
  }
}
```

### Build and Deployment

**Frontend Build Process (Vercel)**
1. `npm run build` - Vite builds optimized production bundle
2. Output to `dist/` directory
3. Vercel deploys `dist/` as static site
4. Environment variables injected at build time

**Backend Deployment (Azure App Service F1 Tier)**
1. GitHub Actions workflow triggers on push to main branch
2. Build ASP.NET Core 8 application
3. Deploy to Azure App Service F1 Tier
4. Configure environment variables in Azure App Service Configuration
5. Run EF Core migrations on startup
6. Health endpoint monitored by Azure App Service

**Azure App Service F1 Tier Considerations**
- Free tier with 60 minutes of CPU time per day
- Optimize code to minimize CPU usage (avoid infinite loops, unnecessary processing)
- Use efficient database queries with compiled queries and indexed views
- Implement proper caching strategies
- Background jobs (digest generation, productivity calculation) should be scheduled efficiently
- Monitor CPU time usage in Azure portal

**Bundle Size Targets**
- Initial bundle: < 200KB gzipped
- Lazy-loaded routes: < 50KB each gzipped
- Total bundle (all routes): < 500KB gzipped

**Browser Support**
- Modern browsers (Chrome, Firefox, Safari, Edge) - last 2 versions
- No IE11 support (uses modern JavaScript features)
