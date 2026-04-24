# TanStack Query Configuration

This directory contains the TanStack Query (React Query v5) configuration for the Velocify frontend application.

## Files

### `queryClient.ts`
Configures the global QueryClient with:
- **staleTime**: 5 minutes - Data is considered fresh for 5 minutes
- **gcTime** (formerly cacheTime): 10 minutes - Unused data is garbage collected after 10 minutes
- **refetchOnWindowFocus**: true - Refetch stale queries when window regains focus
- **retry**: 1 - Retry failed queries once before giving up

#### Global Error Handling
The QueryClient includes global error handlers for both queries and mutations:

- **401 Unauthorized**: Clears auth state and redirects to login page
  - Note: The axios interceptor attempts token refresh first
  - If we reach the error handler, it means refresh failed
  
- **403 Forbidden**: Displays "Access denied" message
  - User attempted an action they don't have permission for
  
- **5xx Server Errors**: Displays generic error message
  - Server-side errors that the user can't fix
  
- **Other Errors**: Displays specific error message from API

### `queryKeys.ts`
Defines a consistent query key convention for cache management.

#### Key Structure
Query keys follow a hierarchical structure:
1. **Level 1**: Resource type (e.g., 'tasks', 'users', 'dashboard')
2. **Level 2**: Operation or scope (e.g., 'list', 'detail', 'summary')
3. **Level 3+**: Parameters (e.g., filters, IDs)

#### Available Key Factories

**Tasks**
```typescript
taskKeys.all                              // ['tasks']
taskKeys.lists()                          // ['tasks', 'list']
taskKeys.list({ status: ['InProgress'] }) // ['tasks', 'list', { status: ['InProgress'] }]
taskKeys.detail('task-id')                // ['tasks', 'detail', 'task-id']
taskKeys.comments('task-id')              // ['tasks', 'comments', 'task-id']
taskKeys.history('task-id')               // ['tasks', 'history', 'task-id']
taskKeys.subtasks('task-id')              // ['tasks', 'subtasks', 'task-id']
```

**Users**
```typescript
userKeys.all                    // ['users']
userKeys.me()                   // ['users', 'me']
userKeys.lists()                // ['users', 'list']
userKeys.list({ page: 1 })      // ['users', 'list', { page: 1 }]
userKeys.detail('user-id')      // ['users', 'detail', 'user-id']
userKeys.productivity('user-id') // ['users', 'productivity', 'user-id']
```

**Dashboard**
```typescript
dashboardKeys.all           // ['dashboard']
dashboardKeys.summary()     // ['dashboard', 'summary']
dashboardKeys.velocity(30)  // ['dashboard', 'velocity', 30]
dashboardKeys.workload()    // ['dashboard', 'workload']
dashboardKeys.overdue()     // ['dashboard', 'overdue']
```

**AI Features**
```typescript
aiKeys.all                          // ['ai']
aiKeys.parseTask('input text')      // ['ai', 'parse-task', 'input text']
aiKeys.decompose('task-id')         // ['ai', 'decompose', 'task-id']
aiKeys.search('query')              // ['ai', 'search', 'query']
aiKeys.workloadSuggestions()        // ['ai', 'workload-suggestions']
aiKeys.digest()                     // ['ai', 'digest']
```

**Notifications**
```typescript
notificationKeys.all                      // ['notifications']
notificationKeys.lists()                  // ['notifications', 'list']
notificationKeys.list({ isRead: false })  // ['notifications', 'list', { isRead: false }]
notificationKeys.unreadCount()            // ['notifications', 'unread-count']
```

## Usage Examples

### Basic Query
```typescript
import { useQuery } from '@tanstack/react-query';
import { queryKeys } from '../api/queryKeys';
import axiosInstance from '../api/axios';

function TaskList() {
  const { data, isLoading, error } = useQuery({
    queryKey: queryKeys.tasks.list({ status: ['InProgress'] }),
    queryFn: async () => {
      const response = await axiosInstance.get('/tasks', {
        params: { status: 'InProgress' }
      });
      return response.data;
    },
  });
  
  // ... render logic
}
```

### Mutation with Cache Invalidation
```typescript
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '../api/queryKeys';
import axiosInstance from '../api/axios';

function CreateTaskButton() {
  const queryClient = useQueryClient();
  
  const mutation = useMutation({
    mutationFn: async (newTask) => {
      const response = await axiosInstance.post('/tasks', newTask);
      return response.data;
    },
    onSuccess: () => {
      // Invalidate all task lists to refetch with new task
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.lists() });
    },
  });
  
  // ... render logic
}
```

