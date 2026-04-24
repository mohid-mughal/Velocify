# Requirements Document

## Introduction

Velocify is an AI-augmented task management platform designed for production deployment across multiple cloud services. The system provides real-time collaboration, LangChain-powered AI features, advanced database optimization, role-based access control, and comprehensive analytics. The platform consists of three independently deployable components: an ASP.NET Core 8 backend API, a React.js 18 frontend application, and an Azure SQL Database. The system is architected for clean separation of concerns, enabling independent scaling and maintenance of each component.

## Glossary

- **System**: The Velocify platform as a whole, including backend API, frontend application, and database
- **Backend**: The ASP.NET Core 8 Web API service
- **Frontend**: The React.js 18 single-page application
- **Database**: The Azure SQL Database Serverless instance
- **User**: An authenticated person using the platform with one of three roles: SuperAdmin, Admin, or Member
- **TaskItem**: A work item with title, description, status, priority, category, and assignment information
- **AI Engine**: The LangChain.NET-powered service layer providing intelligent features
- **Productivity Score**: A computed metric representing task completion efficiency weighted by priority
- **Natural Language Input**: User-provided text in conversational format that the AI Engine parses into structured data
- **Semantic Search**: AI-powered search using embedding vectors to find conceptually similar tasks
- **Task Decomposition**: AI-generated breakdown of a complex task into smaller subtasks
- **Daily Digest**: AI-generated personalized summary of tasks and priorities for a user
- **Workload Balancing**: AI-powered analysis and suggestions for optimal task distribution across team members
- **Sentiment Analysis**: AI-powered evaluation of comment tone and emotional content
- **Real-time Event**: A notification pushed to connected clients via SignalR without requiring a page refresh
- **Soft Delete**: Marking a record as deleted without physically removing it from the database
- **Optimistic Concurrency**: A conflict detection mechanism using row versioning to prevent lost updates
- **Compiled Query**: A pre-compiled EF Core query that improves performance by eliminating repeated query translation
- **Indexed View**: A materialized view with a clustered index that pre-computes aggregations
- **Table Partitioning**: Dividing a large table into smaller physical segments based on a column value
- **JWT**: JSON Web Token used for stateless authentication
- **Refresh Token**: A long-lived token used to obtain new access tokens without re-authentication
- **CQRS**: Command Query Responsibility Segregation pattern separating read and write operations
- **SignalR Hub**: A server-side component managing real-time bidirectional communication with clients

## Requirements

### Requirement 1: User Authentication and Authorization

**User Story:** As a user, I want to securely register, log in, and maintain authenticated sessions, so that my data is protected and I can access the platform from multiple devices.

#### Acceptance Criteria

1. WHEN a user submits registration with valid email and password THEN the Backend SHALL create a new user account with hashed password and return a success response
2. WHEN a user submits registration with an email that already exists THEN the Backend SHALL reject the registration and return a conflict error
3. WHEN a user submits valid credentials to the login endpoint THEN the Backend SHALL generate a JWT access token with 15-minute expiration and a refresh token with 7-day expiration
4. WHEN a user submits a valid refresh token THEN the Backend SHALL generate a new access token and invalidate the old refresh token
5. WHEN a user logs out THEN the Backend SHALL revoke the current refresh token and prevent its reuse
6. WHEN an access token expires THEN the Backend SHALL reject API requests with 401 Unauthorized status
7. WHEN a SuperAdmin revokes all sessions for a user THEN the Backend SHALL invalidate all refresh tokens for that user
8. THE Backend SHALL store refresh tokens as SHA-256 hashes in the UserSession table

### Requirement 2: Role-Based Access Control

**User Story:** As a system administrator, I want to assign roles to users with different permission levels, so that data access and operations are properly restricted based on organizational hierarchy.

#### Acceptance Criteria

