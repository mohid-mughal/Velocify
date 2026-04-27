# API Critical Fixes - Production Issues

## Issues Fixed

### 1. Delete Task (500 Error) - Foreign Key Constraint Violation ✅
**Problem**: `FK_TaskAuditLogs_Users_ChangedByUserId` constraint violation  
**Root Cause**: DeleteTaskCommandHandler not passing userId to repository, causing Guid.Empty to be inserted  
**Fix**: 
- Updated `ITaskRepository.Delete()` to accept `deletedByUserId` parameter
- Updated `TaskRepository.Delete()` to use the provided userId instead of Guid.Empty
- Updated `DeleteTaskCommandHandler` to pass `request.DeletedByUserId` to repository
- Updated test in `TaskRepositoryTests.cs` to pass userId

**Files Modified**:
- `backend/Velocify.Application/Interfaces/ITaskRepository.cs`
- `backend/Velocify.Infrastructure/Repositories/TaskRepository.cs`
- `backend/Velocify.Application/Commands/Tasks/DeleteTaskCommandHandler.cs`
- `backend/Velocify.Tests/Infrastructure/Repositories/TaskRepositoryTests.cs`

### 2. Update User Profile (500 Error) - Duplicate Email ✅
**Problem**: Unique constraint violation on `IX_Users_Email`  
**Root Cause**: No email uniqueness check before update  
**Fix**: 
- Added email uniqueness validation in `UpdateCurrentUserCommandHandler`
- Added `InvalidOperationException` handling in `GlobalExceptionHandler` to return 409 Conflict
- Updated test file to use unique email address

**Files Modified**:
- `backend/Velocify.Application/Commands/Users/UpdateCurrentUserCommandHandler.cs`
- `backend/Velocify.API/Middleware/GlobalExceptionHandler.cs`
- `backend/test-azure-deployment.http`

### 3. Dashboard Endpoints (404 Not Found) ✅
**Problem**: Routes `/api/v1/dashboard/statistics` and `/api/v1/dashboard/task-distribution` return 404  
**Root Cause**: Endpoints exist but with different route names  
**Actual Routes**:
- `/api/v1/dashboard/summary` (not statistics)
- `/api/v1/dashboard/velocity` (not task-distribution)
- `/api/v1/dashboard/workload` (Admin only)
- `/api/v1/dashboard/overdue`

**Fix**: Updated test file with correct endpoint names

**Files Modified**:
- `backend/test-azure-deployment.http`

### 4. Mark Notification as Read (400 Bad Request) ✅
**Problem**: Literal string `{notificationId}` sent instead of actual GUID  
**Root Cause**: Variable not properly defined in test file  
**Fix**: Added `@notificationId` variable declaration with instructions

**Files Modified**:
- `backend/test-azure-deployment.http`

### 5. Mark All Notifications (404 Not Found) ✅
**Problem**: Route `/api/v1/notifications/mark-all-read` returns 404  
**Root Cause**: Actual route is `/api/v1/notifications/read-all`  
**Fix**: Updated test file with correct route and HTTP method (PATCH, not POST)

**Files Modified**:
- `backend/test-azure-deployment.http`

### 6. OpenAI API Key Configuration ℹ️
**Problem**: Background sentiment analysis failing  
**Root Cause**: OpenAI:ApiKey not configured  
**Fix**: Already documented in `GROQ-API-SETUP.md` - no code changes needed

## Deployment Instructions

1. **Build and test locally**:
   ```bash
   cd backend
   dotnet build
   dotnet test
   ```

2. **Deploy to Azure**:
   ```bash
   # Using GitHub Actions (recommended)
   git add .
   git commit -m "Fix: Critical API issues - delete task, duplicate email, endpoint routes"
   git push origin main
   
   # Or manual deployment
   cd backend/Velocify.API
   dotnet publish -c Release
   # Deploy publish folder to Azure App Service
   ```

3. **Verify fixes**:
   - Run tests in `backend/test-azure-deployment.http`
   - Check Azure Application Insights for any remaining errors

## Testing Checklist

After deployment, verify:
- ✅ DELETE /api/v1/tasks/{taskId} returns 204 No Content (not 500)
- ✅ PUT /api/v1/users/me with duplicate email returns 409 Conflict (not 500)
- ✅ GET /api/v1/dashboard/summary returns 200 OK (not 404)
- ✅ GET /api/v1/dashboard/velocity returns 200 OK (not 404)
- ✅ PATCH /api/v1/notifications/read-all returns 204 No Content (not 404)
- ✅ PATCH /api/v1/notifications/{id}/read with valid ID returns 204 No Content (not 400)

## Expected Behavior Changes

### Delete Task
**Before**: 500 Internal Server Error with FK constraint violation  
**After**: 204 No Content, task soft-deleted with proper audit log

### Update User Profile
**Before**: 500 Internal Server Error when email already exists  
**After**: 409 Conflict with message "Email 'x@example.com' is already in use by another user."

### Dashboard Endpoints
**Before**: 404 Not Found  
**After**: 200 OK with dashboard data

### Notifications
**Before**: 404 Not Found or 400 Bad Request  
**After**: 204 No Content when marking as read
