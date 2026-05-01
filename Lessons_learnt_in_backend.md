# Backend Lessons Learned

## Architecture and Design Decisions

I built the Velocify backend using Clean Architecture principles with clear separation of concerns across four layers: API, Application, Domain, and Infrastructure. I chose this architecture because it provides excellent maintainability and testability while keeping business logic independent of frameworks and external dependencies.

I implemented the CQRS pattern using MediatR to separate read and write operations. This decision proved valuable as it allowed me to optimize queries independently from commands and made the codebase more maintainable.

I used the Repository pattern for data access, which abstracted database operations and made testing significantly easier. I created generic repository interfaces for common operations while implementing specific repositories for complex queries.

## Database Management

I configured Entity Framework Core with Azure SQL Database for production. I learned that connection pooling configuration is critical for Azure App Service F1 tier, so I set Min Pool Size to 2 and Max Pool Size to 100 to optimize memory usage within the 1GB RAM limit.

I implemented comprehensive audit logging by tracking all entity changes with CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, DeletedBy, and DeletedAt fields. I stored audit logs in separate tables to maintain a complete history of all modifications.

I used EF Core migrations for schema management, which automated database updates during deployment through the GitHub Actions workflow. I configured the InMemory database provider for tests to ensure fast, isolated test execution without requiring external database dependencies.

I optimized query performance by using AsNoTracking for read-only queries, implementing efficient pagination, adding proper indexes on frequently queried columns, and using compiled queries for dashboard statistics.

## API Design and Implementation

I designed RESTful APIs following best practices with appropriate HTTP methods, consistent URL patterns, proper status codes, and API versioning. I used DTOs to decouple the API layer from domain models, which provided flexibility to change internal structures without breaking client contracts.

I implemented comprehensive request validation using FluentValidation at the API boundary. I validated all incoming requests, returned detailed validation errors, and prevented invalid data from reaching the database.

I created a global exception handler middleware that mapped exceptions to appropriate HTTP status codes, provided meaningful error messages, logged all errors for debugging, and returned ProblemDetails format responses.

I designed API endpoints with clear naming conventions including GET /api/tasks for listing tasks with filtering and pagination, POST /api/tasks for creating new tasks, GET /api/tasks/{id} for retrieving specific tasks, PUT /api/tasks/{id} for updating tasks, DELETE /api/tasks/{id} for soft deleting tasks, POST /api/tasks/{id}/comments for adding comments, and GET /api/dashboard/stats for dashboard metrics. I ensured consistent response formats across all endpoints with proper HTTP status codes and error handling.

## Authentication and Authorization

I implemented JWT-based authentication with secure token generation, refresh token mechanism, token validation on protected endpoints, and graceful handling of token expiration. I configured tokens to expire in 15 minutes with refresh tokens valid for 7 days.

I implemented role-based access control with three roles: SuperAdmin, Admin, and Member. I used policy-based authorization to protect endpoints with role requirements and implemented resource-based authorization where users could only modify their own resources.

## Logging and Monitoring

I implemented structured logging with Serilog, configuring different log levels for development and production. I logged important events and operations with contextual information and structured logs for easy querying. I set 7-day log retention to manage disk space on the F1 tier.

I created comprehensive audit trails that tracked all entity changes, recorded who made changes and when, stored audit logs in separate tables, and made audit logs queryable for reporting.

I implemented health checks for database connectivity, LangChain API availability, and disk space monitoring. I configured health checks to minimize cold starts on the F1 tier.

## AI Integration with LangChain

I integrated LangChain.NET for AI-powered features including natural language task creation, smart task decomposition, daily digest generation, workload balancing suggestions, semantic search, and CSV import normalization.

I initially configured the system for OpenAI but later switched to Groq API for faster inference and cost-effectiveness. I configured the system to use the openai/gpt-oss-120b model with competitive pricing and excellent performance.

I implemented caching for AI responses to minimize API calls and reduce CPU time consumption on the F1 tier. I added proper error handling for AI service failures to ensure the application remained functional even when AI features were unavailable.

## Testing Strategy

I wrote extensive unit tests for business logic in the Application layer, mocking dependencies appropriately and testing edge cases and error conditions. I used xUnit as the testing framework with FluentAssertions for readable assertions.