1. WHEN a SuperAdmin assigns a role to a user THEN the Backend SHALL update the user's role and apply the new permissions immediately
2. WHEN a Member attempts to access another user's tasks THEN the Backend SHALL reject the request with 403 Forbidden status
3. WHEN an Admin requests task data THEN the Backend SHALL return only tasks belonging to their team members
4. WHEN a SuperAdmin requests task data THEN the Backend SHALL return all tasks across all users
5. WHEN a non-SuperAdmin attempts to change user roles THEN the Backend SHALL reject the request with 403 Forbidden status
6. THE Backend SHALL enforce role-based authorization on all protected endpoints using JWT claims

### Requirement 3: Task Management Core Operations

**User Story:** As a user, I want to create, read, update, and delete tasks with detailed information, so that I can organize and track my work effectively.

#### Acceptance Criteria

1. WHEN a user creates a task with valid data THEN the Backend SHALL persist the task with a unique identifier and return the created task
2. WHEN a user updates a task THEN the Backend SHALL validate the changes and update the task with a new UpdatedAt timestamp
3. WHEN a user changes task status to Completed THEN the Backend SHALL set the CompletedAt timestamp to the current time
4. WHEN a user deletes a task THEN the Backend SHALL set IsDeleted to true without removing the database record
5. WHEN a user requests their task list THEN the Backend SHALL return only tasks where IsDeleted is false
6. WHEN two users update the same task simultaneously THEN the Backend SHALL detect the conflict using RowVersion and return 409 Conflict with current server values
7. THE Backend SHALL validate all task fields using FluentValidation before persistence
8. THE Backend SHALL record all task field changes in the TaskAuditLog table with timestamp and user identifier

### Requirement 4: Task Filtering and Search

**User Story:** As a user, I want to filter and search tasks by multiple criteria, so that I can quickly find relevant work items.

#### Acceptance Criteria

1. WHEN a user requests tasks filtered by status THEN the Backend SHALL return only tasks matching the specified status values
2. WHEN a user requests tasks filtered by priority THEN the Backend SHALL return only tasks matching the specified priority values
3. WHEN a user requests tasks filtered by category THEN the Backend SHALL return only tasks matching the specified category values
4. WHEN a user requests tasks filtered by assignee THEN the Backend SHALL return only tasks assigned to the specified user
5. WHEN a user requests tasks filtered by due date range THEN the Backend SHALL return only tasks with due dates within the specified range
6. WHEN a user submits a search term THEN the Backend SHALL return tasks where the title or tags contain the search term using case-insensitive matching
7. WHEN a user requests paginated results THEN the Backend SHALL return the specified page with the specified page size and include total count metadata
8. THE Backend SHALL limit maximum page size to 100 items to prevent excessive data transfer

### Requirement 5: Task Comments and Collaboration

**User Story:** As a user, I want to add comments to tasks and see comments from other team members, so that we can collaborate and communicate about work items.

#### Acceptance Criteria

1. WHEN a user posts a comment on a task THEN the Backend SHALL persist the comment with timestamp and user identifier
2. WHEN a user requests comments for a task THEN the Backend SHALL return all non-deleted comments ordered by creation time
3. WHEN a user deletes their own comment THEN the Backend SHALL set IsDeleted to true on that comment
4. WHEN a comment is posted THEN the Backend SHALL asynchronously analyze sentiment and store the score in the SentimentScore column
5. WHEN a user requests task details THEN the Backend SHALL include aggregated sentiment score across all comments
6. THE Backend SHALL prevent users from deleting comments created by other users unless they are Admin or SuperAdmin

### Requirement 6: Real-Time Notifications via SignalR

**User Story:** As a user, I want to receive instant notifications when tasks are assigned to me or updated, so that I can respond quickly without manually refreshing the page.

#### Acceptance Criteria

1. WHEN a task is assigned to a user THEN the Backend SHALL push a task-assigned event to that user's SignalR connection
2. WHEN a task status changes THEN the Backend SHALL push a status-changed event to both the assignee and creator
3. WHEN a comment is added to a task THEN the Backend SHALL push a new-comment event to the task's assignee
4. WHEN an AI suggestion becomes available THEN the Backend SHALL push an ai-suggestion-ready event to the relevant user
5. WHEN a user connects to the SignalR hub THEN the Backend SHALL authenticate the connection using the JWT token
6. WHEN a user connects to the SignalR hub THEN the Backend SHALL add the connection to a group identified by the user's ID
7. THE Frontend SHALL establish a SignalR connection on login and close it on logout
8. THE Frontend SHALL implement automatic reconnection with exponential backoff when the connection is lost

