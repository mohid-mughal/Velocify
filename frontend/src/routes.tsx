import { lazy } from 'react';

/**
 * Route Configuration for Velocify Platform
 * 
 * Requirements: 20.1-20.5
 * - Defines routes array with paths, elements, and required roles
 * - Configures lazy loading for all page components to reduce initial bundle size
 * - Supports role-based access control (SuperAdmin, Admin, Member)
 * 
 * Route Structure:
 * - Public routes: /login, /register
 * - Protected routes: All other routes require authentication
 * - Role-restricted routes: /admin/* requires Admin or SuperAdmin role
 * 
 * Lazy Loading:
 * All page components are lazy-loaded using React.lazy() to split the bundle
 * and improve initial load performance. Components are loaded on-demand when
 * the user navigates to the route.
 */

// Lazy-loaded page components
const LoginPage = lazy(() => import('./pages/LoginPage'));
const RegisterPage = lazy(() => import('./pages/RegisterPage'));
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const TaskListPage = lazy(() => import('./pages/TaskListPage'));
const TaskDetailPage = lazy(() => import('./pages/TaskDetailPage'));
const TaskFormPage = lazy(() => import('./pages/TaskFormPage'));
const UserProfilePage = lazy(() => import('./pages/UserProfilePage'));
const AdminPage = lazy(() => import('./pages/AdminPage'));
const NotificationsPage = lazy(() => import('./pages/NotificationsPage'));
const NotFoundPage = lazy(() => import('./pages/NotFoundPage'));

/**
 * User roles for role-based access control
 */
export type UserRole = 'SuperAdmin' | 'Admin' | 'Member';

/**
 * Extended route configuration with role-based access control
 */
export interface AppRoute {
  /**
   * Route path
   */
  path?: string;
  
  /**
   * Route element to render
   */
  element?: React.ReactNode;
  
  /**
   * Index route flag
   */
  index?: boolean;
  
  /**
   * Required roles to access this route
   * If undefined, route is public (no authentication required)
   * If empty array, route requires authentication but no specific role
   * If contains roles, user must have one of the specified roles
   */
  roles?: UserRole[];
  
  /**
   * Child routes with role-based access control
   */
  children?: AppRoute[];
}

/**
 * Application routes configuration
 * 
 * Route Patterns:
 * - Public routes: No roles specified (login, register)
 * - Protected routes: Empty roles array (requires authentication)
 * - Role-restricted routes: Specific roles array (admin panel)
 * 
 * Lazy Loading:
 * All page components are lazy-loaded to reduce initial bundle size.
 * React Router will automatically handle code splitting and loading states.
 * Wrap routes with Suspense boundary in App.tsx to show loading fallback.
 */
export const routes: AppRoute[] = [
  // Public routes - no authentication required
  {
    path: '/login',
    element: <LoginPage />,
    // No roles specified = public route
  },
  {
    path: '/register',
    element: <RegisterPage />,
    // No roles specified = public route
  },

  // Protected routes - authentication required
  {
    path: '/',
    element: <DashboardPage />,
    roles: [], // Empty array = requires authentication, any role
  },
  {
    path: '/dashboard',
    element: <DashboardPage />,
    roles: [], // Empty array = requires authentication, any role
  },
  {
    path: '/tasks',
    element: <TaskListPage />,
    roles: [], // Empty array = requires authentication, any role
  },
  {
    path: '/tasks/new',
    element: <TaskFormPage />,
    roles: [], // Empty array = requires authentication, any role
  },
  {
    path: '/tasks/:id',
    element: <TaskDetailPage />,
    roles: [], // Empty array = requires authentication, any role
  },
  {
    path: '/tasks/:id/edit',
    element: <TaskFormPage />,
    roles: [], // Empty array = requires authentication, any role
  },
  {
    path: '/profile',
    element: <UserProfilePage />,
    roles: [], // Empty array = requires authentication, any role
  },
  {
    path: '/notifications',
    element: <NotificationsPage />,
    roles: [], // Empty array = requires authentication, any role
  },

  // Admin-only routes - requires Admin or SuperAdmin role
  {
    path: '/admin',
    element: <AdminPage />,
    roles: ['Admin', 'SuperAdmin'], // Specific roles = requires one of these roles
  },

  // 404 Not Found - catch all route
  {
    path: '*',
    element: <NotFoundPage />,
    // No roles specified = public route
  },
];

/**
 * Helper function to check if user has required role for a route
 * 
 * @param route - The route to check
 * @param userRole - The user's current role (or null if not authenticated)
 * @returns true if user can access the route, false otherwise
 */
export function canAccessRoute(route: AppRoute, userRole: UserRole | null): boolean {
  // If route has no roles specified, it's a public route
  if (route.roles === undefined) {
    return true;
  }

  // If route requires authentication but user is not authenticated
  if (userRole === null) {
    return false;
  }

  // If route requires authentication but no specific role (empty array)
  if (route.roles.length === 0) {
    return true;
  }

  // Check if user has one of the required roles
  return route.roles.includes(userRole);
}

/**
 * Helper function to get redirect path for unauthorized access
 * 
 * @param isAuthenticated - Whether user is authenticated
 * @returns Redirect path (/login for unauthenticated, /dashboard for unauthorized)
 */
export function getUnauthorizedRedirect(isAuthenticated: boolean): string {
  return isAuthenticated ? '/dashboard' : '/login';
}
