# Implementation Plan

## 1. Project Setup and Configuration

- [x] 1.1 Initialize repository structure
  - Create monorepo folder structure (backend/, frontend/, infrastructure/)
  - Initialize .gitignore with all specified exclusions
  - Create README.md with project overview
  - _Requirements: All_

- [x] 1.2 Install required development tools and SDKs
  - Verify .NET 8 SDK is installed (dotnet --version should show 8.x.x)
  - Verify Node.js 18+ is installed (node --version should show 18.x.x or higher)
  - Install Entity Framework Core CLI tools globally: dotnet tool install --global dotnet-ef
  - Verify Git is installed and configured
  - Install Azure CLI (for deployment): https://aka.ms/installazurecliwindows
  - _Requirements: All backend and frontend requirements_

- [x] 1.3 Setup backend solution structure
  - Create Velocify.sln
  - Create Velocify.Domain project (class library): dotnet new classlib -n Velocify.Domain
  - Create Velocify.Application project (class library): dotnet new classlib -n Velocify.Application
  - Create Velocify.Infrastructure project (class library): dotnet new classlib -n Velocify.Infrastructure
  - Create Velocify.API project (web API): dotnet new webapi -n Velocify.API
  - Create Velocify.Tests project (xUnit test project): dotnet new xunit -n Velocify.Tests
  - Add all projects to solution: dotnet sln add **/*.csproj
  - Configure project references
  - _Requirements: All backend requirements_

- [x] 1.4 Install ALL backend NuGet packages upfront
  - Navigate to backend/ directory
  - **Velocify.Domain** (no external dependencies - pure domain layer)
  - **Velocify.Application** packages:
    - dotnet add Velocify.Application/Velocify.Application.csproj package MediatR --version 12.2.0
    - dotnet add Velocify.Application/Velocify.Application.csproj package FluentValidation --version 11.9.0
    - dotnet add Velocify.Application/Velocify.Application.csproj package FluentValidation.DependencyInjectionExtensions --version 11.9.0
    - dotnet add Velocify.Application/Velocify.Application.csproj package AutoMapper --version 12.0.1
    - dotnet add Velocify.Application/Velocify.Application.csproj package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
  - **Velocify.Infrastructure** packages:
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package Microsoft.EntityFrameworkCore --version 8.0.0
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.0
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package LangChain --version 0.13.0
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package LangChain.Providers.OpenAI --version 0.13.0
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package Polly --version 8.2.0
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package BCrypt.Net-Next --version 4.0.3
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt --version 7.0.3
    - dotnet add Velocify.Infrastructure/Velocify.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
  - **Velocify.API** packages:
    - dotnet add Velocify.API/Velocify.API.csproj package MediatR --version 12.2.0
    - dotnet add Velocify.API/Velocify.API.csproj package Serilog.AspNetCore --version 8.0.0
    - dotnet add Velocify.API/Velocify.API.csproj package Serilog.Sinks.Console --version 5.0.1
    - dotnet add Velocify.API/Velocify.API.csproj package Serilog.Sinks.File --version 5.0.0
    - dotnet add Velocify.API/Velocify.API.csproj package Serilog.Enrichers.Environment --version 2.3.0
    - dotnet add Velocify.API/Velocify.API.csproj package Serilog.Enrichers.Thread --version 3.1.0
    - dotnet add Velocify.API/Velocify.API.csproj package Serilog.Settings.Configuration --version 8.0.0
    - dotnet add Velocify.API/Velocify.API.csproj package Microsoft.AspNetCore.SignalR --version 1.1.0
    - dotnet add Velocify.API/Velocify.API.csproj package Microsoft.AspNetCore.Mvc.Versioning --version 5.1.0
    - dotnet add Velocify.API/Velocify.API.csproj package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer --version 5.1.0
    - dotnet add Velocify.API/Velocify.API.csproj package Swashbuckle.AspNetCore --version 6.5.0
    - dotnet add Velocify.API/Velocify.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
    - dotnet add Velocify.API/Velocify.API.csproj package Microsoft.Extensions.Diagnostics.HealthChecks --version 8.0.0
    - dotnet add Velocify.API/Velocify.API.csproj package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore --version 8.0.0
    - dotnet add Velocify.API/Velocify.API.csproj package AspNetCore.HealthChecks.SqlServer --version 8.0.0
  - **Velocify.Tests** packages:
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package xUnit --version 2.6.2
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package xunit.runner.visualstudio --version 2.5.4
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package Microsoft.NET.Test.Sdk --version 17.8.0
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package Moq --version 4.20.70
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package FluentAssertions --version 6.12.0
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package FsCheck --version 2.16.6
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package FsCheck.Xunit --version 2.16.6
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
    - dotnet add Velocify.Tests/Velocify.Tests.csproj package BenchmarkDotNet --version 0.13.11
  - Verify all packages restored: dotnet restore
  - _Requirements: All backend requirements_

- [x] 1.5 Setup frontend project structure and install ALL packages upfront
  - Navigate to frontend/ directory
  - Initialize Vite + React 18 project: npm create vite@latest . -- --template react-ts
  - Install ALL production dependencies:
    - npm install react@18.2.0 react-dom@18.2.0
    - npm install react-router-dom@6.20.1
    - npm install zustand@4.4.7
    - npm install @tanstack/react-query@5.12.2
    - npm install @tanstack/react-query-devtools@5.12.2
    - npm install axios@1.6.2
    - npm install @microsoft/signalr@8.0.0
    - npm install react-hook-form@7.49.2
    - npm install @hookform/resolvers@3.3.3
    - npm install zod@3.22.4
    - npm install recharts@2.10.3
    - npm install date-fns@2.30.0
    - npm install clsx@2.0.0
    - npm install tailwind-merge@2.1.0
  - Install ALL development dependencies:
    - npm install -D @types/react@18.2.43
    - npm install -D @types/react-dom@18.2.17
    - npm install -D @vitejs/plugin-react@4.2.1
    - npm install -D typescript@5.3.3
    - npm install -D tailwindcss@3.3.6
    - npm install -D postcss@8.4.32
    - npm install -D autoprefixer@10.4.16
    - npm install -D eslint@8.55.0
    - npm install -D @typescript-eslint/eslint-plugin@6.14.0
    - npm install -D @typescript-eslint/parser@6.14.0
    - npm install -D eslint-plugin-react-hooks@4.6.0
    - npm install -D eslint-plugin-react-refresh@0.4.5
    - npm install -D prettier@3.1.1
    - npm install -D eslint-config-prettier@9.1.0
    - npm install -D eslint-plugin-prettier@5.0.1
  - Initialize Tailwind CSS: npx tailwindcss init -p
  - Configure folder structure (api/, components/, features/, hooks/, pages/, store/, utils/)
  - Verify all packages installed: npm list --depth=0
  - _Requirements: 20.1-28.5_

- [x] 1.6 Configure Tailwind CSS and Vite
  - Update tailwind.config.js with custom theme (colors, fonts, spacing)
  - Update vite.config.ts with path aliases and build optimizations
  - Create index.css with Tailwind directives and custom styles
  - _Requirements: 20.1-28.5_

- [x] 1.7 Install additional development tools
  - Install SonarScanner for .NET: dotnet tool install --global dotnet-sonarscanner
  - Install SonarScanner for JavaScript: npm install -g sonarqube-scanner
  - Verify Azure CLI is configured: az --version
  - Install SQL Server Management Studio or Azure Data Studio (for database management)
  - _Requirements: 30.4, 30.5_

- [x] 1.8 Configure deployment files
  - Create vercel.json for frontend
  - Create azure-app-service.yml for backend deployment
  - Create azure-sql-setup.sql bootstrap script
  - Configure Azure App Service deployment settings
  - _Requirements: 29.1-29.6_

- [x] 1.9 Setup CI/CD workflows
  - Create .github/workflows/backend-ci.yml (with Azure App Service deployment)
  - Create .github/workflows/frontend-ci.yml
  - Configure SonarQube analysis in workflows
  - _Requirements: 30.4, 30.5_

- [x] 1.10 Create configuration files
  - Create .gitignore (exclude node_modules/, bin/, obj/, dist/, .env, appsettings.Development.json)
  - Create .editorconfig for consistent code formatting
  - Create .prettierrc for frontend code formatting
  - Create .eslintrc.json for frontend linting rules
  - Create backend/Directory.Build.props for share


## 2. Domain Layer Implementation

- [x] 2.1 Create domain enums
  - UserRole enum (SuperAdmin, Admin, Member)
  - TaskStatus enum (Pending, InProgress, Completed, Cancelled, Blocked)
  - TaskPriority enum (Critical, High, Medium, Low)
  - TaskCategory enum (Development, Design, Marketing, Operations, Research, Other)
  - NotificationType enum (DueSoon, Overdue, Assigned, StatusChanged, AiSuggestion)
  - AiFeatureType enum (TaskCreation, Decomposition, Digest, Prioritization, Search, Import)
  - _Requirements: 1.1-30.7_

- [x] 2.2 Create User entity
  - Properties: Id, FirstName, LastName, Email, PasswordHash, Role, ProductivityScore, IsActive, CreatedAt, UpdatedAt, LastLoginAt
  - Navigation properties: TasksAssigned, TasksCreated, Comments, Sessions, Notifications
  - Business methods: CalculateFullName(), IsInRole(), CanAccessTask()
  - _Requirements: 1.1, 1.2, 2.1-2.6_

- [x] 2.3 Create TaskItem entity
  - Properties: Id, Title, Description, Status, Priority, Category, AssignedToUserId, CreatedByUserId, ParentTaskId, DueDate, CompletedAt, EstimatedHours, ActualHours, Tags, AiPriorityScore, PredictedCompletionProbability, IsDeleted, CreatedAt, UpdatedAt, RowVersion
  - Navigation properties: AssignedTo, CreatedBy, Comments, AuditLogs, Subtasks, ParentTask, Embedding
  - Business methods: IsOverdue(), CanBeEditedBy(), MarkAsCompleted(), SoftDelete()
  - _Requirements: 3.1-3.8, 4.1-4.8, 9.1-9.6_

- [x] 2.4 Create TaskComment entity
  - Properties: Id, TaskItemId, UserId, Content, SentimentScore, CreatedAt, IsDeleted
  - Navigation properties: TaskItem, User
  - Business methods: CanBeDeletedBy()
  - _Requirements: 5.1-5.6, 14.1-14.5_

- [x] 2.5 Create remaining entities
  - TaskAuditLog entity (Id, TaskItemId, ChangedByUserId, FieldName, OldValue, NewValue, ChangedAt)
  - Notification entity (Id, UserId, Type, Message, IsRead, CreatedAt, TaskItemId)
  - UserSession entity (Id, UserId, RefreshToken, ExpiresAt, IsRevoked, CreatedAt, IpAddress)
  - AiInteractionLog entity (Id, UserId, FeatureType, InputSummary, OutputSummary, TokensUsed, LatencyMs, CreatedAt)
  - TaskEmbedding entity (Id, TaskItemId, EmbeddingVector, CreatedAt)
  - _Requirements: 1.4, 1.5, 1.7, 6.1-6.8, 8.6, 12.5, 12.6_


## 3. Application Layer - DTOs and Interfaces

- [x] 3.1 Create DTOs
  - TaskDto, TaskDetailDto, UserDto, UserSummaryDto, AuthResponseDto
  - DashboardSummaryDto, VelocityDataPoint, CommentDto
  - SubtaskSuggestion, WorkloadSuggestion, TaskImportRow
  - PagedResult<T> generic DTO
  - _Requirements: 3.1, 7.1-7.7, 8.1-8.7, 9.1-9.6_

- [x] 3.2 Create Application layer interfaces
  - IAuthService (Register, Login, RefreshToken, Logout, RevokeAllSessions)
  - ITaskRepository (GetById, GetList, Create, Update, Delete, GetSubtasks, GetComments, GetAuditLog)
  - IUserRepository (GetById, GetByEmail, GetList, Create, Update, Delete, GetProductivityHistory)
  - _Requirements: 1.1-1.8, 3.1-3.8, 4.1-4.8_

- [x] 3.3 Create AI service interfaces
  - INaturalLanguageTaskService (ParseTaskFromText)
  - ITaskDecompositionService (DecomposeTask)
  - IDailyDigestService (GenerateDigest)
  - IWorkloadBalancingService (GetSuggestions)
  - ISemanticSearchService (SearchTasks)
  - IAiImportService (NormalizeImport)
  - ICommentSentimentService (AnalyzeSentiment)
  - _Requirements: 8.1-8.7, 9.1-9.6, 10.1-10.7, 11.1-11.6, 12.1-12.7, 13.1-13.7, 14.1-14.5_

- [x] 3.4 Create notification and hub interfaces
  - INotificationService (CreateNotification, GetUserNotifications, MarkAsRead, MarkAllAsRead)
  - ITaskHubService (NotifyTaskAssigned, NotifyStatusChanged, NotifyCommentAdded, NotifyAiSuggestion)
  - _Requirements: 6.1-6.8, 28.1-28.5_


## 4. Application Layer - Commands and Validators

- [x] 4.1 Create authentication commands
  - RegisterUserCommand with RegisterUserCommandHandler
  - LoginCommand with LoginCommandHandler
  - RefreshTokenCommand with RefreshTokenCommandHandler
  - LogoutCommand with LogoutCommandHandler
  - _Requirements: 1.1-1.8_

- [x] 4.2 Create task management commands
  - CreateTaskCommand with CreateTaskCommandHandler
  - UpdateTaskCommand with UpdateTaskCommandHandler
  - UpdateTaskStatusCommand with UpdateTaskStatusCommandHandler
  - DeleteTaskCommand with DeleteTaskCommandHandler
  - _Requirements: 3.1-3.8_

- [x] 4.3 Create comment commands
  - CreateCommentCommand with CreateCommentCommandHandler
  - DeleteCommentCommand with DeleteCommentCommandHandler
  - _Requirements: 5.1-5.6_

- [x] 4.4 Create FluentValidation validators
  - RegisterUserCommandValidator
  - LoginCommandValidator
  - CreateTaskCommandValidator
  - UpdateTaskCommandValidator
  - CreateCommentCommandValidator
  - _Requirements: 3.7, 19.3_


## 5. Application Layer - Queries

- [x] 5.1 Create task queries
  - GetTaskListQuery with GetTaskListQueryHandler
  - GetTaskByIdQuery with GetTaskByIdQueryHandler
  - GetTaskCommentsQuery with GetTaskCommentsQueryHandler
  - GetTaskAuditLogQuery with GetTaskAuditLogQueryHandler
  - GetSubtasksQuery with GetSubtasksQueryHandler
  - _Requirements: 3.1-3.8, 4.1-4.8, 5.1-5.6_

- [x] 5.2 Create dashboard queries
  - GetDashboardSummaryQuery with GetDashboardSummaryQueryHandler
  - GetDashboardVelocityQuery with GetDashboardVelocityQueryHandler
  - GetWorkloadDistributionQuery with GetWorkloadDistributionQueryHandler
  - GetOverdueTasksQuery with GetOverdueTasksQueryHandler
  - _Requirements: 7.1-7.7_

- [x] 5.3 Create user queries
  - GetCurrentUserQuery with GetCurrentUserQueryHandler
  - GetUserByIdQuery with GetUserByIdQueryHandler
  - GetUsersQuery with GetUsersQueryHandler
  - GetUserProductivityQuery with GetUserProductivityQueryHandler
  - _Requirements: 2.1-2.6, 26.1-26.5_

- [x] 5.4 Create notification queries
  - GetNotificationsQuery with GetNotificationsQueryHandler
  - _Requirements: 28.1-28.5_


## 6. Application Layer - MediatR Pipeline Behaviors

- [x] 6.1 Create ValidationBehavior
  - Implement IPipelineBehavior<TRequest, TResponse>
  - Run FluentValidation validators
  - Throw ValidationException if validation fails
  - Add comment explaining pipeline position and order importance
  - _Requirements: 3.7, 19.3_

- [x] 6.2 Create LoggingBehavior
  - Implement IPipelineBehavior<TRequest, TResponse>
  - Log request and response with Serilog
  - Include correlation ID and user ID
  - Add comment explaining pipeline position
  - _Requirements: 16.2, 16.3_

- [x] 6.3 Create PerformanceBehavior
  - Implement IPipelineBehavior<TRequest, TResponse>
  - Measure handler execution time
  - Log warning if exceeds 500ms
  - Add comment explaining pipeline position
  - _Requirements: 16.4_

- [x] 6.4 Configure AutoMapper profiles
  - Create mapping profiles for all entities to DTOs
  - Configure in Application layer
  - _Requirements: All DTO mappings_


## 7. Infrastructure Layer - Database Configuration

- [x] 7.1 Create VelocifyDbContext
  - Configure DbSets for all entities
  - Configure UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
  - Configure connection pooling (Min=2, Max=100)
  - Add comment explaining Cartesian explosion and split queries
  - _Requirements: 15.5, 15.9_

- [x] 7.2 Create entity configurations
  - UserConfiguration (IEntityTypeConfiguration<User>)
  - TaskItemConfiguration (IEntityTypeConfiguration<TaskItem>)
  - TaskCommentConfiguration (IEntityTypeConfiguration<TaskComment>)
  - Configure all relationships, indexes, and constraints
  - Configure RowVersion as concurrency token on TaskItem
  - Configure soft delete global query filters
  - _Requirements: 3.6, 15.1-15.10_

- [x] 7.3 Create entity configurations for remaining entities
  - TaskAuditLogConfiguration
  - NotificationConfiguration
  - UserSessionConfiguration
  - AiInteractionLogConfiguration
  - TaskEmbeddingConfiguration
  - _Requirements: All entity requirements_

- [x] 7.4 Create initial migration
  - Run dotnet ef migrations add InitialCreate
  - Verify all tables, relationships, and basic indexes are created
  - _Requirements: All entity requirements_


## 8. Infrastructure Layer - Database Optimizations

- [x] 8.1 Create optimization migration for filtered indexes
  - Add filtered indexes on TaskItem (AssignedToUserId WHERE IsDeleted = 0)
  - Add filtered indexes on TaskComment (TaskItemId WHERE IsDeleted = 0)
  - Add comments explaining why filtered indexes improve performance
  - _Requirements: 15.1_

- [x] 8.2 Create optimization migration for composite indexes
  - Add composite index on TaskItem (AssignedToUserId, Status, IsDeleted)
  - Add composite index on TaskItem (DueDate, Priority, IsDeleted)
  - Add composite index on TaskItem (CreatedByUserId, CreatedAt DESC, IsDeleted)
  - Add comments explaining query patterns these indexes support
  - _Requirements: 15.2_

- [x] 8.3 Create indexed view migration
  - Create vw_UserTaskSummary view with SCHEMABINDING
  - Create unique clustered index on (UserId, Status)
  - Add comment explaining materialized view benefits
  - _Requirements: 15.3_

- [x] 8.4 Create table partitioning migration
  - Create partition function PF_AuditLog_Monthly
  - Create partition scheme PS_AuditLog_Monthly
  - Alter TaskAuditLog table to use partitioning
  - Add comment explaining audit log growth and partition benefits
  - _Requirements: 15.6_

- [x] 8.5 Create stored procedure for productivity score
  - Create usp_RecalculateUserProductivityScores stored procedure
  - Implement weighted calculation logic
  - Add comment explaining why calculation belongs in database
  - _Requirements: 7.6, 15.10_


## 9. Infrastructure Layer - Repositories and Query Extensions

- [x] 9.1 Create query extension methods
  - Create AsReadOnly() extension method
  - Wrap AsNoTracking().AsSplitQuery()
  - Add comment explaining memory and performance benefits
  - _Requirements: 15.4, 15.9_

- [x] 9.2 Create CompiledQueries static class
  - Create compiled query for GetTaskById
  - Create compiled query for GetDashboardSummary
  - Create compiled query for GetUserByEmail
  - Add comments explaining which endpoints use each query and why compilation matters
  - _Requirements: 15.4_

- [x] 9.3 Create TaskRepository
  - Implement ITaskRepository
  - Use AsReadOnly() for read operations
  - Handle DbUpdateConcurrencyException for updates
  - Record audit log entries on changes
  - Use compiled queries where applicable
  - _Requirements: 3.1-3.8, 4.1-4.8, 15.5_

- [x] 9.4 Create UserRepository
  - Implement IUserRepository
  - Use compiled query for GetByEmail
  - Hash passwords with SHA-256
  - Use AsReadOnly() for read operations
  - _Requirements: 1.1-1.8, 2.1-2.6_

- [x] 9.5 Create NotificationRepository
  - Implement INotificationService
  - CRUD operations for notifications
  - Mark as read functionality
  - _Requirements: 28.1-28.5_


## 10. Infrastructure Layer - Authentication Service

- [x] 10.1 Create AuthService
  - Implement IAuthService
  - Implement Register method with password hashing
  - Implement Login method with JWT generation
  - Implement RefreshToken method with token rotation
  - Implement Logout method with token revocation
  - Implement RevokeAllSessions method
  - Store refresh tokens as SHA-256 hashes
  - Add comments explaining token security
  - _Requirements: 1.1-1.8_

- [x] 10.2 Create JwtTokenService
  - Generate access tokens (15-minute expiration)
  - Generate refresh tokens (7-day expiration)
  - Validate tokens
  - Extract claims from tokens
  - _Requirements: 1.3, 1.4_


## 11. Infrastructure Layer - AI Services

- [x] 11.1 Create NaturalLanguageTaskService
  - Implement INaturalLanguageTaskService
  - Use LangChain structured output parser
  - Implement Polly retry policy (3 retries, exponential backoff)
  - Extract: Title, Description, Priority, Category, AssigneeEmail, DueDate
  - Log to AiInteractionLog with FeatureType.TaskCreation
  - Add comments explaining retry policy
  - _Requirements: 8.1-8.7_

- [x] 11.2 Create TaskDecompositionService
  - Implement ITaskDecompositionService
  - Use LangChain structured output parser
  - Cap subtask generation at 8 items
  - Return Title and EstimatedHours for each subtask
  - Log to AiInteractionLog with FeatureType.Decomposition
  - _Requirements: 9.1-9.6_

- [x] 11.3 Create DailyDigestService
  - Implement IDailyDigestService and IHostedService
  - Run daily at 8 AM
  - Query tasks due today and overdue for each active user
  - Use LangChain summarization chain
  - Create Notification with Type.AiSuggestion
  - Push via SignalR when user connects
  - _Requirements: 10.1-10.7_

- [x] 11.4 Create WorkloadBalancingService
  - Implement IWorkloadBalancingService
  - Analyze task count, productivity scores, due dates
  - Provide structured JSON to LangChain
  - Return TaskId, SuggestedAssigneeId, Reason
  - Log to AiInteractionLog with FeatureType.Prioritization
  - _Requirements: 11.1-11.6_

- [x] 11.5 Create SemanticSearchService
  - Implement ISemanticSearchService
  - Run SQL LIKE search and embedding search in parallel (Task.WhenAll)
  - Generate query embedding with LangChain
  - Compare against TaskEmbedding table (cosine similarity)
  - Merge and rank results by combined score
  - Regenerate embeddings on task title/description change
  - Log to AiInteractionLog with FeatureType.Search
  - _Requirements: 12.1-12.7_

- [x] 11.6 Create AiImportService
  - Implement IAiImportService
  - Analyze CSV headers with LangChain
  - Map non-standard columns to schema fields
  - Normalize enum values
  - Return normalized list for user review
  - Log to AiInteractionLog with FeatureType.Import
  - _Requirements: 13.1-13.7_

- [x] 11.7 Create CommentSentimentService
  - Implement ICommentSentimentService
  - Async analysis (non-blocking)
  - Return score 0.0 (negative) to 1.0 (positive)
  - Store in TaskComment.SentimentScore
  - _Requirements: 14.1-14.5_


## 12. Infrastructure Layer - Background Services

- [x] 12.1 Create ProductivityScoreCalculationService
  - Implement IHostedService
  - Run every 6 hours
  - Call stored procedure usp_RecalculateUserProductivityScores
  - Update User.ProductivityScore for all users
  - Log execution time and affected rows
  - _Requirements: 7.6, 15.10_

- [x] 12.2 Configure dependency injection
  - Register all repositories
  - Register all services
  - Register DbContext with connection pooling configuration
  - Register MediatR with pipeline behaviors
  - Register AutoMapper
  - Register background services
  - Add comment explaining connection pooling for serverless database
  - _Requirements: 15.8, 15.9_


## 13. API Layer - Controllers

- [x] 13.1 Create ApiController base class
  - Add [ApiVersion("1.0")] attribute
  - Add [ApiController] attribute
  - Add [Route("api/v{version:apiVersion}/[controller]")] attribute
  - _Requirements: 18.1-18.4_

- [x] 13.2 Create AuthController
  - POST /api/v1/auth/register
  - POST /api/v1/auth/login
  - POST /api/v1/auth/refresh
  - POST /api/v1/auth/logout
  - POST /api/v1/auth/revoke-all-sessions (Admin only)
  - _Requirements: 1.1-1.8_

- [x] 13.3 Create UsersController
  - GET /api/v1/users/me
  - PUT /api/v1/users/me
  - GET /api/v1/users (Admin/SuperAdmin only, paginated)
  - GET /api/v1/users/{id} (Admin/SuperAdmin only)
  - PUT /api/v1/users/{id}/role (SuperAdmin only)
  - DELETE /api/v1/users/{id} (SuperAdmin only, soft delete)
  - GET /api/v1/users/{id}/productivity
  - _Requirements: 2.1-2.6, 26.1-26.5, 27.1-27.5_

- [x] 13.4 Create TasksController
  - GET /api/v1/tasks (filterable, paginated)
  - GET /api/v1/tasks/{id}
  - POST /api/v1/tasks
  - PUT /api/v1/tasks/{id}
  - PATCH /api/v1/tasks/{id}/status
  - DELETE /api/v1/tasks/{id} (soft delete)
  - GET /api/v1/tasks/{id}/history
  - GET /api/v1/tasks/{id}/comments
  - POST /api/v1/tasks/{id}/comments
  - DELETE /api/v1/tasks/{id}/comments/{commentId}
  - GET /api/v1/tasks/{id}/subtasks
  - POST /api/v1/tasks/export
  - POST /api/v1/tasks/import
  - _Requirements: 3.1-3.8, 4.1-4.8, 5.1-5.6, 9.1-9.6_

- [x] 13.5 Create DashboardController
  - GET /api/v1/dashboard/summary
  - GET /api/v1/dashboard/velocity
  - GET /api/v1/dashboard/workload (Admin only)
  - GET /api/v1/dashboard/overdue
  - _Requirements: 7.1-7.7_

- [x] 13.6 Create AiController
  - POST /api/v1/ai/parse-task
  - POST /api/v1/ai/decompose/{taskId}
  - POST /api/v1/ai/search
  - GET /api/v1/ai/workload-suggestions (Admin only)
  - POST /api/v1/ai/import-normalize
  - GET /api/v1/ai/digest/me
  - _Requirements: 8.1-8.7, 9.1-9.6, 10.1-10.7, 11.1-11.6, 12.1-12.7, 13.1-13.7_

- [x] 13.7 Create NotificationsController
  - GET /api/v1/notifications (paginated, filterable by IsRead)
  - PATCH /api/v1/notifications/{id}/read
  - PATCH /api/v1/notifications/read-all
  - _Requirements: 28.1-28.5_

- [x] 13.8 Create HealthController
  - GET /health
  - Check database connectivity
  - Check LangChain service availability
  - Check available disk space for log files
  - Return JSON with status of each check
  - _Requirements: 17.1-17.6_


## 14. API Layer - SignalR Hub

- [x] 14.1 Create TaskHub
  - Implement OnConnectedAsync (joins user group)
  - Implement OnDisconnectedAsync
  - Authenticate connections using JWT
  - Create groups by UserId
  - _Requirements: 6.5, 6.6_

- [x] 14.2 Create TaskHubService
  - Implement ITaskHubService
  - NotifyTaskAssigned method
  - NotifyStatusChanged method
  - NotifyCommentAdded method
  - NotifyAiSuggestionReady method
  - Broadcast to appropriate user groups
  - _Requirements: 6.1-6.4_


## 15. API Layer - Middleware and Exception Handling

- [x] 15.1 Create GlobalExceptionHandler
  - Implement IExceptionHandler
  - Map domain exceptions to HTTP status codes
  - Return ProblemDetails response
  - Log with correlation ID
  - Never expose stack traces to client
  - _Requirements: 19.1-19.6_

- [x] 15.2 Create RequestLoggingMiddleware
  - Log request/response at Information level
  - Exclude /health endpoint
  - Include correlation ID, user ID, duration
  - _Requirements: 16.2, 16.5, 16.6_

- [x] 15.3 Create CorrelationIdMiddleware
  - Generate or extract correlation ID from request
  - Add to HTTP context
  - Include in response headers
  - _Requirements: 16.2, 16.3_


## 16. API Layer - Program.cs Configuration

- [x] 16.1 Configure Serilog
  - Two-stage initialization (bootstrap logger first)
  - Enrich with MachineName, Environment, CorrelationId, UserId
  - Write to Console (JSON format) and rolling file
  - Add comments explaining two-stage pattern
  - _Requirements: 16.1-16.6_

- [x] 16.2 Configure authentication and authorization
  - Configure JWT Bearer authentication
  - Configure role-based authorization policies
  - Add comments explaining token configuration
  - _Requirements: 1.1-1.8, 2.1-2.6_

- [x] 16.3 Configure MediatR and pipeline behaviors
  - Register MediatR
  - Register ValidationBehavior
  - Register LoggingBehavior
  - Register PerformanceBehavior
  - _Requirements: 6.1-6.4_

- [x] 16.4 Configure API versioning
  - Configure DefaultApiVersion
  - Configure AssumeDefaultVersionWhenUnspecified
  - Configure ReportApiVersions
  - _Requirements: 18.1-18.4_

- [x] 16.5 Configure SignalR
  - Add SignalR services
  - Map TaskHub endpoint
  - Configure JWT authentication for hub
  - _Requirements: 6.1-6.8_

- [x] 16.6 Configure health checks
  - Add database health check
  - Add LangChain service health check
  - Add disk space health check
  - Map /health endpoint
  - Azure App Service uses this endpoint for health monitoring
  - _Requirements: 17.1-17.6_

- [x] 16.7 Configure CORS
  - Read allowed origins from environment variable
  - Configure CORS policy
  - _Requirements: 29.5_

- [x] 16.8 Configure database migrations on startup
  - Run dotnet ef database update if migrations pending
  - Implement as part of startup, not background job
  - _Requirements: 29.3_


## 17. Frontend - Core Setup

- [ ] 17.1 Create Axios configuration
  - Create axios instance with base URL
  - Add request interceptor for Authorization header
  - Add response interceptor for token refresh on 401
  - Add comments explaining token refresh flow
  - _Requirements: 21.3, 21.5_

- [ ] 17.2 Create Zustand auth store
  - State: user, accessToken, role, isAuthenticated
  - Actions: login, logout, setUser, setToken
  - Persist to sessionStorage (except tokens)
  - _Requirements: 21.1, 21.5, 21.6_

- [ ] 17.3 Create Zustand notification store
  - State: unreadCount, notifications
  - Actions: addNotification, markAsRead, markAllAsRead, incrementUnread
  - _Requirements: 28.1-28.5_

- [ ] 17.4 Configure TanStack Query
  - Create QueryClient with global configuration
  - Configure staleTime, cacheTime, refetchOnWindowFocus
  - Configure global error handler (401, 403, 5xx)
  - Define query keys convention
  - _Requirements: 21.2, 21.4_

- [ ] 17.5 Create route configuration
  - Define routes array with paths, elements, roles
  - Configure lazy loading for all pages
  - _Requirements: 20.1-20.5_

- [ ] 17.6 Create PrivateRoute component
  - Check authStore.isAuthenticated
  - Redirect to /login if not authenticated
  - Check user role against route.roles
  - Show AccessDenied if insufficient permissions
  - _Requirements: 20.1, 20.2, 20.5_


## 18. Frontend - API Service Modules

- [ ] 18.1 Create auth.service.ts
  - register(data)
  - login(email, password)
  - refreshToken()
  - logout()
  - revokeAllSessions()
  - _Requirements: 1.1-1.8_

- [ ] 18.2 Create tasks.service.ts
  - getTasks(filters)
  - getTaskById(id)
  - createTask(data)
  - updateTask(id, data)
  - updateTaskStatus(id, status)
  - deleteTask(id)
  - getTaskHistory(id)
  - getComments(taskId)
  - createComment(taskId, content)
  - deleteComment(taskId, commentId)
  - getSubtasks(taskId)
  - exportTasks(filters)
  - importTasks(file)
  - _Requirements: 3.1-3.8, 4.1-4.8, 5.1-5.6_

- [ ] 18.3 Create dashboard.service.ts
  - getSummary()
  - getVelocity(days)
  - getWorkload()
  - getOverdue()
  - _Requirements: 7.1-7.7_

- [ ] 18.4 Create ai.service.ts
  - parseTask(input)
  - decomposeTask(taskId)
  - searchTasks(query, semantic)
  - getWorkloadSuggestions()
  - normalizeImport(csvData)
  - getMyDigest()
  - _Requirements: 8.1-8.7, 9.1-9.6, 10.1-10.7, 11.1-11.6, 12.1-12.7, 13.1-13.7_

- [ ] 18.5 Create notifications.service.ts
  - getNotifications(filters)
  - markAsRead(id)
  - markAllAsRead()
  - _Requirements: 28.1-28.5_

- [ ] 18.6 Create users.service.ts
  - getCurrentUser()
  - updateCurrentUser(data)
  - getUsers(filters)
  - getUserById(id)
  - updateUserRole(id, role)
  - deleteUser(id)
  - getUserProductivity(id)
  - _Requirements: 2.1-2.6, 26.1-26.5, 27.1-27.5_


## 19. Frontend - Custom Hooks

- [ ] 19.1 Create auth hooks
  - useAuth() - access auth store
  - useLogin() - mutation for login
  - useRegister() - mutation for register
  - useLogout() - mutation for logout
  - _Requirements: 1.1-1.8_

- [ ] 19.2 Create task hooks
  - useTasks(filters) - query for task list
  - useTask(id) - query for single task
  - useCreateTask() - mutation for create
  - useUpdateTask() - mutation for update
  - useDeleteTask() - mutation for delete
  - useTaskComments(taskId) - query for comments
  - useCreateComment() - mutation for comment
  - _Requirements: 3.1-3.8, 4.1-4.8, 5.1-5.6_

- [ ] 19.3 Create dashboard hooks
  - useDashboard() - query for summary
  - useVelocity() - query for velocity data
  - useWorkload() - query for workload distribution
  - _Requirements: 7.1-7.7_

- [ ] 19.4 Create AI hooks
  - useAiParse() - mutation for natural language parsing
  - useAiDecompose() - mutation for task decomposition
  - useSemanticSearch() - query for semantic search
  - useWorkloadSuggestions() - query for workload suggestions
  - _Requirements: 8.1-8.7, 9.1-9.6, 11.1-11.6, 12.1-12.7_

- [ ] 19.5 Create utility hooks
  - useDebounce(value, delay) - debounce hook for search
  - useInfiniteScroll() - infinite scroll pagination
  - useToast() - toast notifications
  - _Requirements: 22.5_

- [ ] 19.6 Create SignalR hook
  - useSignalR() - establish connection, handle events
  - Connect on login, disconnect on logout
  - Implement automatic reconnection with exponential backoff
  - Handle TaskAssigned, StatusChanged, CommentAdded, AiSuggestionReady events
  - Invalidate TanStack Query cache keys on events
  - _Requirements: 6.7, 6.8, 25.1-25.6_


## 20. Frontend - Shared Components

- [ ] 20.1 Create UI components
  - Button component (variants: primary, secondary, danger)
  - Input component (with error display)
  - Select component
  - DatePicker component
  - Modal component
  - Toast component
  - Loading spinner component
  - Badge component (for priority, status)
  - Avatar component
  - _Requirements: All UI requirements_

- [ ] 20.2 Create layout components
  - Header component (with navigation, user menu, notification bell)
  - Sidebar component (with navigation links)
  - Footer component
  - MainLayout component (combines header, sidebar, content area)
  - _Requirements: All page requirements_

- [ ] 20.3 Create form components
  - FormField component (wraps input with label and error)
  - MultiSelect component
  - TagInput component (multi-input chip field)
  - UserSearchDropdown component
  - _Requirements: 24.1-24.5_


## 21. Frontend - Authentication Pages

- [ ] 21.1 Create LoginPage
  - Email/password form with React Hook Form and Zod validation
  - Link to register page
  - On success, store tokens and redirect to dashboard
  - _Requirements: 1.1-1.8_

- [ ] 21.2 Create RegisterPage
  - Full name, email, password, confirm password fields
  - Password strength indicator
  - Form validation with Zod
  - On success, redirect to login
  - _Requirements: 1.1, 1.2_


## 22. Frontend - Dashboard Page

- [ ] 22.1 Create DashboardPage
  - Four stat cards (Pending, InProgress, Completed, Blocked counts)
  - Line chart (Recharts) showing task completion velocity (last 30 days)
  - Bar chart (Recharts) showing tasks by priority
  - AI digest card displaying today's digest
  - Overdue tasks alert section
  - Admin users see workload distribution donut chart (Recharts PieChart)
  - _Requirements: 7.1-7.7_

- [ ] 22.2 Create dashboard components
  - StatCard component
  - VelocityChart component (Recharts LineChart)
  - PriorityChart component (Recharts BarChart)
  - WorkloadChart component (Recharts PieChart)
  - DigestCard component
  - OverdueAlert component
  - _Requirements: 7.1-7.7_


## 23. Frontend - Task List Page

- [ ] 23.1 Create TaskListPage
  - Filter panel (Status, Priority, Category, Assignee, Due Date range)
  - Search input with 300ms debounce
  - Semantic search toggle
  - Task cards with all required information
  - Pagination or infinite scroll
  - Admin users see bulk action toolbar
  - _Requirements: 22.1-22.7_

- [ ] 23.2 Create task list components
  - TaskFilters component
  - TaskCard component (shows title, badges, avatar, due date, AI probability)
  - TaskList component
  - BulkActionToolbar component (Admin only)
  - SearchBar component with semantic toggle
  - _Requirements: 22.1-22.7_


## 24. Frontend - Task Detail Page

- [ ] 24.1 Create TaskDetailPage
  - Full task information display
  - Inline status change dropdown
  - Comment thread with sentiment indicators
  - Task audit history timeline
  - Subtasks list with add/complete/remove actions
  - AI decomposition button with modal
  - _Requirements: 23.1-23.5_

- [ ] 24.2 Create task detail components
  - TaskInfo component (editable fields)
  - CommentThread component
  - CommentItem component (with sentiment indicator)
  - AuditTimeline component
  - SubtasksList component
  - DecompositionModal component
  - _Requirements: 23.1-23.5_


## 25. Frontend - Task Form Page

- [ ] 25.1 Create TaskFormPage (New/Edit)
  - Two modes: Natural Language and Manual Form
  - Natural Language mode: textarea + Parse button
  - Manual Form mode: all fields with validation
  - Form validation with React Hook Form + Zod
  - Fields: Title, Description, Assignee, Priority, Category, Due Date, Estimated Hours, Tags
  - _Requirements: 24.1-24.5_

- [ ] 25.2 Create task form components
  - NaturalLanguageInput component
  - TaskForm component
  - Zod validation schema for task form
  - _Requirements: 24.1-24.5_


## 26. Frontend - User Profile and Admin Pages

- [ ] 26.1 Create UserProfilePage
  - Display user info (name, email, role, account creation date)
  - Productivity score gauge chart (Recharts RadialBarChart)
  - Personal velocity chart (tasks completed per week, last 12 weeks)
  - Edit profile form
  - Logout button
  - _Requirements: 26.1-26.5_

- [ ] 26.2 Create AdminPage
  - User management table with role assignment
  - Workload balancing panel with AI suggestions and Accept buttons
  - System metrics cards (total tasks, active users, AI feature usage)
  - AI adoption metrics charts
  - _Requirements: 27.1-27.5_

- [ ] 26.3 Create admin components
  - UserManagementTable component
  - WorkloadBalancingPanel component
  - SystemMetrics component
  - AiAdoptionCharts component
  - _Requirements: 27.1-27.5_


## 27. Frontend - Notifications and AI Assistant

- [ ] 27.1 Create NotificationsPanel
  - Slide-in drawer from right side
  - Notification list ordered by creation time
  - Each notification shows icon, message, timestamp, read/unread indicator
  - Mark as read button per notification
  - Mark all as read button
  - Real-time updates via SignalR
  - Unread count badge on notification bell
  - _Requirements: 28.1-28.5_

- [ ] 27.2 Create AiAssistantDrawer
  - Slide-in panel from right side
  - Floating action button (AI icon) accessible from all pages
  - Natural language task input with Parse button
  - Semantic search input
  - Today's digest display
  - Quick actions (decompose, workload suggestions for admin)
  - _Requirements: 8.1-8.7, 12.1-12.7_

- [ ] 27.3 Create notification and AI components
  - NotificationItem component
  - NotificationBell component (with unread badge)
  - AiAssistantButton component (floating action button)
  - DigestDisplay component
  - _Requirements: 28.1-28.5_


## 28. Frontend - Utilities and Styling

- [ ] 28.1 Create utility functions
  - constants.ts (API URLs, enum mappings)
  - formatters.ts (date, number, status formatters)
  - validators.ts (Zod schemas for all forms)
  - helpers.ts (utility functions)
  - _Requirements: All frontend requirements_

- [ ] 28.2 Configure Tailwind CSS design system
  - Color palette (primary, success, warning, danger, neutral)
  - Typography (Inter font, responsive sizes)
  - Spacing (4px base unit)
  - Component styles (border radius, shadows, transitions)
  - Priority badge colors
  - Status badge colors
  - _Requirements: All UI requirements_

- [ ] 28.3 Create App.tsx and main.tsx
  - Setup QueryClientProvider
  - Setup Router
  - Setup global error boundary
  - Setup toast container
  - _Requirements: All frontend requirements_


## 29. Testing - Backend Unit Tests

- [ ] 29.1 Create AuthService tests
  - Test registration with duplicate email returns conflict
  - Test login with wrong password returns unauthorized
  - Test refresh token rotation invalidates old token
  - Test logout revokes token
  - Add comment describing test coverage
  - _Requirements: 1.1-1.8, 30.1-30.7_

- [ ] 29.2 Create TaskService tests
  - Test create task assigns correct defaults
  - Test status update to Completed sets CompletedAt
  - Test soft delete sets IsDeleted and does not remove record
  - Test optimistic concurrency conflict handling
  - Add comment describing test coverage
  - _Requirements: 3.1-3.8, 30.1-30.7_

- [ ] 29.3 Create DashboardQueryHandler tests
  - Test counts match seeded data
  - Test admin sees all users
  - Test member sees only their own
  - Test indexed view is queried
  - Add comment describing test coverage
  - _Requirements: 7.1-7.7, 30.1-30.7_

- [ ] 29.4 Create NaturalLanguageTaskService tests
  - Mock LangChain call
  - Test returned command has correctly parsed fields
  - Test failed AI call triggers Polly retry
  - Test AI interaction is logged
  - Add comment describing test coverage
  - _Requirements: 8.1-8.7, 30.1-30.7_

- [ ] 29.5 Create WorkloadBalancingService tests
  - Test with seeded team (one member 10 tasks, another 0)
  - Test AI suggestion is accepted
  - Test endpoint returns correct suggestion structure
  - Add comment describing test coverage
  - _Requirements: 11.1-11.6, 30.1-30.7_

- [ ] 29.6 Create ExceptionHandlingMiddleware tests
  - Test unhandled exception returns 500 with ProblemDetails
  - Test stack trace is not exposed
  - Test exception is logged with correlation ID
  - Add comment describing test coverage
  - _Requirements: 19.1-19.6, 30.1-30.7_

- [ ] 29.7 Create repository tests
  - Use in-memory SQLite provider
  - Test CRUD operations
  - Test query filters
  - Test soft delete behavior
  - Add comment describing test coverage
  - _Requirements: 30.1-30.7_


## 30. Configuration and Deployment

- [ ] 30.1 Create backend configuration files
  - appsettings.json (with placeholders for environment variables)
  - sonar-project.properties (backend)
  - Configure exclusions for migrations and auto-generated files
  - _Requirements: 30.4, 30.5_

- [ ] 30.2 Create frontend configuration files
  - vite.config.ts
  - tailwind.config.js
  - vercel.json (with SPA rewrite rules)
  - sonar-project.properties (frontend)
  - Configure exclusions for node_modules and dist
  - _Requirements: 29.1, 30.4, 30.5_

- [ ] 30.3 Configure Azure App Service deployment
  - Create .deployment file for Azure
  - Configure web.config for IIS (if needed)
  - Configure connection string with pooling parameters in Azure App Service settings
  - Configure environment variables in Azure App Service Configuration
  - Enable Always On (if available in F1 tier, otherwise document CPU time management)
  - Add comment explaining CPU time quota management for F1 tier
  - _Requirements: 15.8, 29.2_

- [ ] 30.4 Create infrastructure files
  - azure-sql-setup.sql bootstrap script
  - CONTRIBUTING.md (branch naming, commit format, PR process)
  - Document Azure App Service F1 tier limitations and best practices
  - _Requirements: 29.2, 29.3_

- [ ] 30.5 Create comprehensive README.md
  - Project overview
  - Tech stack
  - Repository structure
  - Setup instructions (backend, frontend, database)
  - Environment variables documentation
  - Deployment instructions (Vercel for frontend, Azure App Service F1 for backend, Azure SQL)
  - Azure App Service F1 tier setup and configuration steps
  - SonarQube setup steps
  - Git workflow
  - _Requirements: All_

- [ ] 30.6 Final checkpoint - Ensure all tests pass
  - Run all backend unit tests
  - Run all frontend builds
  - Verify SonarQube analysis passes
  - Verify all environment variables are documented
  - _Requirements: All_

