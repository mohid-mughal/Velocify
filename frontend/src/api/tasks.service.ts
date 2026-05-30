/**
 * Tasks API Service
 * 
 * Requirements: 3.1-3.8, 4.1-4.8, 5.1-5.6
 * 
 * Handles all task-related API calls:
 * - Task CRUD operations
 * - Task status updates
 * - Task comments
 * - Task history/audit logs
 * - Task subtasks
 * - Task export/import
 */

import axiosInstance from './axios';
import type {
  TaskDto,
  TaskDetailDto,
  CommentDto,
  TaskAuditLogDto,
  PagedResult,
  TaskFilters,
  CreateTaskRequest,
  UpdateTaskRequest,
  UpdateTaskStatusRequest,
  CreateCommentRequest,
} from './types';

/**
 * Get a paginated list of tasks with optional filters
 * 
 * Requirement 3.1: Users can view a list of tasks
 * Requirement 22.1: Filter panel with status, priority, category, assignee, due date range
 * 
 * @param filters - Optional filters for status, priority, category, assignee, due date, search
 * @returns PagedResult of TaskDto
 */
export async function getTasks(filters?: TaskFilters): Promise<PagedResult<TaskDto>> {
  const response = await axiosInstance.get<PagedResult<TaskDto>>('/tasks', {
    params: filters,
  });
  return response.data;
}

/**
 * Get a single task by ID with full details
 * 
 * Requirement 3.2: Users can view task details
 * Requirement 23.1: Full task information display
 * 
 * @param id - Task ID
 * @returns TaskDetailDto with comments, audit logs, and subtasks
 */
export async function getTaskById(id: string): Promise<TaskDetailDto> {
  const response = await axiosInstance.get<TaskDetailDto>(`/tasks/${id}`);
  return response.data;
}

/**
 * Create a new task
 * 
 * Requirement 3.3: Users can create tasks
 * Requirement 24.1: Task form with all fields
 * 
 * @param data - Task creation data
 * @returns Created TaskDto
 */
export async function createTask(data: CreateTaskRequest): Promise<TaskDto> {
  // Convert tags array to comma-separated string if needed
  // Backend expects a string, not an array
  const tags = Array.isArray(data.tags) 
    ? data.tags.filter(Boolean).join(',') 
    : (data.tags || '');
  
  // Convert date string to ISO 8601 format if provided
  // HTML5 date input returns "YYYY-MM-DD", backend expects "YYYY-MM-DDTHH:mm:ssZ"
  let dueDate: string | null = null;
  if (data.dueDate) {
    try {
      // Parse the date string and convert to ISO format
      const date = new Date(data.dueDate);
      // Check if date is valid
      if (!isNaN(date.getTime())) {
        dueDate = date.toISOString();
      }
    } catch (error) {
      console.warn('Invalid date format:', data.dueDate);
    }
  }
  
  const payload = {
    title: data.title,
    description: data.description,
    priority: data.priority,
    category: data.category,
    assignedToUserId: data.assignedToUserId,
    dueDate: dueDate,
    estimatedHours: data.estimatedHours,
    tags: tags,
  };
  
  console.log('Creating task with payload:', JSON.stringify(payload, null, 2));
  
  try {
    const response = await axiosInstance.post<TaskDto>('/tasks', payload);
    return response.data;
  } catch (error: any) {
    console.error('Task creation error:', error.response?.data || error.message);
    throw error;
  }
}

/**
 * Update an existing task
 * 
 * Requirement 3.4: Users can update tasks
 * Requirement 4.1: Task owners and admins can edit tasks
 * 
 * @param id - Task ID
 * @param data - Task update data (partial)
 * @returns Updated TaskDto
 */
export async function updateTask(id: string, data: UpdateTaskRequest): Promise<TaskDto> {
  // Convert tags array to comma-separated string if needed
  const tags = data.tags 
    ? (Array.isArray(data.tags) ? data.tags.filter(Boolean).join(',') : data.tags)
    : undefined;
  
  // Convert date string to ISO 8601 format if provided
  let dueDate: string | null | undefined = data.dueDate;
  if (data.dueDate && typeof data.dueDate === 'string') {
    try {
      const date = new Date(data.dueDate);
      if (!isNaN(date.getTime())) {
        dueDate = date.toISOString();
      }
    } catch (error) {
      console.warn('Invalid date format:', data.dueDate);
    }
  }
  
  const payload = {
    ...data,
    dueDate: dueDate,
    tags: tags,
  };
  
  const response = await axiosInstance.put<TaskDto>(`/tasks/${id}`, payload);
  return response.data;
}