### Requirement 7: Dashboard Analytics and Metrics

**User Story:** As a user, I want to see visual analytics of my tasks and productivity, so that I can understand my work patterns and improve efficiency.

#### Acceptance Criteria

1. WHEN a user requests dashboard summary THEN the Backend SHALL return task counts grouped by status using the indexed view vw_UserTaskSummary
2. WHEN a user requests velocity data THEN the Backend SHALL return task completion counts per day for the last 30 days
3. WHEN an Admin requests workload distribution THEN the Backend SHALL return task counts per team member
4. WHEN a user requests their productivity score THEN the Backend SHALL return the current score and historical trend
5. WHEN a user requests overdue tasks THEN the Backend SHALL return tasks where DueDate is before current time and Status is not Completed
6. THE Backend SHALL calculate productivity score using the stored procedure usp_RecalculateUserProductivityScores every 6 hours
7. THE Frontend SHALL display dashboard metrics using Recharts visualizations including line charts, bar charts, and donut charts

### Requirement 8: Natural Language Task Creation

**User Story:** As a user, I want to create tasks by typing natural language descriptions, so that I can quickly capture work items without filling out multiple form fields.

#### Acceptance Criteria

1. WHEN a user submits natural language text to the parse-task endpoint THEN the AI Engine SHALL extract title, description, priority, category, assignee email, and due date
2. WHEN the AI Engine successfully parses the input THEN the Backend SHALL return a pre-filled task structure for user confirmation
3. WHEN the AI Engine cannot parse required fields THEN the Backend SHALL return partial results with null values for unparsed fields
4. WHEN the AI Engine call fails THEN the Backend SHALL retry up to 3 times with exponential backoff using Polly
5. WHEN all retry attempts fail THEN the Backend SHALL return an error response with a user-friendly message
6. THE Backend SHALL log all AI interactions to the AiInteractionLog table including input summary, output summary, tokens used, and latency
7. THE Frontend SHALL display the parsed task fields in an editable form before final submission

### Requirement 9: Smart Task Decomposition

**User Story:** As a user, I want the AI to suggest breaking down complex tasks into subtasks, so that I can better plan and track progress on large work items.

#### Acceptance Criteria

1. WHEN a user requests decomposition for a task THEN the AI Engine SHALL analyze the title and description and generate up to 8 subtask suggestions
2. WHEN the AI Engine generates subtasks THEN the Backend SHALL return a list of suggested titles and estimated hours
3. WHEN a user accepts a subtask suggestion THEN the Backend SHALL create a new TaskItem with ParentTaskId linking to the original task
4. WHEN a user requests subtasks for a task THEN the Backend SHALL return all TaskItems where ParentTaskId matches the specified task
5. THE Backend SHALL use LangChain structured output parser to extract subtask information from AI responses
6. THE Backend SHALL log decomposition requests to AiInteractionLog with FeatureType set to Decomposition

### Requirement 10: Daily AI Digest Generation

**User Story:** As a user, I want to receive a personalized AI-generated summary each morning, so that I can start my day with clear priorities and context.

#### Acceptance Criteria

1. WHEN the daily digest job runs at 8 AM THEN the Backend SHALL generate a digest for each active user
2. WHEN generating a digest THEN the AI Engine SHALL include tasks due today, overdue tasks, priority recommendations, and an encouraging message
3. WHEN a digest is generated THEN the Backend SHALL store it as a Notification with Type set to AiSuggestion
4. WHEN a user connects via SignalR THEN the Backend SHALL push any unread digest notifications to the client
5. WHEN a user requests their digest THEN the Backend SHALL return the most recent digest notification
6. THE Backend SHALL implement digest generation as an IHostedService that runs on a daily schedule
7. THE Backend SHALL use LangChain summarization chain to generate digest content

### Requirement 11: Workload Balancing Suggestions

