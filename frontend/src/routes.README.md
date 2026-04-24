# Route Configuration

This document describes the route configuration for the Velocify platform frontend.

## Overview

The route configuration is defined in `routes.tsx` and provides:
- Lazy-loaded page components for optimal bundle size
- Role-based access control (SuperAdmin, Admin, Member)
- Type-safe route definitions with TypeScript
- Helper functions for authorization checks

## Route Types

### Public Routes
Routes that don't require authentication:
- `/login` - User login page
- `/register` - User registration page
- `*` (404) - Not found page

### Protected Routes
Routes that require authentication but no specific role:
- `/` - Dashboard (home page)
- `/dashboard` - Dashboard (explicit path)
- `/tasks` - Task list page
- `/tasks/new` - Create new task
- `/tasks/:id` - View task details
- `/tasks/:id/edit` - Edit task
- `/profile` - User profile and productivity metrics
- `/notifications` - Notification management

### Role-Restricted Routes
Routes that require specific roles:
- `/admin` - Admin panel (requires Admin or SuperAdmin role)

## Lazy Loading

All page components are lazy-loaded using `React.lazy()` to reduce the initial bundle size. This means:
- Each page is in a separate chunk
- Pages are loaded on-demand when navigated to
- Initial load time is faster
- Code splitting is automatic

### Usage with Suspense

Wrap your router with a Suspense boundary to show a loading state while lazy components load:

```tsx
import { Suspense } from 'react';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import { routes } from './routes';

const router = createBrowserRouter(routes);

function App() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <RouterProvider router={router} />
    </Suspense>
  );
}
```

## Role-Based Access Control

### Role Types
```typescript
type UserRole = 'SuperAdmin' | 'Admin' | 'Member';
```

### Route Configuration
Each route can specify required roles:
- `roles: undefined` - Public route (no authentication required)
- `roles: []` - Protected route (authentication required, any role)
- `roles: ['Admin', 'SuperAdmin']` - Role-restricted route (specific roles required)

### Authorization Helpers

#### `canAccessRoute(route, userRole)`
Checks if a user can access a specific route:
```typescript
const route = routes.find(r => r.path === '/admin');
const canAccess = canAccessRoute(route, 'Admin'); // true
const canAccess = canAccessRoute(route, 'Member'); // false
```

#### `getUnauthorizedRedirect(isAuthenticated)`
Gets the redirect path for unauthorized access:
```typescript
const redirect = getUnauthorizedRedirect(false); // '/login'
const redirect = getUnauthorizedRedirect(true); // '/dashboard'
```

## Implementation with PrivateRoute

The `PrivateRoute` component (to be implemented in task 17.6) will use these helpers to:
1. Check if user is authenticated
2. Check if user has required role
3. Redirect to appropriate page if unauthorized

Example usage:
```tsx
import { Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import { canAccessRoute, getUnauthorizedRedirect } from './routes';

function PrivateRoute({ route, children }) {
  const { isAuthenticated, role } = useAuthStore();
  
  if (!canAccessRoute(route, role)) {
    return <Navigate to={getUnauthorizedRedirect(isAuthenticated)} replace />;
  }
  
  return children;
}
```

## Adding New Routes

To add a new route:

1. Create the page component in `src/pages/`
2. Add lazy import in `routes.tsx`:
   ```typescript
   const MyNewPage = lazy(() => import('./pages/MyNewPage'));
   ```
3. Add route configuration:
   ```typescript
   {
     path: '/my-new-page',
     element: <MyNewPage />,
     roles: [], // or ['Admin', 'SuperAdmin'] for role-restricted
   }
   ```

## Route Parameters

Routes can include parameters using React Router syntax:
- `:id` - Required parameter (e.g., `/tasks/:id`)
- `:id?` - Optional parameter (e.g., `/tasks/:id?`)

Access parameters in components using `useParams()`:
```typescript
import { useParams } from 'react-router-dom';

function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  // Use id to fetch task data
}
```

## Navigation

Use React Router hooks for navigation:

### `useNavigate()`
```typescript
import { useNavigate } from 'react-router-dom';

function MyComponent() {
  const navigate = useNavigate();
  
  const handleClick = () => {
    navigate('/tasks');
  };
}
```

### `Link` Component
```typescript
import { Link } from 'react-router-dom';

function MyComponent() {
  return <Link to="/tasks">View Tasks</Link>;
}
```

## Requirements Mapping

This route configuration satisfies the following requirements:

- **20.1**: Protected routes redirect to login when not authenticated
- **20.2**: Role-restricted routes show access denied for insufficient permissions
- **20.3**: Lazy loading reduces initial bundle size
- **20.4**: React Router v6 maintains route state on page refresh
- **20.5**: PrivateRoute component (to be implemented) checks auth store

## Future Enhancements

Potential improvements for future iterations:
- Nested routes for complex layouts
- Route-level data loading with loaders
- Route-level error boundaries
- Breadcrumb generation from route hierarchy
- Route-based code splitting optimization
