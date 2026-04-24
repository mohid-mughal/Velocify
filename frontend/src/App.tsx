/**
 * App Component - Main Application Entry Point
 * 
 * Task: 28.3
 * Requirements: All frontend requirements
 * 
 * Sets up the main application structure with:
 * - React Router for navigation
 * - Error boundary for graceful error handling
 * - Suspense for lazy-loaded routes
 * - Toast notifications
 * - SignalR real-time connection
 */

import { Suspense, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { routes } from './routes';
import { PrivateRoute } from './components/PrivateRoute';
import { useAuth } from './hooks/useAuth';
import { useSignalR } from './hooks/useSignalR';
import { ErrorBoundary } from './components/ErrorBoundary';
import { LoadingSpinner } from './components/ui/LoadingSpinner';

/**
 * Loading fallback component for lazy-loaded routes
 * Requirement 20.3: Show loading state while route components load
 */
function RouteLoadingFallback() {
  return (
    <div className="flex items-center justify-center min-h-screen bg-neutral-50">
      <div className="text-center">
        <LoadingSpinner size="lg" />
        <p className="mt-4 text-neutral-600">Loading...</p>
      </div>
    </div>
  );
}

/**
 * Main App Component
 * 
 * Responsibilities:
 * - Setup React Router with all application routes
 * - Wrap protected routes with PrivateRoute component
 * - Establish SignalR connection for authenticated users
 * - Provide global error boundary
 * - Setup toast notification system
 * 
 * Requirements:
 * - 20.1: Route configuration and navigation
 * - 20.2: Role-based access control
 * - 20.3: Lazy loading for code splitting
 * - 21.4: Global error handling
 * - 25.6: Real-time SignalR connection
 * - 28.5: Toast notifications
 */
function App() {
  const { isAuthenticated } = useAuth();

  // Establish SignalR connection for authenticated users
  // Requirement 6.7, 25.6: Real-time notifications via SignalR
  useSignalR();

  return (
    <ErrorBoundary>
      <BrowserRouter>
        <Suspense fallback={<RouteLoadingFallback />}>
          <Routes>
            {routes.map((route, index) => {
              // Public routes (no roles specified)
              if (route.roles === undefined) {
                return (
                  <Route
                    key={index}
                    path={route.path}
                    element={route.element}
                  />
                );
              }

              // Protected routes (requires authentication and optional role check)
              return (
                <Route
                  key={index}
                  path={route.path}
                  element={
                    <PrivateRoute requiredRoles={route.roles}>
                      {route.element}
                    </PrivateRoute>
                  }
                />
              );
            })}
          </Routes>
        </Suspense>

        {/* Global toast notification container */}
        {/* Requirement 28.5: Toast notifications for user feedback */}
        <Toaster
          position="top-right"
          toastOptions={{
            // Default options
            duration: 5000,
            style: {
              background: '#fff',
              color: '#171717',
              boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)',
              borderRadius: '0.5rem',
              padding: '1rem',
            },
            // Success toast
            success: {
              duration: 5000,
              iconTheme: {
                primary: '#22c55e',
                secondary: '#fff',
              },
            },
            // Error toast
            error: {
              duration: 7000,
              iconTheme: {
                primary: '#ef4444',
                secondary: '#fff',
              },
            },
            // Loading toast
            loading: {
              iconTheme: {
                primary: '#0ea5e9',
                secondary: '#fff',
              },
            },
          }}
        />
      </BrowserRouter>
    </ErrorBoundary>
  );
}

export default App;
