import { ReactElement } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { AppRoute, canAccessRoute } from '../routes';
import AccessDenied from './AccessDenied';

/**
 * PrivateRoute Component
 * 
 * Requirements: 20.1, 20.2, 20.5
 * 
 * Protects routes based on authentication and role-based authorization.
 * 
 * Flow:
 * 1. Check if user is authenticated (authStore.isAuthenticated)
 * 2. If not authenticated, redirect to /login
 * 3. Check if user has required role for the route (route.roles)
 * 4. If insufficient permissions, show AccessDenied component
 * 5. If authorized, render the route element
 * 
 * Usage:
 * Wrap protected route elements with this component in the router configuration.
 * The component uses the route configuration from routes.tsx to determine
 * required roles and authorization logic.
 */

interface PrivateRouteProps {
  /**
   * The route configuration containing role requirements
   */
  route: AppRoute;
  
  /**
   * The element to render if user is authorized
   */
  children: ReactElement;
}

export default function PrivateRoute({ route, children }: PrivateRouteProps): ReactElement {
  const { isAuthenticated, role } = useAuthStore();

  // Check authentication status
  if (!isAuthenticated) {
    // Redirect to login if not authenticated
    return <Navigate to="/login" replace />;
  }

  // Check role-based authorization
  const hasAccess = canAccessRoute(route, role);

  if (!hasAccess) {
    // Show access denied page if user lacks required role
    return <AccessDenied />;
  }

  // User is authenticated and authorized - render the protected content
  return children;
}
