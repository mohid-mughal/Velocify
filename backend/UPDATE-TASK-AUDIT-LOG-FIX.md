# Update Task Audit Log Fix - Foreign Key Constraint Error

## The Problem

When trying to update a task status, you got this error:

```
Microsoft.Data.SqlClient.SqlException: The MERGE statement conflicted with the FOREIGN KEY constraint "FK_TaskAuditLogs_Users_ChangedByUserId". 
The conflict occurred in database "velocify-free-sql-db-0695809", table "dbo.Users", column 'Id'.
```

### The Request
```json
{
  "Id": "e30d8d9a-4ffa-47d4-810c-5d979b8a22cb",
  "Status": 2,
  "UpdatedByUserId": "00000000-0000-0000-0000-000000000000"
}
```

### What Happened

1. You sent a PATCH request to update task status
2. The backend tried to create an audit log entry to track the change
3. The `UpdatedByUserId` was an empty GUID (`00000000-0000-0000-0000-000000000000`)
4. Azure SQL tried to create a foreign key relationship to a user with that ID
5. No user exists with ID `00000000-0000-0000-0000-000000000000`
6. Foreign key constraint violation → 500 Internal Server Error

## Root Cause

The `TasksController` was NOT extracting the user ID from the JWT token and setting it on the command before sending to MediatR.

### Affected Endpoints

1. **PATCH /api/v1/tasks/{id}/status** - UpdateTaskStatus
2. **PUT /api/v1/tasks/{id}** - UpdateTask

Both endpoints had the same bug.

## The Fix

### File: `backend/Velocify.API/Controllers/TasksController.cs`

#### UpdateTaskStatus Endpoint

**BEFORE:**
```csharp
public async Task<ActionResult<TaskDto>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusCommand command)
{
    command.Id = id;
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

**AFTER:**
```csharp
public async Task<ActionResult<TaskDto>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusCommand command)
{
    command.Id = id;
    command.UpdatedByUserId = GetCurrentUserId();  // ← ADDED THIS LINE
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

#### UpdateTask Endpoint

**BEFORE:**
```csharp
public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskCommand command)
{
    command.Id = id;
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

**AFTER:**
```csharp
public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskCommand command)
{
    command.Id = id;
    command.UpdatedByUserId = GetCurrentUserId();  // ← ADDED THIS LINE
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### The GetCurrentUserId() Helper

This helper method already exists in the controller (line ~240):

```csharp
private Guid GetCurrentUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        throw new UnauthorizedAccessException("Unable to identify the current user from the authentication token.");
    }
    return userId;
}
```

It extracts the user ID from the JWT token's `sub` claim (ClaimTypes.NameIdentifier).

## Why This Happened

The command classes have comments saying "Set by handler from authenticated user context":

```csharp
public class UpdateTaskStatusCommand : IRequest<TaskDto>
{
    public Guid Id { get; set; }
    public TaskStatus Status { get; set; }
    
    // Set by handler from authenticated user context
    public Guid UpdatedByUserId { get; set; }
}
```

But the controller was NOT setting it. The comment was misleading - it should have said "Set by **controller** from authenticated user context".

The `CreateTask` endpoint correctly sets `CreatedByUserId`:
```csharp
public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskCommand command)
{
    command.CreatedByUserId = GetCurrentUserId();  // ✅ Correctly set
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
}
```

But the update endpoints were missing this line.

## Impact

This fix affects:
- ✅ **Task status updates** - Now properly tracks who changed the status
- ✅ **Task updates** - Now properly tracks who updated the task
- ✅ **Audit logs** - Will correctly record the user who made changes
- ✅ **Foreign key constraints** - No more constraint violations

## Testing After Fix

### Deploy the Fix

```bash
git add backend/Velocify.API/Controllers/TasksController.cs
git commit -m "Fix: Set UpdatedByUserId from JWT token in update endpoints"
git push origin main
```

Wait for GitHub Actions to deploy (~5-10 minutes).

### Test Update Task Status

```http
PATCH https://velocify.azurewebsites.net/api/v1/tasks/{taskId}/status
Authorization: Bearer YOUR_ACCESS_TOKEN
Content-Type: application/json

{
  "status": 2
}
```

**Expected Response:** `200 OK` with updated task ✅

### Test Update Task

```http
PUT https://velocify.azurewebsites.net/api/v1/tasks/{taskId}
Authorization: Bearer YOUR_ACCESS_TOKEN
Content-Type: application/json

{
  "title": "Updated Task",
  "description": "Updated description",
  "priority": 1,
  "category": 0,
  "status": 2,
  "assignedToUserId": "e0d881b0-ba1c-40e1-b22d-f7bdd0be1714",
  "dueDate": "2026-05-02T12:00:00Z",
  "estimatedHours": 3.0,
  "actualHours": 1.5,
  "tags": "updated,testing"
}
```

**Expected Response:** `200 OK` with updated task ✅

### Verify Audit Log

```http
GET https://velocify.azurewebsites.net/api/v1/tasks/{taskId}/history
Authorization: Bearer YOUR_ACCESS_TOKEN
```

**Expected Response:** Audit log entries showing your user ID as `ChangedByUserId` ✅

## Security Note

This fix also improves security:
- **Before:** Client could potentially send any `UpdatedByUserId` in the request body
- **After:** Server always uses the authenticated user's ID from the JWT token
- **Result:** Audit logs are trustworthy and cannot be spoofed

## Summary

✅ **Fixed:** Added `command.UpdatedByUserId = GetCurrentUserId();` to both update endpoints  
✅ **Impact:** Task updates and status changes now work correctly  
✅ **Security:** Audit logs now trustworthy (server-side user ID extraction)  
✅ **Action Required:** Deploy the updated TasksController.cs  

**Credit:** Thanks to your "naive person" for the excellent debugging! 🎯