I implemented integration tests for repository implementations, database operations, and API endpoints. I used the InMemory database for tests to ensure fast execution and isolation.

I organized tests to mirror the source code structure, used descriptive test names following the AAA pattern, and kept tests independent and isolated.

## Deployment and DevOps

I deployed to Azure App Service F1 tier, which required careful optimization due to strict limitations: 60 minutes CPU time per day, 1GB RAM, no Always On feature, and 1GB disk space.

I implemented automated deployment using GitHub Actions with workflows that built the project, ran tests, published artifacts, deployed to Azure App Service, and ran EF Core migrations automatically.

I configured environment variables in Azure App Service for database connection strings, JWT settings, LangChain API keys, and CORS origins. I stored secrets in Azure configuration rather than in code.

I configured CORS properly to allow specific frontend origins, configured allowed methods and headers, handled preflight requests correctly, and tested CORS in different environments.

I created deployment scripts for both Windows and Linux environments to streamline the deployment process. I configured web.config for IIS hosting with proper request handling, security headers, and URL rewriting rules. I set up health check endpoints to monitor application status and prevent cold starts on the F1 tier.

## Critical Issues and Fixes

I encountered and fixed several critical issues during development:

I fixed user ID handling issues where user IDs were not being extracted correctly from JWT claims. I ensured consistent user ID extraction, validated user IDs before database operations, handled missing user IDs gracefully, and added comprehensive logging for debugging.

I resolved enum serialization issues by configuring JsonStringEnumConverter to support both string and numeric enum values. I documented all enum values and handled invalid enum values appropriately.

I fixed task status update problems by validating status transitions, updating audit logs correctly, handling concurrent updates with row versioning, and returning updated entities.

I resolved test failures by fixing Serilog configuration in tests, properly mocking dependencies, handling async operations correctly, and isolating test data.

## Performance Optimization

I optimized for Azure F1 tier limitations by minimizing AI calls through caching, optimizing database queries with compiled queries and indexed views, reducing logging in production to Warning level for framework code, configuring connection pooling, and implementing health checks to prevent unnecessary cold starts.

I implemented efficient pagination for large result sets, used Include for eager loading related data to avoid N+1 queries, added indexes on frequently queried columns, and profiled queries to identify bottlenecks.

I used async/await throughout the codebase for all I/O operations, avoided blocking calls, used ConfigureAwait appropriately, and handled async exceptions properly.

## Security Best Practices

I implemented comprehensive security measures including password hashing using BCrypt, input validation and sanitization, protection against SQL injection through parameterized queries, and rate limiting considerations.

I handled sensitive data carefully by never logging sensitive information, encrypting data at rest and in transit, implementing proper access controls, and following GDPR principles.

I configured HTTPS enforcement automatically on Azure App Service, implemented security headers in web.config, validated all inputs at API boundaries, and used DTOs to prevent over-posting attacks.

## Key Takeaways

I learned that Clean Architecture provides excellent maintainability and makes testing significantly easier. I found that proper error handling is crucial for production systems and saves debugging time.

I discovered that comprehensive logging is invaluable for troubleshooting production issues. I realized that automated testing provides confidence when making changes and prevents regressions.

I understood that security must be built in from the start rather than added later. I learned that performance optimization should be measured and data-driven rather than based on assumptions.

I found that good documentation prevents confusion and helps onboard new developers. I discovered that consistent patterns improve code quality and make the codebase more maintainable.

## Future Improvements

I plan to implement distributed caching with Redis for better performance across multiple instances. I want to add more comprehensive monitoring with Application Insights for production observability.

I aim to implement event sourcing for the audit trail to provide complete history reconstruction. I intend to add GraphQL support alongside REST for more flexible querying.

I plan to implement background job processing with Hangfire for long-running tasks. I want to add more sophisticated authorization with resource-based policies.

I aim to improve the API versioning strategy to support multiple versions simultaneously. I plan to implement rate limiting per user to prevent abuse and ensure fair usage.

I continue to refine the backend based on production experience, user feedback, and evolving requirements while maintaining the high code quality standards established during initial development.

## Azure Deployment Configuration

