/**
 * Task Hooks
 * 
 * Requirements: 3.1-3.8, 4.1-4.8, 5.1-5.6
 * 
 * Custom hooks for task operations using TanStack Query:
 * - useTasks: Query for task list with filters
 * - useTask: Query for single task details
 * - useCreateTask: Mutation for creating tasks
 * - useUpdateTask: Mutation for updating tasks
 * - useUpdateTaskStatus: Mutation for updating task status
 * - useDeleteTask: Mutation for deleting tasks
 * - useTaskComments: Query for task comments
 * - useCreateComment: Mutation for creating comments
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { taskKeys } from '../api/queryKeys';
import { tasksService } from '../api/tasks.service';
import type {
  TaskDto,
  TaskDetailDto,
  CommentDto,
  TaskFilters,
  CreateTaskRequest,
  UpdateTaskRequest,
  UpdateTaskStatusRequest,
  CreateCommentRequest,
} from '../api/types';

/**
 * Hook to fetch a paginated list of tasks with optional filters
 * 
 * Requirement 3.1: Users can view a list of tasks
 * Requirement 4.1-4.8: Task filtering and search
 * Requirement 22.1: Filter panel with status, priority, category, assignee, due date range
 * 
 * @param filters - Optional filters for status, priority, category, assignee, due date, search
 * @returns Query result with items, totalCount, page, pageSize, totalPages
 */
export function useTasks(filters?: TaskFilters) {
  return useQuery({
    queryKey: taskKeys.list(filters),
    queryFn: () => tasksService.getTasks(filters),
  });
}

/**
 * Hook to fetch a single task by ID with full details
 * 
 * Requirement 3.2: Users can view task details
 * Requirement 23.1: Full task information display with comments, audit logs, subtasks
 * 
 * @param taskId - Task ID to fetch
 * @returns Query result with TaskDetailDto
 */
export function useTask(taskId: string) {
  return useQuery({
    queryKey: taskKeys.detail(taskId),
    queryFn: () => tasksService.getTaskById(taskId),
    enabled: !!taskId,
  });
}

/**
 * Hook to create a new task
 * 
 * Requirement 3.3: Users can create tasks
 * Requirement 24.1: Task form with all fields
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useCreateTask() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (newTask: CreateTaskRequest) => tasksService.createTask(newTask),
    onSuccess: () => {
      // Invalidate all task lists to refetch with the new task
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      
      // Also invalidate dashboard since task counts changed
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

/**
 * Hook to update an existing task
 * 
 * Requirement 3.4: Users can update tasks
 * Requirement 4.1: Task owners and admins can edit tasks
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useUpdateTask() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ taskId, data }: { taskId: string; data: UpdateTaskRequest }) =>
      tasksService.updateTask(taskId, data),
    onSuccess: (_, { taskId }) => {
      // Invalidate the specific task detail
      queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      
      // Invalidate all task lists since the task may appear in multiple lists
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      
      // Invalidate task history since updates are logged
      queryClient.invalidateQueries({ queryKey: taskKeys.history(taskId) });
    },
  });
}

/**
 * Hook to update task status
 * 
 * Requirement 3.5: Users can update task status
 * Requirement 4.2: Status transitions follow business rules
 * 
 * Demonstrates optimistic updates for better UX with rollback on error.
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useUpdateTaskStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ taskId, status }: { taskId: string; status: UpdateTaskStatusRequest }) =>
      tasksService.updateTaskStatus(taskId, status),
    // Optimistic update: Update cache immediately before request completes
    onMutate: async ({ taskId, status }) => {
      // Cancel any outgoing refetches to avoid overwriting our optimistic update
      await queryClient.cancelQueries({ queryKey: taskKeys.detail(taskId) });

      // Snapshot the previous value for rollback
      const previousTask = queryClient.getQueryData<TaskDetailDto>(taskKeys.detail(taskId));

      // Optimistically update the cache
      if (previousTask) {
        queryClient.setQueryData<TaskDetailDto>(taskKeys.detail(taskId), {
          ...previousTask,
          status: status.status,
          updatedAt: new Date().toISOString(),
          // Set completedAt if status is Completed
          ...(status.status === 'Completed' && { completedAt: new Date().toISOString() }),
        });
      }

      return { previousTask };
    },
    // If mutation fails, rollback to the previous value
    onError: (_err, { taskId }, context) => {
      if (context?.previousTask) {
        queryClient.setQueryData(taskKeys.detail(taskId), context.previousTask);
      }
    },
    // Always refetch after error or success to ensure cache is in sync
    onSettled: (_data, _error, { taskId }) => {
      queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

/**
 * Hook to delete a task (soft delete)
 * 
 * Requirement 3.6: Users can delete tasks
 * Requirement 4.3: Tasks are soft-deleted, not permanently removed
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useDeleteTask() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (taskId: string) => tasksService.deleteTask(taskId),
    onSuccess: () => {
      // Invalidate all task-related queries
      queryClient.invalidateQueries({ queryKey: taskKeys.all });
      
      // Also invalidate dashboard since task counts changed
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

/**
 * Hook to fetch comments for a task
 * 
 * Requirement 5.1: Users can view comments on tasks
 * Requirement 23.1: Comment thread with sentiment indicators
 * 
 * @param taskId - Task ID to fetch comments for
 * @returns Query result with array of CommentDto
 */
