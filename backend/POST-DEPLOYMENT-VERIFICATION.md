# Post-Deployment Verification Checklist

Use this checklist after deploying the critical fixes to Azure.

## Pre-Deployment

- [ ] All code changes committed to Git
- [ ] Local build successful (`dotnet build`)
- [ ] Local tests passing (`dotnet test`)
- [ ] No compilation errors or warnings (except known AutoMapper vulnerability)

## Deployment

- [ ] Code pushed to main branch (triggers GitHub Actions)
- [ ] GitHub Actions workflow completed successfully
- [ ] Azure App Service shows "Running" status
- [ ] No errors in Azure Log Stream during startup

## Verification Tests

### 1. Health Check
```http
GET https://velocify.azurewebsites.net/health
```
- [ ] Returns 200 OK
- [ ] Database status: Healthy
- [ ] LangChain/OpenAI status: Healthy (or warning if not configured)
- [ ] Disk space: Healthy

### 2. Authentication Flow
```http
POST https://velocify.azurewebsites.net/api/v1/auth/login
Content-Type: application/json

{
  "email": "testuser@example.com",
  "password": "Test123!@#"
}
```
- [ ] Returns 200 OK
- [ ] Receives accessToken and refreshToken
- [ ] Token is valid (can be used in subsequent requests)

### 3. Delete Task (Critical Fix #1)
```http
DELETE https://velocify.azurewebsites.net/api/v1/tasks/{taskId}
Authorization: Bearer {accessToken}
```
**Expected Before Fix**: 500 Internal Server Error  
**Expected After Fix**: 204 No Content

- [ ] Returns 204 No Content (not 500)
- [ ] Task is soft-deleted (IsDeleted = true)
- [ ] Audit log created with correct ChangedByUserId
- [ ] No FK constraint violation in logs

### 4. Update User Profile with Duplicate Email (Critical Fix #2)
```http
PUT https://velocify.azurewebsites.net/api/v1/users/me
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "User",
  "email": "existing@example.com"
}
```
**Expected Before Fix**: 500 Internal Server Error  
**Expected After Fix**: 409 Conflict

- [ ] Returns 409 Conflict (not 500)
- [ ] Error message: "Email 'existing@example.com' is already in use by another user."
- [ ] No database exception in logs

### 5. Dashboard Summary (Critical Fix #3)
```http
GET https://velocify.azurewebsites.net/api/v1/dashboard/summary
Authorization: Bearer {accessToken}
```
**Expected Before Fix**: 404 Not Found  
**Expected After Fix**: 200 OK

- [ ] Returns 200 OK (not 404)
- [ ] Response contains task counts by status
- [ ] Data is accurate

### 6. Dashboard Velocity (Critical Fix #3)
```http
GET https://velocify.azurewebsites.net/api/v1/dashboard/velocity?days=30
Authorization: Bearer {accessToken}
```
**Expected Before Fix**: 404 Not Found  
**Expected After Fix**: 200 OK

- [ ] Returns 200 OK (not 404)
- [ ] Response contains velocity data points
- [ ] Data covers requested time period

### 7. Mark Notification as Read (Critical Fix #4)
```http
PATCH https://velocify.azurewebsites.net/api/v1/notifications/{notificationId}/read
Authorization: Bearer {accessToken}
```
**Expected Before Fix**: 400 Bad Request  
**Expected After Fix**: 204 No Content

- [ ] Returns 204 No Content (not 400)
- [ ] Notification marked as read in database
- [ ] No validation error

### 8. Mark All Notifications as Read (Critical Fix #5)
```http
PATCH https://velocify.azurewebsites.net/api/v1/notifications/read-all
Authorization: Bearer {accessToken}
```
**Expected Before Fix**: 404 Not Found  
**Expected After Fix**: 204 No Content

- [ ] Returns 204 No Content (not 404)
- [ ] All user notifications marked as read
- [ ] Correct endpoint and HTTP method

## Regression Testing

Verify that existing functionality still works:

### Task Management
- [ ] Create task: `POST /api/v1/tasks` returns 201 Created
- [ ] Get tasks: `GET /api/v1/tasks` returns 200 OK
- [ ] Update task: `PUT /api/v1/tasks/{id}` returns 200 OK
- [ ] Add comment: `POST /api/v1/tasks/{id}/comments` returns 201 Created
- [ ] Get history: `GET /api/v1/tasks/{id}/history` returns 200 OK

### User Management
- [ ] Get profile: `GET /api/v1/users/me` returns 200 OK
- [ ] Update profile (unique email): `PUT /api/v1/users/me` returns 200 OK

### Authentication
- [ ] Register: `POST /api/v1/auth/register` returns 201 Created
- [ ] Login: `POST /api/v1/auth/login` returns 200 OK
- [ ] Refresh: `POST /api/v1/auth/refresh` returns 200 OK
- [ ] Logout: `POST /api/v1/auth/logout` returns 204 No Content

## Monitoring

### Azure Application Insights
- [ ] No new exceptions in last 15 minutes
- [ ] Response times within acceptable range (<500ms for most endpoints)
- [ ] No failed requests (except expected 4xx errors)

### Azure Log Stream
- [ ] No error logs during verification tests
- [ ] Correlation IDs present in all logs
- [ ] Audit logs being created correctly

### Database
- [ ] TaskAuditLogs table has entries with valid ChangedByUserId
- [ ] No orphaned records
- [ ] Soft-deleted tasks have IsDeleted = true

## Performance

- [ ] Average response time: < 500ms
- [ ] P95 response time: < 1000ms
- [ ] No memory leaks (check App Service metrics)
- [ ] CPU usage: < 70%

## Error Handling

Test error scenarios to ensure proper responses:

- [ ] 401 Unauthorized: Request without token
- [ ] 403 Forbidden: Member accessing admin endpoint
- [ ] 404 Not Found: Non-existent resource
- [ ] 400 Bad Request: Invalid data
- [ ] 409 Conflict: Duplicate email

## Documentation

- [ ] API-CRITICAL-FIXES.md updated
- [ ] CRITICAL-FIXES-SUMMARY.md created
- [ ] API-ENDPOINTS-REFERENCE.md updated
- [ ] test-azure-deployment.http updated with correct endpoints

## Rollback Criteria

If any of these occur, consider rollback:
- [ ] More than 5% of requests return 500 errors
- [ ] Critical functionality broken (auth, task creation)
- [ ] Database corruption or data loss
- [ ] Performance degradation > 50%

## Sign-Off

- [ ] All critical fixes verified
- [ ] No regressions detected
- [ ] Monitoring shows healthy metrics
- [ ] Documentation updated
- [ ] Team notified of deployment

**Deployed By**: _________________  
**Date**: _________________  
**Time**: _________________  
**Commit Hash**: _________________  

## Notes

Use this space to document any issues encountered during verification:

```
[Add notes here]
```

## Next Steps

After successful verification:
1. Update project documentation with new endpoint information
2. Notify frontend team of corrected endpoint routes
3. Schedule follow-up to address AutoMapper vulnerability warning
4. Consider adding integration tests for these scenarios
5. Review and update API documentation (Swagger/OpenAPI)
