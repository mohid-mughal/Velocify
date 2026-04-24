# Contributing to Velocify Platform

Thank you for your interest in contributing to the Velocify Platform! This document provides guidelines and best practices for contributing to this project.

## Table of Contents

- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Branch Naming Convention](#branch-naming-convention)
- [Commit Message Format](#commit-message-format)
- [Pull Request Process](#pull-request-process)
- [Code Style Guidelines](#code-style-guidelines)
- [Testing Requirements](#testing-requirements)
- [Azure F1 Tier Considerations](#azure-f1-tier-considerations)

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8 SDK** - `dotnet --version` should show 8.x.x
- **Node.js 18+** - `node --version` should show 18.x.x or higher
- **Git** - For version control
- **Azure CLI** (optional) - For Azure deployment tasks
- **SQL Server Management Studio or Azure Data Studio** - For database management

### Initial Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/velocify-platform.git
   cd velocify-platform
   ```

3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/ORIGINAL-OWNER/velocify-platform.git
   ```

4. **Install backend dependencies**:
   ```bash
   cd backend
   dotnet restore
   ```

5. **Install frontend dependencies**:
   ```bash
   cd frontend
   npm install
   ```

6. **Set up environment variables** (see README.md for details)

## Development Workflow

We follow a **feature branch workflow** with the following branches:

- **`main`** - Production-ready code, protected branch
- **`develop`** - Integration branch for features, protected branch
- **`feature/*`** - Feature development branches
- **`bugfix/*`** - Bug fix branches
- **`hotfix/*`** - Critical production fixes

### Typical Workflow

1. **Sync with upstream**:
   ```bash
   git checkout develop
   git pull upstream develop
   ```

2. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes** and commit regularly

4. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Open a Pull Request** to the `develop` branch

## Branch Naming Convention

Use descriptive branch names that follow this pattern:

### Format

```
<type>/<short-description>
```

### Types

- **`feature/`** - New features or enhancements
  - Example: `feature/add-task-priority-filter`
  - Example: `feature/ai-workload-balancing`

- **`bugfix/`** - Bug fixes for non-critical issues
  - Example: `bugfix/fix-task-status-update`
  - Example: `bugfix/correct-sentiment-calculation`

- **`hotfix/`** - Critical production bug fixes
  - Example: `hotfix/fix-auth-token-expiration`
  - Example: `hotfix/resolve-database-connection-leak`

- **`refactor/`** - Code refactoring without changing functionality
  - Example: `refactor/extract-ai-service-interface`
  - Example: `refactor/optimize-dashboard-queries`

- **`docs/`** - Documentation updates
  - Example: `docs/update-deployment-guide`
  - Example: `docs/add-api-examples`

- **`test/`** - Adding or updating tests
  - Example: `test/add-task-repository-tests`
  - Example: `test/improve-ai-service-coverage`

- **`chore/`** - Maintenance tasks, dependency updates
  - Example: `chore/update-dependencies`
  - Example: `chore/configure-sonarqube`

### Guidelines

- Use **lowercase** with **hyphens** to separate words
- Keep names **short but descriptive** (max 50 characters)
- Use **present tense** verbs (add, fix, update, not added, fixed, updated)
- Avoid special characters except hyphens

## Commit Message Format

We follow the **Conventional Commits** specification for clear and structured commit messages.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type

Must be one of the following:

- **`feat`** - A new feature
- **`fix`** - A bug fix
- **`docs`** - Documentation changes
- **`style`** - Code style changes (formatting, missing semicolons, etc.)
- **`refactor`** - Code refactoring without changing functionality
- **`perf`** - Performance improvements
- **`test`** - Adding or updating tests
- **`chore`** - Maintenance tasks, dependency updates
- **`ci`** - CI/CD configuration changes
- **`build`** - Build system or external dependency changes

### Scope (Optional)

The scope specifies the area of the codebase affected:

- **Backend scopes**: `auth`, `tasks`, `dashboard`, `ai`, `notifications`, `api`, `infrastructure`, `domain`
- **Frontend scopes**: `ui`, `components`, `store`, `api`, `routing`, `hooks`
- **General scopes**: `deps`, `config`, `deployment`, `docs`

### Subject

- Use **imperative mood** ("add" not "added" or "adds")
- **Lowercase** first letter
- **No period** at the end
- **Max 50 characters**

### Body (Optional)

- Explain **what** and **why**, not how
- Wrap at **72 characters**
- Separate from subject with a blank line

### Footer (Optional)

- Reference issues: `Closes #123`, `Fixes #456`
- Breaking changes: `BREAKING CHANGE: description`

### Examples

#### Simple Feature
```
feat(tasks): add priority filter to task list

Allows users to filter tasks by priority level (Critical, High, Medium, Low).
This improves task discovery and helps users focus on high-priority work.

Closes #42
```

#### Bug Fix
```
fix(auth): resolve token refresh race condition

Fixed an issue where concurrent refresh token requests could cause
authentication failures. Added locking mechanism to ensure only one
refresh operation occurs at a time.

Fixes #89
```

#### Documentation
```
docs(deployment): update Azure F1 tier setup guide

Added detailed instructions for CPU time monitoring and optimization
strategies for Azure App Service F1 tier deployments.
```

#### Refactoring
```
refactor(ai): extract LangChain service interface

Extracted ILangChainService interface to improve testability and
allow for easier mocking in unit tests.
```

#### Breaking Change
```
feat(api): change task status endpoint to PATCH

BREAKING CHANGE: Task status updates now use PATCH /api/v1/tasks/{id}/status
instead of PUT /api/v1/tasks/{id}. Update client code accordingly.

Closes #156
```

## Pull Request Process

### Before Opening a PR

1. **Sync with upstream** to avoid conflicts:
   ```bash
   git checkout develop
   git pull upstream develop
   git checkout your-branch
   git rebase develop
   ```

2. **Run all tests** and ensure they pass:
   ```bash
   # Backend tests
   cd backend
   dotnet test

   # Frontend build
   cd frontend
   npm run build
   ```

3. **Run code quality checks**:
   ```bash
   # Backend
   dotnet format

   # Frontend
   npm run lint
   npm run format
   ```

4. **Update documentation** if needed

### Opening a PR

1. **Push your branch** to your fork
2. **Open a Pull Request** on GitHub targeting the `develop` branch
3. **Fill out the PR template** completely:
   - Clear title following commit message format
   - Description of changes
   - Related issues
   - Testing performed
   - Screenshots (if UI changes)

### PR Title Format

Use the same format as commit messages:

```
<type>(<scope>): <description>
```

Examples:
- `feat(tasks): add bulk status update feature`
- `fix(auth): resolve token expiration issue`
- `docs(api): add endpoint examples`

### PR Description Template

```markdown
## Description
Brief description of what this PR does.

## Related Issues
Closes #123
Relates to #456

## Changes Made
- Change 1
- Change 2
- Change 3

## Testing Performed
- [ ] Unit tests added/updated
- [ ] Manual testing completed
- [ ] Tested on Azure F1 tier (if backend changes)

## Screenshots (if applicable)
[Add screenshots here]

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] Tests pass locally
- [ ] No new warnings introduced
```

### Review Process

1. **Automated checks** must pass:
   - GitHub Actions CI/CD
   - SonarQube analysis
   - All unit tests

2. **Code review** by at least one maintainer:
   - Code quality and style
   - Test coverage
   - Documentation completeness
   - Performance considerations (especially for F1 tier)

3. **Address feedback** and push updates

4. **Squash and merge** once approved

## Code Style Guidelines

### Backend (C#)

- Follow **Microsoft C# Coding Conventions**
- Use **PascalCase** for public members
- Use **camelCase** for private fields with `_` prefix
- Use **async/await** for asynchronous operations
- Add **XML documentation comments** for public APIs
- Keep methods **small and focused** (max 50 lines)
- Use **meaningful variable names**

Example:
```csharp
/// <summary>
/// Creates a new task with the specified details.
/// </summary>
/// <param name="command">The task creation command.</param>
/// <returns>The created task DTO.</returns>
public async Task<TaskDto> CreateTaskAsync(CreateTaskCommand command)
{
    // Implementation
}
```

### Frontend (TypeScript/React)

- Follow **Airbnb JavaScript Style Guide**
- Use **PascalCase** for components
- Use **camelCase** for functions and variables
- Use **functional components** with hooks
- Use **TypeScript** for type safety
- Keep components **small and focused**
- Use **meaningful prop names**

Example:
```typescript
interface TaskCardProps {
  task: Task;
  onStatusChange: (taskId: string, status: TaskStatus) => void;
}

export const TaskCard: React.FC<TaskCardProps> = ({ task, onStatusChange }) => {
  // Implementation
};
```

### General Guidelines

- **DRY** (Don't Repeat Yourself) - Extract reusable logic
- **SOLID** principles for backend architecture
- **Single Responsibility** - One class/function, one purpose
- **Meaningful names** - Code should be self-documenting
- **Comments** - Explain "why", not "what"
- **Error handling** - Always handle errors gracefully

## Testing Requirements

### Backend Tests

All backend changes must include appropriate tests:

- **Unit tests** for business logic
- **Integration tests** for database operations
- **Mock external dependencies** (LangChain, SignalR)
- **Test edge cases** and error conditions
- **Aim for meaningful coverage** of critical paths

Example test structure:
```csharp
public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesTask()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new CreateTaskCommand { /* ... */ };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
    }
}
```

### Frontend Tests

Frontend changes should include:

- **Component tests** for UI logic
- **Hook tests** for custom hooks
- **Integration tests** for critical flows
- **Mock API calls** using MSW or similar

### Running Tests

```bash
# Backend
cd backend
dotnet test

# Frontend
cd frontend
npm run test
```

## Azure F1 Tier Considerations

When contributing backend code, keep in mind the **Azure App Service F1 (Free) tier limitations**:

### CPU Time Quota (60 minutes/day)

- **Optimize queries** - Use compiled queries and indexed views
- **Minimize AI calls** - Cache results when possible
- **Efficient logging** - Use appropriate log levels
- **Connection pooling** - Already configured (Min=2, Max=100)

### Memory Limit (1 GB RAM)

- **Avoid memory leaks** - Dispose resources properly
- **Use streaming** for large data transfers
- **Limit collection sizes** in memory

### Cold Starts

- **Expect delays** after 20 minutes of inactivity
- **Optimize startup time** - Lazy load services when possible
- **Health checks** configured to minimize cold starts

### Testing on F1 Tier

If your changes affect performance:

1. **Deploy to a test F1 instance**
2. **Monitor CPU time usage** in Azure Portal
3. **Test cold start behavior**
4. **Verify memory usage** stays within limits

See `backend/AZURE-APP-SERVICE-SETUP.md` for detailed F1 tier documentation.

## Questions or Issues?

- **Open an issue** on GitHub for bugs or feature requests
- **Start a discussion** for questions or ideas
- **Check existing issues** before creating new ones

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing to Velocify Platform! 🚀
