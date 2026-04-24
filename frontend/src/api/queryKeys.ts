/**
 * Query Keys Convention and Factory
 * 
 * Requirement 21.2: TanStack Query cache key management
 * 
 * Query keys are used to identify and manage cached data in TanStack Query.
 * This factory provides a consistent, type-safe way to generate query keys
 * across the application.
 * 
 * Key Structure:
 * - Level 1: Resource type (e.g., 'tasks', 'users', 'dashboard')
 * - Level 2: Operation or scope (e.g., 'list', 'detail', 'summary')
 * - Level 3+: Parameters (e.g., filters, IDs)
 * 
 * Benefits:
 * - Type safety: TypeScript ensures correct key structure
 * - Consistency: All keys follow the same pattern
 * - Easy invalidation: Can invalidate all keys for a resource or specific queries
 * - Debugging: Clear, readable keys in React Query DevTools
 * 
 * Examples:
 * - ['tasks', 'list'] - All tasks
 * - ['tasks', 'list', { status: 'InProgress' }] - Filtered tasks
 * - ['tasks', 'detail', '123'] - Specific task
 * - ['dashboard', 'summary'] - Dashboard summary
 * 
 * Invalidation Examples:
 * - queryClient.invalidateQueries({ queryKey: ['tasks'] }) - Invalidate all task queries
 * - queryClient.invalidateQueries({ queryKey: ['tasks', 'list'] }) - Invalidate all task lists
 * - queryClient.invalidateQueries({ queryKey: ['tasks', 'detail', taskId] }) - Invalidate specific task
 */

// Task-related query keys
export const taskKeys = {
  // All task queries
  all: ['tasks'] as const,
  
  // Task lists with optional filters
  lists: () => [...taskKeys.all, 'list'] as const,
  list: (filters?: {
    status?: string[];
    priority?: string[];
    category?: string[];
    assignedToUserId?: string;
    dueDateFrom?: string;
    dueDateTo?: string;
    searchTerm?: string;
    page?: number;
    pageSize?: number;
  }) => [...taskKeys.lists(), filters] as const,
  
  // Task details
  details: () => [...taskKeys.all, 'detail'] as const,
  detail: (id: string) => [...taskKeys.details(), id] as const,
  
  // Task comments
  comments: (taskId: string) => [...taskKeys.all, 'comments', taskId] as const,
  
  // Task audit history
  history: (taskId: string) => [...taskKeys.all, 'history', taskId] as const,
  
  // Task subtasks
  subtasks: (taskId: string) => [...taskKeys.all, 'subtasks', taskId] as const,
};

// User-related query keys
export const userKeys = {
  // All user queries
  all: ['users'] as const,
  
  // Current user
  me: () => [...userKeys.all, 'me'] as const,
  
  // User lists
  lists: () => [...userKeys.all, 'list'] as const,
  list: (filters?: { page?: number; pageSize?: number }) => 
    [...userKeys.lists(), filters] as const,
  
  // User details
  details: () => [...userKeys.all, 'detail'] as const,
  detail: (id: string) => [...userKeys.details(), id] as const,
  
  // User productivity
  productivity: (userId: string) => [...userKeys.all, 'productivity', userId] as const,
};

// Dashboard-related query keys
export const dashboardKeys = {
  // All dashboard queries
  all: ['dashboard'] as const,
  
  // Dashboard summary
  summary: () => [...dashboardKeys.all, 'summary'] as const,
  
  // Dashboard velocity
  velocity: (days?: number) => [...dashboardKeys.all, 'velocity', days] as const,
  
  // Workload distribution (Admin only)
  workload: () => [...dashboardKeys.all, 'workload'] as const,
  
  // Overdue tasks
  overdue: () => [...dashboardKeys.all, 'overdue'] as const,
};

// AI-related query keys
export const aiKeys = {
  // All AI queries
  all: ['ai'] as const,
  
  // AI task parsing
  parseTask: (input: string) => [...aiKeys.all, 'parse-task', input] as const,
  
  // AI task decomposition
  decompose: (taskId: string) => [...aiKeys.all, 'decompose', taskId] as const,
  
  // AI semantic search
  search: (query: string) => [...aiKeys.all, 'search', query] as const,
  
  // AI workload suggestions (Admin only)
  workloadSuggestions: () => [...aiKeys.all, 'workload-suggestions'] as const,
  
  // AI daily digest
  digest: () => [...aiKeys.all, 'digest'] as const,
};

// Notification-related query keys
export const notificationKeys = {
  // All notification queries
  all: ['notifications'] as const,
  
  // Notification lists
  lists: () => [...notificationKeys.all, 'list'] as const,
  list: (filters?: { isRead?: boolean; page?: number; pageSize?: number }) => 
    [...notificationKeys.lists(), filters] as const,
  
  // Unread count
  unreadCount: () => [...notificationKeys.all, 'unread-count'] as const,
};

// Auth-related query keys
export const authKeys = {
  // All auth queries
  all: ['auth'] as const,
  
  // Current session
  session: () => [...authKeys.all, 'session'] as const,
};

/**
 * Helper function to invalidate all queries for a specific resource
 * 
 * Usage:
 * - invalidateResource(queryClient, taskKeys) - Invalidate all task queries
 * - invalidateResource(queryClient, userKeys) - Invalidate all user queries
 */
export const invalidateResource = (
  queryClient: any,
  resourceKeys: { all: readonly string[] }
) => {
  return queryClient.invalidateQueries({ queryKey: resourceKeys.all });
};

/**
 * Export all query keys for easy access
 */
export const queryKeys = {
  tasks: taskKeys,
  users: userKeys,
  dashboard: dashboardKeys,
  ai: aiKeys,
  notifications: notificationKeys,
  auth: authKeys,
};