**User Story:** As an admin, I want AI-powered suggestions for redistributing tasks across my team, so that workload is balanced and team members are not overloaded.

#### Acceptance Criteria

1. WHEN an Admin requests workload suggestions THEN the AI Engine SHALL analyze current task assignments, productivity scores, and due dates for all team members
2. WHEN the AI Engine identifies imbalanced workload THEN the Backend SHALL return suggestions with task ID, suggested assignee ID, and reasoning
3. WHEN an Admin accepts a workload suggestion THEN the Backend SHALL reassign the specified task to the suggested user
4. WHEN a Member requests workload suggestions THEN the Backend SHALL reject the request with 403 Forbidden status
5. THE Backend SHALL provide the AI Engine with structured JSON summary of team workload including task counts and productivity metrics
6. THE Backend SHALL log workload balancing requests to AiInteractionLog with FeatureType set to Prioritization

### Requirement 12: Semantic Search

**User Story:** As a user, I want to search for tasks using natural language and find conceptually similar results, so that I can discover relevant tasks even when exact keywords don't match.

#### Acceptance Criteria

1. WHEN a user submits a semantic search query THEN the Backend SHALL execute both SQL LIKE search and embedding-based semantic search in parallel
2. WHEN executing semantic search THEN the Backend SHALL generate an embedding vector for the query using LangChain
3. WHEN comparing embeddings THEN the Backend SHALL retrieve cached embeddings from the TaskEmbedding table and calculate similarity scores
4. WHEN merging search results THEN the Backend SHALL combine and rank results by combined relevance score
5. WHEN a task title or description changes THEN the Backend SHALL regenerate and update the embedding vector in TaskEmbedding table
6. THE Backend SHALL store embedding vectors as JSON arrays in nvarchar(max) columns
7. THE Backend SHALL log semantic search requests to AiInteractionLog with FeatureType set to Search

### Requirement 13: Smart CSV Import with AI Normalization

**User Story:** As a user, I want to import tasks from CSV files with flexible column formats, so that I can migrate data from other systems without manual reformatting.

#### Acceptance Criteria

1. WHEN a user uploads a CSV file THEN the AI Engine SHALL analyze the column headers and map them to internal schema fields
2. WHEN the AI Engine detects non-standard values THEN the Backend SHALL normalize them to valid enum values
3. WHEN normalization is complete THEN the Backend SHALL return the mapped and normalized task list for user review
4. WHEN a user confirms the import THEN the Backend SHALL create TaskItems for all rows in the normalized list
5. WHEN the CSV contains invalid data that cannot be normalized THEN the Backend SHALL return validation errors with row numbers
6. THE Backend SHALL support multipart form upload for CSV files
7. THE Backend SHALL log import operations to AiInteractionLog with FeatureType set to Import

### Requirement 14: Comment Sentiment Analysis

**User Story:** As an admin, I want to see sentiment analysis of task comments, so that I can identify team health issues and tasks with negative communication patterns.

#### Acceptance Criteria

1. WHEN a comment is posted THEN the Backend SHALL asynchronously analyze sentiment using LangChain
2. WHEN sentiment analysis completes THEN the Backend SHALL store a score between 0.0 and 1.0 in the SentimentScore column
3. WHEN a user requests task details THEN the Backend SHALL include average sentiment score across all comments
4. WHEN an Admin views team metrics THEN the Frontend SHALL display aggregated sentiment trends
5. THE Backend SHALL treat sentiment analysis as a non-blocking operation that does not delay comment creation response

### Requirement 15: Database Performance Optimization

**User Story:** As a system operator, I want the database to perform efficiently under load, so that response times remain fast and resource costs stay low.

#### Acceptance Criteria

