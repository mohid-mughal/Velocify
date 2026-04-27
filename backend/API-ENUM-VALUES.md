# API Enum Values Reference

Quick reference for valid enum values when testing the Velocify API.

## Task Priority

Valid values for `priority` field (in enum order):

```json
"Critical"   // Value: 0
"High"       // Value: 1
"Medium"     // Value: 2
"Low"        // Value: 3
```

**Example:**
```json
{
  "priority": "High"
}
```

**Note:** You can also use numeric values: `0` = Critical, `1` = High, `2` = Medium, `3` = Low

## Task Status

Valid values for `status` field (in enum order):

```json
"Pending"      // Value: 0 (default for new tasks)
"InProgress"   // Value: 1
"Completed"    // Value: 2
"Cancelled"    // Value: 3
"Blocked"      // Value: 4
```

**Example:**
```json
{
  "status": "InProgress"
}
```

**Note:** New tasks default to "Pending" status

## Task Category

Valid values for `category` field (in enum order):

```json
"Development"   // Value: 0
"Design"        // Value: 1
"Marketing"     // Value: 2
"Operations"    // Value: 3
"Research"      // Value: 4
"Other"         // Value: 5
```

**Example:**
```json
{
  "category": "Development"
}
```

## User Role

Valid values for `role` field (read-only, set during registration):

```json
"Member"      // Default role (value: 2)
"Admin"       // Admin role (value: 1)
"SuperAdmin"  // Super admin role (value: 0)
```

**Note:** Role is automatically set to "Member" during registration. Only SuperAdmin can change user roles.

## Complete Task Creation Example

**IMPORTANT:** `assignedToUserId` is REQUIRED! Use your own user ID from the login response.

```json
{
  "title": "Implement user authentication",
  "description": "Add JWT-based authentication to the API",
  "priority": "High",
  "category": "Development",
  "assignedToUserId": "f51ffdaf-a91a-4f4a-abf1-8b58260e993b",
  "dueDate": "2026-05-01T12:00:00Z",
  "estimatedHours": 8.0,
  "tags": "authentication,security,backend"
}
```

**Note:** Get `assignedToUserId` from the login response (`user.id` field)

## Complete Task Update Example

```json
{
  "title": "Implement user authentication",
  "description": "Add JWT-based authentication to the API - Updated",
  "priority": "Medium",
  "category": "Development",
  "status": "InProgress",
  "assignedToUserId": "f51ffdaf-a91a-4f4a-abf1-8b58260e993b",
  "dueDate": "2026-05-02T12:00:00Z",
  "estimatedHours": 8.0,
  "actualHours": 3.5,
  "tags": "authentication,security,backend,jwt"
}
```

## Status Update Example

```json
{
  "status": "Completed"
}
```

## Common Validation Errors

### Error: Missing AssignedToUserId
```json
{
  "errors": {
    "AssignedToUserId": [
      "AssignedToUserId is required"
    ]
  }
}
```
**Solution:** Include `assignedToUserId` field with a valid user GUID (get from login response)

### Error: Invalid Priority Value
```json
{
  "errors": {
    "$.priority": [
      "The JSON value could not be converted to Velocify.Domain.Enums.TaskPriority"
    ]
  }
}
```
**Solution:** Use one of: `Critical`, `High`, `Medium`, `Low` (case-sensitive)

### Error: Invalid Status Value
```json
{
  "errors": {
    "$.status": [
      "The JSON value could not be converted to Velocify.Domain.Enums.TaskStatus"
    ]
  }
}
```
**Solution:** Use one of: `Pending`, `InProgress`, `Completed`, `Cancelled`, `Blocked` (case-sensitive)

### Error: Invalid Category Value
```json
{
  "errors": {
    "$.category": [
      "The JSON value could not be converted to Velocify.Domain.Enums.TaskCategory"
    ]
  }
}
```
**Solution:** Use one of the valid categories listed above (case-sensitive)

## Date Format

All dates should be in ISO 8601 format:

```
2026-05-01T12:00:00Z        // UTC time
2026-05-01T12:00:00+00:00   // UTC with offset
2026-05-01T08:00:00-04:00   // Eastern time
```

**Example:**
```json
{
  "dueDate": "2026-05-01T12:00:00Z"
}
```

## GUID Format

Task IDs and User IDs are GUIDs:

```
f51ffdaf-a91a-4f4a-abf1-8b58260e993b
```

**Example:**
```json
{
  "id": "f51ffdaf-a91a-4f4a-abf1-8b58260e993b"
}
```

## Tags Format

Tags are comma-separated strings:

```json
{
  "tags": "authentication,security,backend,jwt"
}
```

**Note:** No spaces after commas

## Quick Test Data

### Test User
```json
{
  "firstName": "Test",
  "lastName": "User",
  "email": "testuser@example.com",
  "password": "Test123!@#"
}
```

### Test Task
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

**Remember:** Replace `assignedToUserId` with your actual user ID from login response!

### Test Comment
```json
{
  "content": "This is a test comment to verify the Azure deployment is working correctly!"
}
```

## Response Status Codes

- `200 OK` - Successful GET, PUT, PATCH
- `201 Created` - Successful POST (resource created)
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Validation error or invalid data
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Concurrent update conflict
- `500 Internal Server Error` - Server error (check logs with correlation ID)
- `503 Service Unavailable` - Health check failed or service starting up

## Useful Tips

1. **Always include Authorization header** for protected endpoints:
   ```
   Authorization: Bearer YOUR_ACCESS_TOKEN
   ```

2. **Check correlation ID** in response headers for debugging:
   ```
   X-Correlation-ID: 8fd7cf3e-332e-44da-a011-5be18304296b
   ```

3. **Access token expires in 15 minutes** - use refresh token to get new one

4. **Enum values are case-sensitive** - use exact capitalization

5. **GUIDs must be valid format** - use actual IDs from responses

6. **Dates must be in ISO 8601 format** - include timezone

7. **Tags have no spaces after commas** - `tag1,tag2,tag3`
