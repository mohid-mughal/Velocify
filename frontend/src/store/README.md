# Zustand Store Documentation

## Auth Store

The auth store manages authentication state including user information, access tokens, and authentication status.

### Features

- **Memory-only token storage**: Access tokens are stored in memory only (never persisted)
- **SessionStorage persistence**: User information persists across page refreshes
- **Security compliant**: Follows requirements 21.1, 21.5, and 21.6
- **Integrated with Axios**: Automatically provides tokens to API requests

### State

```typescript
{
  user: User | null;              // Current authenticated user
  accessToken: string | null;     // JWT access token (memory only)
  role: 'SuperAdmin' | 'Admin' | 'Member' | null;  // User role
  isAuthenticated: boolean;       // Authentication status
}
```

### Actions

#### `login(accessToken: string, user: User)`
Store access token and user information after successful login.

```typescript
import { useAuthStore } from '@/store/authStore';

// In your login handler
const handleLogin = async (email: string, password: string) => {
  const response = await axios.post('/auth/login', { email, password });
  const { accessToken, user } = response.data;
  
  useAuthStore.getState().login(accessToken, user);
};
```

#### `logout()`
Clear all authentication state.

```typescript
import { useAuthStore } from '@/store/authStore';

const handleLogout = () => {
  useAuthStore.getState().logout();
  // Optionally call backend logout endpoint
  await axios.post('/auth/logout');
};
```

#### `setUser(user: User)`
Update user information (e.g., after profile update).

```typescript
import { useAuthStore } from '@/store/authStore';

const handleProfileUpdate = async (updates: Partial<User>) => {
  const response = await axios.put('/users/me', updates);
  useAuthStore.getState().setUser(response.data);
};
```

#### `setToken(accessToken: string)`
Update access token (e.g., after token refresh). This is automatically called by the axios interceptor.

```typescript
// Usually called automatically by axios interceptor
// Manual usage:
useAuthStore.getState().setToken(newAccessToken);
```

### Selector Hooks

Convenient hooks for accessing specific parts of the auth state:

```typescript
import { useUser, useIsAuthenticated, useUserRole, useAccessToken } from '@/store/authStore';

function MyComponent() {
  const user = useUser();
  const isAuthenticated = useIsAuthenticated();
  const role = useUserRole();
  const accessToken = useAccessToken();
  
  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }
  
  return <div>Welcome, {user?.firstName}!</div>;
}
```

### Usage in Components

#### Accessing state in React components

```typescript
import { useAuthStore } from '@/store/authStore';

function ProfilePage() {
  const { user, isAuthenticated } = useAuthStore();
  
  if (!isAuthenticated || !user) {
    return <Navigate to="/login" />;
  }
  
  return (
    <div>
      <h1>{user.firstName} {user.lastName}</h1>
      <p>Email: {user.email}</p>
      <p>Role: {user.role}</p>
      <p>Productivity Score: {user.productivityScore}</p>
    </div>
  );
}
```

#### Role-based rendering

```typescript
import { useUserRole } from '@/store/authStore';

function AdminPanel() {
  const role = useUserRole();
  
  if (role !== 'Admin' && role !== 'SuperAdmin') {
    return <div>Access Denied</div>;
  }
  
  return <div>Admin content here</div>;
}
```

#### Accessing state outside React components

```typescript
import { useAuthStore } from '@/store/authStore';

// In utility functions, API calls, etc.
function getAuthHeaders() {
  const accessToken = useAuthStore.getState().accessToken;
  return {
    Authorization: `Bearer ${accessToken}`
  };
}
```

### Security Notes

1. **Access tokens are never persisted**: They exist only in memory and are lost on page refresh
2. **User info is persisted**: Stored in sessionStorage for convenience across page refreshes
3. **Token refresh flow**: On page refresh, the user info is restored but `isAuthenticated` is false until a token refresh occurs
4. **Automatic token refresh**: The axios interceptor automatically refreshes tokens on 401 responses
5. **Logout on refresh failure**: If token refresh fails, the user is automatically logged out and redirected to login

### Integration with Axios

The auth store is integrated with the axios instance in `frontend/src/api/axios.ts`:

- **Request interceptor**: Automatically adds `Authorization: Bearer <token>` header
- **Response interceptor**: Handles 401 errors by attempting token refresh
- **Automatic logout**: Clears auth state and redirects to login if refresh fails

No manual token management is needed in your API calls!
