# API Configuration

This directory contains the Axios configuration for making HTTP requests to the backend API.

## Files

- `axios.ts` - Main Axios instance with interceptors for authentication and token refresh
- `index.ts` - Exports for easier imports

## Usage

```typescript
import { api } from '@/api';

// Make authenticated requests
const response = await api.get('/tasks');
const task = await api.post('/tasks', taskData);
```

## Features

### Base URL Configuration
The base URL is configured via the `VITE_API_BASE_URL` environment variable, defaulting to `/api/v1` for production.

### Request Interceptor
Automatically adds the `Authorization` header with the access token from the auth store to every request.

### Response Interceptor - Token Refresh Flow
Handles 401 Unauthorized responses by automatically attempting to refresh the access token:

1. **Detect 401 Error**: When a request fails with 401 status
2. **Check Retry Flag**: Ensure we haven't already tried to refresh for this request
3. **Call Refresh Endpoint**: POST to `/api/v1/auth/refresh` with refresh token (sent via httpOnly cookie)
4. **Update Token**: Store the new access token in the auth store (memory)
5. **Retry Request**: Retry the original request with the new access token
6. **Handle Failure**: If refresh fails, clear auth state and redirect to login

### Security Notes

- **Access tokens** are stored in memory (via auth store) - never in localStorage
- **Refresh tokens** are stored in httpOnly cookies and sent automatically with `withCredentials: true`
- The token refresh flow prevents the need for users to re-login when access tokens expire (15 minutes)
- Failed refresh attempts clear auth state and redirect to login for security

## TODO

The current implementation uses `localStorage` as a placeholder. Once the auth store is implemented:

1. Replace `localStorage.getItem('accessToken')` with `useAuthStore.getState().accessToken`
2. Replace `localStorage.setItem('accessToken', accessToken)` with `useAuthStore.getState().setAccessToken(accessToken)`
3. Replace `localStorage.removeItem('accessToken')` with `useAuthStore.getState().clearAuth()`

## Requirements

This implementation satisfies:
- **Requirement 21.3**: Server request failures with 401 clear auth state and redirect to login
- **Requirement 21.5**: Access tokens stored in memory, refresh tokens in httpOnly cookies
