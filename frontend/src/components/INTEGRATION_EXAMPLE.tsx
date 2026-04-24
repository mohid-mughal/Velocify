/**
 * Integration Example: How to use PrivateRoute with React Router
 * 
 * This file demonstrates how to integrate the PrivateRoute component
 * with React Router v6 and the routes configuration.
 * 
 * Requirements: 20.1, 20.2, 20.5
 */

import { Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import PrivateRoute from './PrivateRoute';
import { routes } from '../routes';
import { queryClient } from '../api/queryClient';

/**
 * Example App Component with PrivateRoute Integration
 * 
 * This example shows:
 * 1. How to wrap the app with necessary providers
 * 2. How to iterate over routes and apply PrivateRoute
 * 3. How to handle lazy-loaded components with Suspense
 * 4. How to distinguish between public and protected routes
 */
function AppExample() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Suspense fallback={<LoadingFallback />}>
          <Routes>
            {routes.map((route: any) => {
              // Public routes: no roles specified
              // These routes are accessible without authentication
              if (route.roles === undefined) {
                return (
                  <Route
                    key={route.path}
                    path={route.path}
                    element={route.element}
                  />
                );
              }

              // Protected routes: roles array specified (empty or with specific roles)
              // These routes require authentication and optionally specific roles
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
        </Suspense>
      </BrowserRouter>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

/**
 * Loading Fallback Component
 * 
 * Displayed while lazy-loaded route components are being loaded.
 * This provides a better user experience during code splitting.
 */
function LoadingFallback() {
  return (
    <div style={styles.loadingContainer}>
      <div style={styles.spinner}></div>
      <p style={styles.loadingText}>Loading...</p>
    </div>
  );
}

const styles: { [key: string]: React.CSSProperties } = {
  loadingContainer: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: '100vh',
    backgroundColor: '#f9fafb',
  },
  spinner: {
    width: '3rem',
    height: '3rem',
    border: '4px solid #e5e7eb',
    borderTop: '4px solid #3b82f6',
    borderRadius: '50%',
    animation: 'spin 1s linear infinite',
  },
  loadingText: {
    marginTop: '1rem',
    fontSize: '1rem',
    color: '#6b7280',
  },
};

export default AppExample;

/**
 * Alternative: Manual Route Configuration
 * 
 * If you prefer explicit route configuration instead of mapping,
 * you can define routes manually:
 */
export function AppExampleManual() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Suspense fallback={<LoadingFallback />}>
          <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Protected Routes - Any Authenticated User */}
            <Route
              path="/"
              element={
                <PrivateRoute route={{ path: '/', element: <DashboardPage />, roles: [] }}>
                  <DashboardPage />
                </PrivateRoute>
              }
            />
            <Route
              path="/dashboard"
              element={
                <PrivateRoute route={{ path: '/dashboard', element: <DashboardPage />, roles: [] }}>
                  <DashboardPage />
                </PrivateRoute>
              }
            />
            <Route
              path="/tasks"
              element={
                <PrivateRoute route={{ path: '/tasks', element: <TaskListPage />, roles: [] }}>
                  <TaskListPage />
                </PrivateRoute>
              }
            />

            {/* Protected Routes - Admin/SuperAdmin Only */}
            <Route
              path="/admin"
              element={
                <PrivateRoute
                  route={{
                    path: '/admin',
                    element: <AdminPanelPage />,
                    roles: ['Admin', 'SuperAdmin'],
                  }}
                >
                  <AdminPanelPage />
                </PrivateRoute>
              }
            />

            {/* 404 Not Found */}
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </Suspense>
      </BrowserRouter>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

// Placeholder imports (these would be actual page components)
const LoginPage = () => <div>Login Page</div>;
const RegisterPage = () => <div>Register Page</div>;
const DashboardPage = () => <div>Dashboard Page</div>;
const TaskListPage = () => <div>Task List Page</div>;
const AdminPanelPage = () => <div>Admin Panel Page</div>;
const NotFoundPage = () => <div>404 Not Found</div>;

/**
 * Usage Notes:
 * 
 * 1. Replace AppExample with your actual App component in main.tsx
 * 2. Ensure all page components are lazy-loaded for code splitting
 * 3. The Suspense boundary handles loading states for lazy components
 * 4. PrivateRoute automatically handles authentication and authorization
 * 5. Public routes (login, register) don't need PrivateRoute wrapper
 * 
 * Route Types:
 * - Public: No roles specified (undefined)
 * - Authenticated: Empty roles array ([])
 * - Role-restricted: Specific roles array (['Admin', 'SuperAdmin'])
 */