export function useTaskComments(taskId: string) {
  return useQuery({
    queryKey: taskKeys.comments(taskId),
    queryFn: () => tasksService.getComments(taskId),
    enabled: !!taskId,
  });
}

/**
 * Hook to create a comment on a task
 * 
 * Requirement 5.2: Users can add comments to tasks
 * Requirement 14.1: Comments are analyzed for sentiment
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useCreateComment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ taskId, content }: { taskId: string; content: CreateCommentRequest }) =>
      tasksService.createComment(taskId, content),
    onSuccess: (_, { taskId }) => {
      // Invalidate comments for this task
      queryClient.invalidateQueries({ queryKey: taskKeys.comments(taskId) });
      
      // Invalidate the task detail since it includes comments
      queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
    },
  });
}

/**
 * Hook to delete a comment
 * 
 * Requirement 5.3: Users can delete their own comments
 * Requirement 5.4: Admins can delete any comment
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useDeleteComment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ taskId, commentId }: { taskId: string; commentId: string }) =>
      tasksService.deleteComment(taskId, commentId),
    onSuccess: (_, { taskId }) => {
      // Invalidate comments for this task
      queryClient.invalidateQueries({ queryKey: taskKeys.comments(taskId) });
      
      // Invalidate the task detail since it includes comments
      queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
    },
  });
}

/**
 * Hook to fetch task audit history
 * 
 * Requirement 4.4: Users can view task history
 * Requirement 23.1: Task audit history timeline
 * 
 * @param taskId - Task ID to fetch history for
 * @returns Query result with array of TaskAuditLogDto
 */
export function useTaskHistory(taskId: string) {
  return useQuery({
    queryKey: taskKeys.history(taskId),
    queryFn: () => tasksService.getTaskHistory(taskId),
    enabled: !!taskId,
  });
}

/**
 * Hook to fetch subtasks for a task
 * 
 * Requirement 4.5: Tasks can have subtasks
 * Requirement 9.1: AI can decompose tasks into subtasks
 * 
 * @param taskId - Parent task ID to fetch subtasks for
 * @returns Query result with array of TaskDto
 */
export function useTaskSubtasks(taskId: string) {
  return useQuery({
    queryKey: taskKeys.subtasks(taskId),
    queryFn: () => tasksService.getSubtasks(taskId),
    enabled: !!taskId,
  });
}

// Re-export types for convenience
export type { TaskDto, TaskDetailDto, CommentDto, TaskFilters };