1. WHEN querying active tasks THEN the Database SHALL use filtered indexes that exclude IsDeleted = 1 rows
2. WHEN executing dashboard count queries THEN the Database SHALL use the indexed view vw_UserTaskSummary instead of aggregating raw data
3. WHEN executing frequently called queries THEN the Backend SHALL use EF Core compiled queries stored in CompiledQueries class
4. WHEN reading data for display THEN the Backend SHALL use AsNoTracking() to prevent unnecessary change tracking overhead
5. WHEN including multiple collection navigations THEN the Backend SHALL use AsSplitQuery() to prevent Cartesian explosion
6. WHEN audit log queries execute THEN the Database SHALL use table partitioning to scan only relevant monthly partitions
7. WHEN concurrent updates occur THEN the Backend SHALL detect conflicts using RowVersion and return 409 Conflict responses
8. THE Database SHALL maintain composite indexes on (AssignedToUserId, Status, IsDeleted), (DueDate, Priority, IsDeleted), and (CreatedByUserId, CreatedAt DESC, IsDeleted)
9. THE Backend SHALL configure connection pooling with Min Pool Size 2 and Max Pool Size 100

### Requirement 16: Structured Logging and Monitoring

**User Story:** As a developer, I want comprehensive structured logs with correlation IDs, so that I can diagnose issues and monitor system health in production.

#### Acceptance Criteria

1. WHEN the Backend starts THEN Serilog SHALL initialize with two-stage configuration using bootstrap logger
2. WHEN a request is processed THEN Serilog SHALL enrich log events with MachineName, Environment, CorrelationId, and UserId
3. WHEN an unhandled exception occurs THEN the Backend SHALL log at Error level with full stack trace and return ProblemDetails response without exposing stack trace to client
4. WHEN a MediatR handler exceeds 500ms execution time THEN the Backend SHALL log a performance warning
5. WHEN the Backend writes logs THEN Serilog SHALL output to Console in JSON format and to rolling file for local development
6. THE Backend SHALL exclude health check endpoints from request/response logging to reduce noise

### Requirement 17: Health Checks and Monitoring

**User Story:** As a DevOps engineer, I want health check endpoints that verify system dependencies, so that deployment platforms can detect and respond to service degradation.

#### Acceptance Criteria

1. WHEN the /health endpoint is called THEN the Backend SHALL check database connectivity and return status
2. WHEN the /health endpoint is called THEN the Backend SHALL check LangChain service availability and return status
3. WHEN the /health endpoint is called THEN the Backend SHALL check available disk space for log files and return status
4. WHEN all health checks pass THEN the Backend SHALL return 200 OK with JSON body containing individual check statuses
5. WHEN any health check fails THEN the Backend SHALL return 503 Service Unavailable with JSON body indicating which checks failed
6. THE Backend SHALL configure Azure App Service to use the /health endpoint for health monitoring

### Requirement 18: API Versioning

**User Story:** As an API consumer, I want versioned endpoints, so that I can rely on stable contracts while new features are developed.

#### Acceptance Criteria

1. WHEN a client calls an API endpoint THEN the Backend SHALL require version specification in the URL path as /api/v1/
2. WHEN a client omits the version THEN the Backend SHALL assume default version v1
3. WHEN a client requests API version information THEN the Backend SHALL include supported versions in response headers
4. THE Backend SHALL configure DefaultApiVersion, AssumeDefaultVersionWhenUnspecified, and ReportApiVersions

### Requirement 19: Global Exception Handling

**User Story:** As a user, I want consistent error responses when something goes wrong, so that I can understand what happened and how to proceed.

#### Acceptance Criteria

1. WHEN an unhandled exception occurs THEN the Backend SHALL return a ProblemDetails response with appropriate HTTP status code
2. WHEN a domain exception occurs THEN the Backend SHALL map it to the corresponding HTTP status code
3. WHEN a validation exception occurs THEN the Backend SHALL return 400 Bad Request with field-level error details
4. WHEN an authorization exception occurs THEN the Backend SHALL return 403 Forbidden without exposing internal details
5. THE Backend SHALL never expose stack traces or internal implementation details to clients
6. THE Backend SHALL log all exceptions with correlation ID for troubleshooting

### Requirement 20: Frontend Routing and Navigation

**User Story:** As a user, I want intuitive navigation between different sections of the application, so that I can efficiently access features.

#### Acceptance Criteria