/**
 * Update task status
 * 
 * Requirement 3.5: Users can update task status
 * Requirement 4.2: Status transitions follow business rules
 * 
 * @param id - Task ID
 * @param status - New status
 * @returns Updated TaskDto
 */
export async function updateTaskStatus(id: string, status: UpdateTaskStatusRequest): Promise<TaskDto> {
  const response = await axiosInstance.patch<TaskDto>(`/tasks/${id}/status`, status);
  return response.data;
}

/**
 * Soft delete a task
 * 
 * Requirement 3.6: Users can delete tasks
 * Requirement 4.3: Tasks are soft-deleted, not permanently removed
 * 
 * @param id - Task ID
 */
export async function deleteTask(id: string): Promise<void> {
  await axiosInstance.delete(`/tasks/${id}`);
}

/**
 * Get task audit history
 * 
 * Requirement 4.4: Users can view task history
 * Requirement 23.1: Task audit history timeline
 * 
 * @param id - Task ID
 * @returns List of TaskAuditLogDto
 */
export async function getTaskHistory(id: string): Promise<TaskAuditLogDto[]> {
  const response = await axiosInstance.get<TaskAuditLogDto[]>(`/tasks/${id}/history`);
  return response.data;
}

/**
 * Get comments for a task
 * 
 * Requirement 5.1: Users can view comments on tasks
 * Requirement 23.1: Comment thread with sentiment indicators
 * 
 * @param taskId - Task ID
 * @returns List of CommentDto
 */
export async function getComments(taskId: string): Promise<CommentDto[]> {
  const response = await axiosInstance.get<CommentDto[]>(`/tasks/${taskId}/comments`);
  return response.data;
}

/**
 * Create a comment on a task
 * 
 * Requirement 5.2: Users can add comments to tasks
 * Requirement 14.1: Comments are analyzed for sentiment
 * 
 * @param taskId - Task ID
 * @param content - Comment content
 * @returns Created CommentDto
 */
export async function createComment(taskId: string, content: CreateCommentRequest): Promise<CommentDto> {
  const response = await axiosInstance.post<CommentDto>(`/tasks/${taskId}/comments`, content);
  return response.data;
}

/**
 * Delete a comment
 * 
 * Requirement 5.3: Users can delete their own comments
 * Requirement 5.4: Admins can delete any comment
 * 
 * @param taskId - Task ID
 * @param commentId - Comment ID
 */
export async function deleteComment(taskId: string, commentId: string): Promise<void> {
  await axiosInstance.delete(`/tasks/${taskId}/comments/${commentId}`);
}

/**
 * Get subtasks for a task
 * 
 * Requirement 4.5: Tasks can have subtasks
 * Requirement 9.1: AI can decompose tasks into subtasks
 * 
 * @param taskId - Parent task ID
 * @returns List of TaskDto (subtasks)
 */
export async function getSubtasks(taskId: string): Promise<TaskDto[]> {
  const response = await axiosInstance.get<TaskDto[]>(`/tasks/${taskId}/subtasks`);
  return response.data;
}

/**
 * Export tasks to CSV
 * 
 * Requirement 4.6: Users can export tasks
 * 
 * @param filters - Optional filters for export
 * @returns Blob containing CSV data
 */
export async function exportTasks(filters?: TaskFilters): Promise<Blob> {
  const response = await axiosInstance.post<Blob>('/tasks/export', filters, {
    responseType: 'blob',
  });
  return response.data;
}

/**
 * Import tasks from CSV
 * 
 * Requirement 4.7: Users can import tasks
 * Requirement 13.1: AI can normalize imported data
 * 
 * @param file - CSV file to import
 * @returns List of imported TaskDto
 */
export async function importTasks(file: File): Promise<TaskDto[]> {
  const formData = new FormData();
  formData.append('file', file);
  
  const response = await axiosInstance.post<TaskDto[]>('/tasks/import', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  return response.data;
}

/**
 * Tasks service object with all task-related methods
 */
export const tasksService = {
  getTasks,
  getTaskById,
  createTask,
  updateTask,
  updateTaskStatus,
  deleteTask,
  getTaskHistory,
  getComments,
  createComment,
  deleteComment,
  getSubtasks,
  exportTasks,
  importTasks,
};

export default tasksService;