I configured Azure App Service with specific environment variable naming conventions using double underscores for nested configuration keys. I learned that Azure requires ConnectionStrings__DefaultConnection instead of the colon notation used in local development.

I set up all required environment variables including JWT settings with proper secret key generation, LangChain API key for AI features, and CORS allowed origins for frontend integration. I configured connection strings with critical pooling parameters for F1 tier optimization.

I documented the complete Azure configuration reference including application settings, connection strings, general settings, and monitoring alerts. I created deployment checklists to ensure all configuration steps were completed correctly.

## Database Setup and Migrations

I configured Azure SQL Database with proper firewall rules to allow Azure services access. I set up automated migration execution through GitHub Actions workflow to apply schema changes during deployment.

I learned the importance of connection pooling configuration with Min Pool Size 2 to keep connections warm and Max Pool Size 100 to prevent memory exhaustion on F1 tier. I documented the complete database setup process including connection string format and migration commands.

I created comprehensive database documentation covering production Azure SQL setup, development SQL Server LocalDB configuration, and CI/CD InMemory database for tests. I established clear separation between environments to ensure proper testing and deployment.

## API Endpoint Corrections

I discovered and corrected several API endpoint routing issues during deployment verification. I found that dashboard endpoints used different route names than initially documented, requiring updates to test files and documentation.

I corrected the dashboard summary endpoint from /statistics to /summary and the velocity endpoint from /task-distribution to /velocity. I documented all correct endpoint routes with proper HTTP methods and authentication requirements.

I created comprehensive API endpoint reference documentation listing all routes with correct names, methods, authentication requirements, and query parameters. I included common response status codes and error formats for developer reference.

## Critical Bug Fixes

I fixed a critical foreign key constraint violation in the delete task operation. I discovered that the DeleteTaskCommandHandler was not passing the userId parameter to the repository, causing Guid.Empty to be inserted into audit logs.

I updated the ITaskRepository interface to accept deletedByUserId parameter and modified the TaskRepository implementation to use the provided userId. I updated the DeleteTaskCommandHandler to pass the request.DeletedByUserId to the repository method.

I fixed duplicate email handling in user profile updates by adding email uniqueness validation before database operations. I updated the GlobalExceptionHandler to map InvalidOperationException to 409 Conflict status code with clear error messages.

I corrected multiple user ID extraction issues across TasksController endpoints. I ensured that UpdateTask, UpdateTaskStatus, DeleteTask, CreateComment, and DeleteComment all properly extract and set user IDs from JWT tokens before sending commands to MediatR.

## Enum Serialization Configuration

I discovered that ASP.NET Core by default expects enum values as integers rather than strings. I added JsonStringEnumConverter to the AddControllers configuration in Program.cs to enable string enum serialization.

I documented all enum values with both string and numeric representations for Priority, Status, and Category enums. I created comprehensive enum value reference documentation to help developers use the correct values in API requests.

I learned that the misleading "command field is required" error actually indicated JSON deserialization failure when enum values were incorrect. I updated documentation to clarify this common error scenario.

## CORS Configuration

I implemented CORS configuration reading allowed origins from environment variables. I configured the CORS policy to allow specific origins with credentials support, which is required for JWT authentication and SignalR.

I documented the CORS configuration process including environment variable format, policy settings, and security considerations. I created test files to verify CORS headers in preflight and actual requests.

I learned that CORS must be configured before Authentication and Authorization middleware in the pipeline. I ensured that AllowCredentials requires specific origins and cannot be used with wildcard origins.

## Testing Infrastructure

I fixed integration test failures caused by Serilog logger freezing issues when running tests in parallel. I disabled xUnit test parallelization using assembly-level CollectionBehavior attribute to prevent race conditions.

I configured TestWebApplicationFactory to use InMemory database for fast, isolated test execution. I learned that tests should use InMemory database while production uses Azure SQL Server, which is standard practice.

I marked LocalDB-dependent tests with Skip attribute to allow them to run locally on Windows while being skipped in Linux CI environments. I updated CorrelationIdMiddleware tests to properly trigger OnStarting callbacks by writing to response body.

## Deployment Automation

I configured GitHub Actions workflow for automated deployment including build, test, migration, and deployment steps. I set up required GitHub secrets for Azure SQL connection string, App Service name, and Azure credentials.