1. WHEN a user navigates to a protected route without authentication THEN the Frontend SHALL redirect to the login page
2. WHEN a Member navigates to an admin-only route THEN the Frontend SHALL display an access denied message
3. WHEN a user navigates between pages THEN the Frontend SHALL lazy load page components to reduce initial bundle size
4. WHEN a user refreshes the page THEN the Frontend SHALL maintain the current route using React Router v6
5. THE Frontend SHALL implement PrivateRoute component that checks auth store before rendering protected pages

### Requirement 21: Frontend State Management

**User Story:** As a user, I want the application to maintain my session state and cache server data efficiently, so that the interface feels responsive and doesn't require constant reloading.

#### Acceptance Criteria

1. WHEN a user logs in THEN the Frontend SHALL store user information and tokens in Zustand auth store
2. WHEN a user performs an action that modifies server data THEN TanStack Query SHALL invalidate relevant cache keys and refetch
3. WHEN a server request fails with 401 THEN the Frontend SHALL clear auth state and redirect to login
4. WHEN a server request fails with 5xx THEN the Frontend SHALL display a toast notification with error message
5. THE Frontend SHALL store access tokens in memory and refresh tokens in httpOnly cookies
6. THE Frontend SHALL never store access tokens in localStorage

### Requirement 22: Task List Interface

**User Story:** As a user, I want to view and filter my task list with visual indicators for priority and status, so that I can quickly identify what needs attention.

#### Acceptance Criteria

1. WHEN a user views the task list THEN the Frontend SHALL display tasks with priority badges, status badges, assignee avatars, and due date indicators
2. WHEN a task is overdue THEN the Frontend SHALL display the due date in red
3. WHEN a task is due today THEN the Frontend SHALL display the due date in orange
4. WHEN a user applies filters THEN the Frontend SHALL update the URL query parameters and fetch filtered results
5. WHEN a user types in the search input THEN the Frontend SHALL debounce the input and trigger search after 300ms
6. WHEN a user enables semantic search THEN the Frontend SHALL call the AI semantic search endpoint instead of standard search
7. WHEN an Admin selects multiple tasks THEN the Frontend SHALL display bulk action buttons for status change, reassignment, and deletion

### Requirement 23: Task Detail and Editing Interface

**User Story:** As a user, I want to view complete task information and make inline edits, so that I can manage tasks efficiently without navigating between multiple pages.

#### Acceptance Criteria

1. WHEN a user views task details THEN the Frontend SHALL display all task fields, comment thread, audit history, and subtasks
2. WHEN a user changes task status THEN the Frontend SHALL update the status via PATCH request and invalidate the task cache
3. WHEN a user clicks the AI decomposition button THEN the Frontend SHALL call the decompose endpoint and display suggestions in a modal
4. WHEN a user views comments THEN the Frontend SHALL display sentiment indicators for each comment
5. WHEN a user views audit history THEN the Frontend SHALL display a timeline of all field changes with timestamps and user names

### Requirement 24: Natural Language Task Form

**User Story:** As a user, I want to create tasks using either natural language input or traditional form fields, so that I can choose the most efficient method for my workflow.

#### Acceptance Criteria

1. WHEN a user types natural language input and clicks Parse THEN the Frontend SHALL call the AI parse-task endpoint
2. WHEN the AI returns parsed fields THEN the Frontend SHALL populate the form fields with the extracted values
3. WHEN the user submits the form THEN the Frontend SHALL validate all fields using Zod schema before sending to backend
4. WHEN validation fails THEN the Frontend SHALL display field-level error messages
5. THE Frontend SHALL implement the form using React Hook Form for controlled inputs

### Requirement 25: Real-Time UI Updates

**User Story:** As a user, I want to see updates to tasks and notifications instantly without refreshing the page, so that I always have current information.

#### Acceptance Criteria

1. WHEN a SignalR event is received THEN the Frontend SHALL invalidate the relevant TanStack Query cache keys
2. WHEN a task-assigned event is received THEN the Frontend SHALL display a toast notification and update the task list
3. WHEN a status-changed event is received THEN the Frontend SHALL update the task detail view if currently displayed
4. WHEN a new-comment event is received THEN the Frontend SHALL append the comment to the comment thread
5. WHEN the SignalR connection is lost THEN the Frontend SHALL attempt reconnection with exponential backoff
6. THE Frontend SHALL establish SignalR connection on login and close it on logout

