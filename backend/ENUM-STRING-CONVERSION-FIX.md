# Enum String Conversion Fix - CRITICAL

## The Real Problem

The 400 error wasn't about missing fields - it was about **JSON serialization configuration**.

### Error Message
```json
{
  "errors": {
    "command": [
      "The command field is required."
    ],
    "$.priority": [
      "The JSON value could not be converted to Velocify.Domain.Enums.TaskPriority"
    ]
  }
}
```

### Root Cause

**ASP.NET Core by default expects enum values as integers, not strings.**

When you send:
```json
{
  "priority": "High"
}
```

ASP.NET Core tries to deserialize "High" as an integer and fails. It expects:
```json
{
  "priority": 1
}
```

Because the `TaskPriority` enum is:
```csharp
public enum TaskPriority
{
    Critical,  // 0
    High,      // 1
    Medium,    // 2
    Low        // 3
}
```

### The "command field is required" Error

This misleading error occurs because when JSON deserialization fails, ASP.NET Core's model binding treats the entire request body as invalid, and the `[FromBody] CreateTaskCommand command` parameter becomes null. The framework then reports "command is required" instead of the actual deserialization error.

## The Fix

### Code Change in `Program.cs`

**BEFORE:**
```csharp
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
```

**AFTER:**
```csharp
// Add services to the container.
// Configure JSON serialization to accept string enum values (e.g., "High" instead of 1)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddInfrastructure(builder.Configuration);
```

### What This Does

`JsonStringEnumConverter` tells ASP.NET Core to:
1. **Accept string values** for enums in JSON requests (e.g., `"High"`, `"Medium"`)
2. **Return string values** for enums in JSON responses (instead of integers)
3. **Case-insensitive matching** (so `"high"`, `"High"`, `"HIGH"` all work)

## Testing After Fix

### Deploy the Fix

```bash
git add backend/Velocify.API/Program.cs
git commit -m "Add JsonStringEnumConverter to support string enum values in API"
git push origin main
```

Wait for GitHub Actions to complete (~5-10 minutes).

### Test Request (Now Works!)

```json
{
  "title": "Test Task from Azure",
  "description": "This task was created to test the Azure deployment",
  "priority": "High",
  "category": "Development",
  "assignedToUserId": "f51ffdaf-a91a-4f4a-abf1-8b58260e993b",
  "dueDate": "2026-05-01T12:00:00Z",
  "estimatedHours": 5.0,
  "tags": "testing,azure,deployment"
}
```

### Alternative: Use Numeric Values (Works Without Fix)

If you don't want to wait for deployment, you can use numeric values:

```json
{
  "title": "Test Task from Azure",
  "description": "This task was created to test the Azure deployment",
  "priority": 1,
  "category": 0,
  "assignedToUserId": "f51ffdaf-a91a-4f4a-abf1-8b58260e993b",
  "dueDate": "2026-05-01T12:00:00Z",
  "estimatedHours": 5.0,
  "tags": "testing,azure,deployment"
}
```

Where:
- **Priority**: `0` = Critical, `1` = High, `2` = Medium, `3` = Low
- **Category**: `0` = Development, `1` = Design, `2` = Marketing, `3` = Operations, `4` = Research, `5` = Other

## Why This Wasn't Caught Earlier

1. **Tests use InMemory database** - They might be using numeric values or have different serialization
2. **No integration tests** for the API endpoints with actual HTTP requests
3. **Default ASP.NET Core behavior** - Numeric enums are the default, string enums require explicit configuration

## Impact

This fix affects **all enum fields** in the API:
- ✅ `TaskPriority` - Now accepts "Critical", "High", "Medium", "Low"
- ✅ `TaskStatus` - Now accepts "Pending", "InProgress", "Completed", "Cancelled", "Blocked"
- ✅ `TaskCategory` - Now accepts "Development", "Design", "Marketing", "Operations", "Research", "Other"
- ✅ `UserRole` - Now returns "Member", "Admin", "SuperAdmin" in responses

## Response Format Change

### Before Fix (Numeric)
```json
{
  "id": "abc123...",
  "title": "Test Task",
  "priority": 1,
  "status": 0,
  "category": 0,
  "assignedTo": {
    "role": 2
  }
}
```

### After Fix (String)
```json
{
  "id": "abc123...",
  "title": "Test Task",
  "priority": "High",
  "status": "Pending",
  "category": "Development",
  "assignedTo": {
    "role": "Member"
  }
}
```

## Summary

✅ **Fixed:** Added `JsonStringEnumConverter` to `Program.cs`  
✅ **Impact:** All enum fields now accept and return string values  
✅ **Action Required:** Deploy the updated `Program.cs` to Azure  
✅ **Workaround:** Use numeric values until deployment completes  

This was a **backend configuration issue**, not a documentation issue. The fix is a one-line change that enables string enum serialization across the entire API.