### Optimistic Updates
```typescript
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '../api/queryKeys';
import axiosInstance from '../api/axios';

function UpdateTaskStatus({ taskId }) {
  const queryClient = useQueryClient();
  
  const mutation = useMutation({
    mutationFn: async (newStatus) => {
      const response = await axiosInstance.patch(`/tasks/${taskId}/status`, {
        status: newStatus
      });
      return response.data;
    },
    // Optimistically update the cache before the request completes
    onMutate: async (newStatus) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ 
        queryKey: queryKeys.tasks.detail(taskId) 
      });
      
      // Snapshot the previous value
      const previousTask = queryClient.getQueryData(
        queryKeys.tasks.detail(taskId)
      );
      
      // Optimistically update to the new value
      queryClient.setQueryData(
        queryKeys.tasks.detail(taskId),
        (old) => ({ ...old, status: newStatus })
      );
      
      // Return context with the snapshot
      return { previousTask };
    },
    // If mutation fails, rollback to the previous value
    onError: (err, newStatus, context) => {
      queryClient.setQueryData(
        queryKeys.tasks.detail(taskId),
        context.previousTask
      );
    },
    // Always refetch after error or success
    onSettled: () => {
      queryClient.invalidateQueries({ 
        queryKey: queryKeys.tasks.detail(taskId) 
      });
    },
  });
  
  // ... render logic
}
```

### Invalidating Multiple Related Queries
```typescript
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '../api/queryKeys';
import axiosInstance from '../api/axios';

function DeleteTask({ taskId }) {
  const queryClient = useQueryClient();
  
  const mutation = useMutation({
    mutationFn: async () => {
      await axiosInstance.delete(`/tasks/${taskId}`);
    },
    onSuccess: () => {
      // Invalidate all task-related queries
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.all });
      
      // Also invalidate dashboard since task counts changed
      queryClient.invalidateQueries({ queryKey: queryKeys.dashboard.all });
    },
  });
  
  // ... render logic
}
```

## Integration with SignalR

When real-time updates are received via SignalR, invalidate the relevant query keys:

```typescript
import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '../api/queryKeys';
import { signalRConnection } from '../api/signalr';

function useTaskUpdates() {
  const queryClient = useQueryClient();
  
  useEffect(() => {
    // Listen for task-assigned events
    signalRConnection.on('TaskAssigned', (taskId) => {
      // Invalidate task lists and dashboard
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.lists() });
      queryClient.invalidateQueries({ queryKey: queryKeys.dashboard.all });
    });
    
    // Listen for status-changed events
    signalRConnection.on('StatusChanged', (taskId) => {
      // Invalidate specific task and lists
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.detail(taskId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.lists() });
      queryClient.invalidateQueries({ queryKey: queryKeys.dashboard.all });
    });
    
    // Listen for comment-added events
    signalRConnection.on('CommentAdded', (taskId) => {
      // Invalidate task comments
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.comments(taskId) });
    });
    
    return () => {
      signalRConnection.off('TaskAssigned');
      signalRConnection.off('StatusChanged');
      signalRConnection.off('CommentAdded');
    };
  }, [queryClient]);
}
```

## Best Practices

1. **Always use query key factories**: Never hardcode query keys. Use the factories from `queryKeys.ts`.

2. **Invalidate related queries**: When a mutation affects multiple resources, invalidate all related queries.

3. **Use optimistic updates for better UX**: For mutations that are likely to succeed, update the cache optimistically.

4. **Leverage staleTime**: Set appropriate staleTime for different types of data:
   - Frequently changing data: Lower staleTime (1-2 minutes)
   - Rarely changing data: Higher staleTime (10-15 minutes)
   - Static data: Infinity

5. **Use React Query DevTools**: The DevTools are included in development mode. Use them to debug cache state and query behavior.

6. **Handle loading and error states**: Always handle loading and error states in your components.

7. **Prefetch data when possible**: Use `queryClient.prefetchQuery()` to load data before it's needed.

## Requirements Satisfied

- **21.2**: TanStack Query invalidates cache keys and refetches on mutations
- **21.3**: Handle 401 errors by clearing auth state and redirecting to login
- **21.4**: Handle 5xx errors by displaying toast notifications (console.error for now, toast system to be implemented)

## Future Enhancements

- [ ] Implement toast notification system to replace console.error calls
- [ ] Add query key prefetching for common navigation patterns
- [ ] Implement background refetching for critical data
- [ ] Add query persistence for offline support
- [ ] Implement query cancellation for expensive operations