### Requirement 26: User Profile and Productivity Metrics

**User Story:** As a user, I want to view my productivity metrics and personal statistics, so that I can track my performance over time.

#### Acceptance Criteria

1. WHEN a user views their profile THEN the Frontend SHALL display name, email, role, account creation date, and productivity score
2. WHEN a user views productivity score THEN the Frontend SHALL display a gauge chart using Recharts RadialBarChart
3. WHEN a user views velocity THEN the Frontend SHALL display a line chart of tasks completed per week for the last 12 weeks
4. WHEN a user edits their profile THEN the Frontend SHALL validate changes and submit to the backend
5. THE Frontend SHALL display productivity score as a percentage with visual indicator

### Requirement 27: Admin User Management

**User Story:** As an admin, I want to manage users and assign roles, so that I can control access and permissions across the platform.

#### Acceptance Criteria

1. WHEN an Admin views the user management table THEN the Frontend SHALL display all users with their roles and status
2. WHEN a SuperAdmin changes a user's role THEN the Frontend SHALL call the role update endpoint and refresh the user list
3. WHEN an Admin views workload balancing THEN the Frontend SHALL display AI suggestions with Accept buttons
4. WHEN an Admin accepts a workload suggestion THEN the Frontend SHALL reassign the task and update the workload display
5. WHEN a non-Admin navigates to the admin page THEN the Frontend SHALL display access denied message

### Requirement 28: Notification Management

**User Story:** As a user, I want to view and manage my notifications in a slide-in panel, so that I can stay informed without leaving my current page.

#### Acceptance Criteria

1. WHEN a user opens the notifications panel THEN the Frontend SHALL display all notifications ordered by creation time
2. WHEN a user marks a notification as read THEN the Frontend SHALL update the notification status and decrement the unread count
3. WHEN a user clicks "Mark all as read" THEN the Frontend SHALL update all notifications and reset the unread count to zero
4. WHEN a new notification arrives via SignalR THEN the Frontend SHALL increment the unread count badge and add the notification to the panel
5. THE Frontend SHALL implement the notifications panel as a slide-in drawer accessible from the top navigation

### Requirement 29: Deployment Configuration

**User Story:** As a DevOps engineer, I want deployment configurations for each component, so that I can deploy the system to production environments independently.

#### Acceptance Criteria

1. WHEN deploying the Frontend to Vercel THEN the deployment SHALL use vercel.json configuration with SPA rewrite rules
2. WHEN deploying the Backend to Azure App Service F1 Tier THEN the deployment SHALL use GitHub Actions workflow with Azure deployment
3. WHEN deploying the Database to Azure SQL THEN the deployment SHALL use azure-sql-setup.sql bootstrap script
4. WHEN the Backend starts THEN the Backend SHALL run pending EF Core migrations automatically
5. THE Backend SHALL read all configuration from environment variables without hardcoded values
6. THE Frontend SHALL read API base URL and SignalR hub URL from environment variables

### Requirement 30: Code Quality and Testing

**User Story:** As a developer, I want comprehensive unit tests and code quality analysis, so that the codebase remains maintainable and bugs are caught early.

#### Acceptance Criteria

1. WHEN unit tests run THEN xUnit SHALL execute all test cases and report results
2. WHEN testing repository methods THEN the tests SHALL use in-memory SQLite provider for isolation
3. WHEN testing services with external dependencies THEN the tests SHALL use Moq to mock dependencies
4. WHEN SonarQube analysis runs THEN the analysis SHALL check code quality, coverage, and security vulnerabilities
5. WHEN code is pushed to develop or main branches THEN GitHub Actions SHALL run SonarQube analysis automatically
6. THE Backend tests SHALL cover authentication, task operations, dashboard queries, AI services, and exception handling
7. THE Backend tests SHALL achieve meaningful coverage of critical paths without testing framework code
