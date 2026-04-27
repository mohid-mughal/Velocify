# API Endpoints Reference - Corrected Routes

## Dashboard Endpoints

| Test Description | ❌ Incorrect Route | ✅ Correct Route | Method | Auth |
|-----------------|-------------------|------------------|--------|------|
| Dashboard Summary | `/api/v1/dashboard/statistics` | `/api/v1/dashboard/summary` | GET | Required |
| Task Velocity | `/api/v1/dashboard/task-distribution` | `/api/v1/dashboard/velocity?days=30` | GET | Required |
| Workload Distribution | N/A | `/api/v1/dashboard/workload` | GET | Admin Only |
| Overdue Tasks | N/A | `/api/v1/dashboard/overdue` | GET | Required |

## Notification Endpoints

| Test Description | ❌ Incorrect Route | ✅ Correct Route | Method | Auth |
|-----------------|-------------------|------------------|--------|------|
| Get Notifications | ✅ `/api/v1/notifications` | `/api/v1/notifications?pageNumber=1&pageSize=10` | GET | Required |
| Mark as Read | `/api/v1/notifications/{notificationId}/read` | `/api/v1/notifications/{id}/read` | PATCH | Required |
| Mark All as Read | `/api/v1/notifications/mark-all-read` | `/api/v1/notifications/read-all` | PATCH | Required |

## Task Endpoints (All Working)

| Endpoint | Method | Auth | Notes |
|----------|--------|------|-------|
| `/api/v1/tasks` | GET | Required | Query params: status, priority, category, etc. |
| `/api/v1/tasks` | POST | Required | Requires assignedToUserId |
| `/api/v1/tasks/{id}` | GET | Required | Get task details |
| `/api/v1/tasks/{id}` | PUT | Required | Update task |
| `/api/v1/tasks/{id}` | DELETE | Required | ✅ Fixed: Now properly handles audit logs |
| `/api/v1/tasks/{id}/status` | PATCH | Required | Update status only |
| `/api/v1/tasks/{id}/comments` | GET | Required | Get task comments |
| `/api/v1/tasks/{id}/comments` | POST | Required | Add comment |
| `/api/v1/tasks/{id}/history` | GET | Required | Get audit log |

## User Endpoints

| Endpoint | Method | Auth | Notes |
|----------|--------|------|-------|
| `/api/v1/users/me` | GET | Required | Get current user profile |
| `/api/v1/users/me` | PUT | Required | ✅ Fixed: Now validates email uniqueness |

## Authentication Endpoints (All Working)

| Endpoint | Method | Auth | Notes |
|----------|--------|------|-------|
| `/api/v1/auth/register` | POST | None | Create new user |
| `/api/v1/auth/login` | POST | None | Get access token |
| `/api/v1/auth/refresh` | POST | None | Refresh access token |
| `/api/v1/auth/logout` | POST | Required | Invalidate tokens |

## Health Check

| Endpoint | Method | Auth | Notes |
|----------|--------|------|-------|
| `/health` | GET | None | Check API health, database, and services |

## Common Query Parameters

### Pagination
- `pageNumber` or `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)

### Task Filtering
- `status`: Pending, InProgress, Completed, Cancelled, Blocked
- `priority`: Critical, High, Medium, Low
- `category`: Development, Design, Marketing, Operations, Research, Other
- `assignedToUserId`: Filter by assigned user (GUID)
- `dueDateFrom`: Filter by due date start (ISO 8601)
- `dueDateTo`: Filter by due date end (ISO 8601)
- `searchTerm`: Search in title and description

### Notification Filtering
- `isRead`: true (read), false (unread), null (all)

## Response Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET request |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE or PATCH |
| 400 | Bad Request | Validation error or invalid data |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate resource (e.g., email already exists) |
| 500 | Internal Server Error | Unexpected server error |

## Error Response Format

All errors return ProblemDetails format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Email 'user@example.com' is already in use by another user.",
  "instance": "correlation-id-here"
}
```

## Authentication

All protected endpoints require a Bearer token:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Token expires in 15 minutes (900 seconds). Use the refresh token to get a new access token.

## CORS

The API supports CORS for the following origins:
- `http://localhost:3000` (Development)
- `https://velocify.azurewebsites.net` (Production)

## Rate Limiting

Currently no rate limiting is implemented. Consider adding rate limiting for production use.

## Versioning

API version is specified in the URL: `/api/v1/...`

The `api-supported-versions` header in responses shows supported versions.

## Quick Test Sequence

1. **Health Check**: `GET /health`
2. **Register**: `POST /api/v1/auth/register`
3. **Login**: `POST /api/v1/auth/login` (get access token)
4. **Create Task**: `POST /api/v1/tasks` (use token)
5. **Get Dashboard**: `GET /api/v1/dashboard/summary` (use token)
6. **Update Profile**: `PUT /api/v1/users/me` (use token)
7. **Delete Task**: `DELETE /api/v1/tasks/{id}` (use token)

## Notes

- All timestamps are in UTC (ISO 8601 format)
- All IDs are GUIDs (UUID v4)
- Enum values are case-sensitive strings
- Soft delete is used for tasks (IsDeleted flag)
- All changes are logged in audit tables