I created deployment scripts for both Windows and Linux environments to streamline manual deployments when needed. I configured the workflow to run migrations automatically after deployment using the Azure SQL connection string.

I documented the complete deployment process including GitHub secret configuration, workflow monitoring, and post-deployment verification steps. I created verification checklists to ensure all critical functionality works after deployment.

## Azure F1 Tier Optimization

I learned that Azure App Service F1 tier has strict limitations including 60 minutes CPU time per day, 1GB RAM, no Always On feature, and 1GB disk space. I optimized the application to work within these constraints.

I configured connection pooling parameters specifically for F1 tier memory limitations. I implemented compiled queries and indexed views to reduce CPU time consumption. I set production logging to Warning level for framework code to minimize log processing overhead.

I configured health checks with 5-minute intervals to balance keeping the app warm with CPU time consumption. I documented all F1 tier limitations and optimization strategies for future reference.

I created monitoring alerts for CPU time usage at 50 minutes to provide 10-minute warning before hitting the daily limit. I documented when to upgrade to B1 tier based on CPU usage, cold start impact, and feature requirements.

## Environment Variable Management

I learned the critical difference between Azure App Service environment variable naming and local development configuration. I documented that Azure uses double underscores while local appsettings.json uses colons for nested keys.

I created comprehensive environment variable documentation covering all required settings including database connection strings, JWT configuration, LangChain API keys, and CORS origins. I included examples for both local and Azure formats.

I documented security best practices for environment variable management including never committing secrets to source control, rotating passwords quarterly, and using Azure Key Vault for production secrets.

## Health Check Implementation

I implemented comprehensive health checks for database connectivity, LangChain API availability, and disk space monitoring. I configured health checks to support both LangChain and OpenAI API key naming conventions.

I learned that health checks help minimize cold starts on F1 tier by keeping the app warm. I configured appropriate check intervals to balance availability with CPU time consumption.

I created health check documentation explaining each check's purpose, expected responses, and troubleshooting steps for common failures. I included correlation IDs in health check responses for debugging.

## Documentation Strategy

I created extensive documentation covering all aspects of backend development, deployment, and operations. I organized documentation by topic including API endpoints, environment variables, deployment procedures, and troubleshooting guides.

I learned that good documentation prevents confusion during deployment and helps new developers understand the system quickly. I maintained documentation alongside code changes to ensure accuracy.

I created quick reference guides for common tasks including deployment verification, environment variable configuration, and troubleshooting common issues. I included examples and expected outputs to make documentation actionable.

## Audit Logging Implementation

I implemented comprehensive audit logging for all entity changes including who made changes, when changes occurred, what changed, and previous values. I stored audit logs in separate tables with proper foreign key relationships.

I learned the importance of extracting user IDs from JWT tokens at the controller level rather than in handlers. I ensured all audit log entries have valid user IDs to maintain referential integrity.

I configured audit log table partitioning by month to optimize query performance for historical data. I implemented soft delete functionality with proper audit trail for deleted entities.

## API Versioning Strategy

I implemented API versioning using URL path segments with /api/v1 prefix for all endpoints. I configured the API to return supported versions in response headers for client discovery.

I documented the versioning strategy and planned for future version support. I learned that versioning should be considered from the start to enable backward-compatible changes.

## Error Handling and Logging

I implemented global exception handler middleware that maps domain exceptions to appropriate HTTP status codes. I configured the handler to return ProblemDetails format responses with correlation IDs for debugging.

I learned that correlation IDs are essential for tracing requests through logs and identifying issues in production. I implemented CorrelationIdMiddleware to generate and propagate correlation IDs through the request pipeline.

I configured Serilog with structured logging to enable efficient log querying and analysis. I set appropriate log levels for different environments and configured 7-day retention to manage disk space.

## Performance Monitoring

I implemented performance monitoring using MediatR pipeline behaviors for logging and performance tracking. I configured behaviors to log slow queries and commands for optimization opportunities.

I learned the importance of monitoring CPU time usage on F1 tier to avoid hitting daily limits. I created alerts and dashboards to track key performance metrics.

I documented performance optimization strategies including compiled queries, indexed views, connection pooling, and caching. I measured the impact of each optimization to ensure data-driven decisions.
