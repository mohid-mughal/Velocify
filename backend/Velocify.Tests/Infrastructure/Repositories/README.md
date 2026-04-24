# Repository Tests

This directory contains comprehensive unit tests for the repository layer, covering CRUD operations, query filters, and soft delete behavior.

## Test Coverage

### TaskRepositoryTests
Tests for TaskRepository covering:
- **CRUD Operations**: Create, Update, Delete tasks with proper defaults and timestamps
- **Status Updates**: Verifies CompletedAt timestamp is set when status changes to Completed
- **Soft Deletes**: Verifies IsDeleted flag is set without removing database records
- **Query Filters**: Tests filtering by status, priority, category, assigned user, search term, and due date
- **Soft Delete Filtering**: Verifies soft-deleted tasks are excluded from normal queries
- **Comments**: Create, delete, and sentiment analysis for task comments
- **Audit Logging**: Verifies all changes are recorded in TaskAuditLog table

### UserRepositoryTests
Tests for UserRepository covering:
- **CRUD Operations**: Create, Update, Delete users with proper defaults and timestamps
- **Email Lookup**: Tests GetByEmail using compiled query
- **Soft Deletes**: Verifies IsActive flag is set to false without removing records
- **Password Security**: Tests BCrypt password hashing and verification
- **Query Filters**: Verifies only active users are returned by GetList
- **Pagination**: Tests page size limits and page navigation

## Testing Approach

- **In-Memory Database**: Uses EF Core InMemory provider for fast, isolated testing
- **AutoMapper**: Uses actual mapping profiles for realistic DTO conversion
- **Business Rules**: Validates timestamp assignment, soft delete behavior, and data integrity
- **Requirements**: Satisfies requirements 30.1-30.7 for comprehensive unit testing

## Known Limitations

- **Concurrency Testing**: InMemory database does not support RowVersion concurrency tokens, so optimistic concurrency tests are skipped
- **SQL-Specific Features**: Tests do not cover SQL Server-specific features like indexed views, partitioning, or stored procedures (these require integration tests)

## Running Tests

```bash
# Run all repository tests
dotnet test --filter "FullyQualifiedName~Velocify.Tests.Infrastructure.Repositories"

# Run specific test class
dotnet test --filter "FullyQualifiedName~TaskRepositoryTests"
dotnet test --filter "FullyQualifiedName~UserRepositoryTests"
```
