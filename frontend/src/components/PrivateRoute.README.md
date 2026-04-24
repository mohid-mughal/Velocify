# PrivateRoute Component

## Overview

The `PrivateRoute` component provides authentication and role-based authorization for protected routes in the Velocify platform.

**Requirements:** 20.1, 20.2, 20.5

## Features

- ✅ Authentication check using `authStore.isAuthenticated`
- ✅ Redirect to `/login` for unauthenticated users
- ✅ Role-based authorization using `route.roles` configuration
- ✅ Display `AccessDenied` component for insufficient permissions
- ✅ Integration with React Router v6
- ✅ Uses helper functions from `routes.tsx` for authorization logic

## Usage

### Basic Usage with React Router

```tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { PrivateRoute } from './components';
import { routes } from './routes';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {routes.map((route) => {
          // Public routes (no roles specified)
          if (route.roles === undefined) {
            return (
              <Route
                key={route.path}
                path={route.path}
                element={route.element}
              />
            );
          }

          // Protected routes (requires authentication and/or specific roles)
          return (
            <Route
              key={route.path}
              path={route.path}
              element={
                <PrivateRoute route={route}>
                  {route.element}
                </PrivateRoute>
              }
            />
          );
        })}
      </Routes>
    </BrowserRouter>
  );
}
```

### Manual Usage

```tsx
import { PrivateRoute } from './components';
import { AppRoute } from './routes';
import DashboardPage from './pages/DashboardPage';

const dashboardRoute: AppRoute = {
  path: '/dashboard',
  element: <DashboardPage />,
  roles: [], // Requires authentication, any role
};

function App() {
  return (
    <PrivateRoute route={dashboardRoute}>
      <DashboardPage />
    </PrivateRoute>
  );
}
```

## Authorization Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    User navigates to route                   │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │  Is authenticated?   │
              └──────────┬───────────┘
                         │
           ┌─────────────┴─────────────┐
           │                           │
          NO                          YES
           │                           │
           ▼                           ▼
    ┌─────────────┐         ┌──────────────────┐
    │  Redirect   │         │  Has required    │
    │  to /login  │         │  role?           │
    └─────────────┘         └────────┬─────────┘
                                     │
                       ┌─────────────┴─────────────┐
                       │                           │
                      NO                          YES
                       │                           │
                       ▼                           ▼
                ┌─────────────┐           ┌──────────────┐
                │    Show     │           │    Render    │
                │ AccessDenied│           │   Protected  │
                │  Component  │           │   Content    │
                └─────────────┘           └──────────────┘
```

## Route Configuration

Routes are configured in `routes.tsx` with the following patterns:

### Public Routes
```tsx
{
  path: '/login',
  element: <LoginPage />,
  // No roles specified = public route
}
```

### Authenticated Routes (Any Role)
```tsx
{
  path: '/dashboard',
  element: <DashboardPage />,
  roles: [], // Empty array = requires authentication, any role
}
```

### Role-Restricted Routes
```tsx
{
  path: '/admin',
  element: <AdminPanelPage />,
  roles: ['Admin', 'SuperAdmin'], // Requires one of these roles
}
```

## Helper Functions

The component uses two helper functions from `routes.tsx`:

### `canAccessRoute(route, userRole)`

Checks if a user can access a route based on their role.

```tsx
// Returns true if:
// - Route has no roles (public)
// - Route has empty roles array and user is authenticated
// - User's role is in the route's roles array

canAccessRoute(route, 'Admin') // true if route allows Admin
canAccessRoute(route, null)    // false if route requires authentication
```

### `getUnauthorizedRedirect(isAuthenticated)`

Returns the appropriate redirect path for unauthorized access.

```tsx
getUnauthorizedRedirect(false) // '/login' - not authenticated
getUnauthorizedRedirect(true)  // '/dashboard' - authenticated but wrong role
```

## AccessDenied Component

The `AccessDenied` component is displayed when a user lacks the required role:

- Shows clear "Access Denied" message
- Displays user's current role
- Provides "Go to Dashboard" button
- Provides "Logout" button

## Integration with Auth Store

The component reads authentication state from Zustand auth store:

```tsx
const { isAuthenticated, role } = useAuthStore();

// isAuthenticated: boolean - whether user has valid session
// role: 'SuperAdmin' | 'Admin' | 'Member' | null - user's role
```

## Security Considerations

1. **Client-side only**: This component provides UI-level protection. Backend API must enforce authorization.
2. **Token validation**: Backend validates JWT tokens on every request.
3. **Role claims**: User role is stored in JWT claims and verified server-side.
4. **No token storage**: Access tokens are stored in memory only (not localStorage).

## Examples

### Protecting Dashboard (Any Authenticated User)

```tsx
<PrivateRoute route={{ path: '/dashboard', element: <Dashboard />, roles: [] }}>
  <Dashboard />
</PrivateRoute>
```

### Protecting Admin Panel (Admin or SuperAdmin Only)

```tsx
<PrivateRoute route={{ 
  path: '/admin', 
  element: <AdminPanel />, 
  roles: ['Admin', 'SuperAdmin'] 
}}>
  <AdminPanel />
</PrivateRoute>
```

### Protecting User Profile (Any Authenticated User)

```tsx
<PrivateRoute route={{ path: '/profile', element: <Profile />, roles: [] }}>
  <Profile />
</PrivateRoute>
```

## Testing

To test the PrivateRoute component:

1. **Unauthenticated access**: Should redirect to `/login`
2. **Insufficient permissions**: Should show `AccessDenied`
3. **Authorized access**: Should render protected content
4. **Public routes**: Should render without authentication
5. **Role verification**: Should check user role against route.roles

## Related Files

- `frontend/src/routes.tsx` - Route configuration and helper functions
- `frontend/src/store/authStore.ts` - Authentication state management
- `frontend/src/components/AccessDenied.tsx` - Access denied UI component
