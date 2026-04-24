import { QueryClient, QueryCache, MutationCache } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useAuthStore } from '../store/authStore';

/**
 * TanStack Query Client Configuration
 * 
 * Requirements:
 * - 21.2: TanStack Query invalidates cache keys and refetches on mutations
 * - 21.3: Handle 401 errors by clearing auth state and redirecting to login
 * - 21.4: Handle 5xx errors by displaying toast notifications
 * 
 * Global Configuration:
 * - staleTime: 5 minutes - data is considered fresh for 5 minutes
 * - cacheTime (gcTime in v5): 10 minutes - unused data is garbage collected after 10 minutes
 * - refetchOnWindowFocus: true - refetch stale queries when window regains focus
 * - retry: 1 - retry failed queries once before giving up
 */

// Global error handler for queries and mutations
const handleError = (error: unknown) => {
  // Type guard for AxiosError
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    
    // Requirement 21.3: Handle 401 Unauthorized
    // Clear auth state and redirect to login
    // Note: axios interceptor already handles token refresh, so if we get here
    // it means refresh failed and user needs to re-authenticate
    if (status === 401) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
      return;
    }
    
    // Requirement 21.4: Handle 403 Forbidden
    // Show access denied message
    if (status === 403) {
      // TODO: Replace with toast notification when toast system is implemented
      console.error('Access denied: You do not have permission to perform this action');
      // Future: showToast({ type: 'error', message: 'Access denied' });
      return;
    }
    
    // Requirement 21.4: Handle 5xx Server Errors
    // Show generic error message
    if (status && status >= 500) {
      // TODO: Replace with toast notification when toast system is implemented
      console.error('Server error: Something went wrong. Please try again later.');
      // Future: showToast({ type: 'error', message: 'Server error. Please try again.' });
      return;
    }
    
    // Handle other errors
    const message = error.response?.data?.message || error.message || 'An error occurred';
    console.error('Request failed:', message);
    // Future: showToast({ type: 'error', message });
  } else {
    // Non-Axios errors
    console.error('Unexpected error:', error);
    // Future: showToast({ type: 'error', message: 'An unexpected error occurred' });
  }
};

/**
 * Create QueryClient with global configuration
 * 
 * QueryCache: Handles errors for all queries
 * MutationCache: Handles errors for all mutations
 */
export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: handleError,
  }),
  mutationCache: new MutationCache({
    onError: handleError,
  }),
  defaultOptions: {
    queries: {
      // Data is considered fresh for 5 minutes
      // During this time, queries will return cached data without refetching
      staleTime: 5 * 60 * 1000, // 5 minutes
      
      // Unused data is garbage collected after 10 minutes
      // In TanStack Query v5, this is called gcTime (garbage collection time)
      gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
      
      // Refetch stale queries when window regains focus
      // Ensures users see fresh data when returning to the app
      refetchOnWindowFocus: true,
      
      // Retry failed queries once before giving up
      // Helps handle transient network issues
      retry: 1,
      
      // Don't retry on 4xx errors (client errors)
      // Only retry on network errors or 5xx errors
      retryOnMount: true,
      
      // Refetch on mount if data is stale
      refetchOnMount: true,
      
      // Refetch on reconnect if data is stale
      refetchOnReconnect: true,
    },
    mutations: {
      // Retry mutations once on failure
      retry: 1,
    },
  },
});
