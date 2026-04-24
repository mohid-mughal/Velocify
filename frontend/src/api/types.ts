/**
 * TypeScript type definitions matching backend DTOs
 * 
 * These types correspond to the C# DTOs in Velocify.Application/DTOs/
 * and ensure type safety between frontend and backend.
 */

// ============== Enums ==============

export type UserRole = 'SuperAdmin' | 'Admin' | 'Member';

export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled' | 'Blocked';

export type TaskPriority = 'Critical' | 'High' | 'Medium' | 'Low';

export type TaskCategory = 'Development' | 'Design' | 'Marketing' | 'Operations' | 'Research' | 'Other';

export type NotificationType = 'DueSoon' | 'Overdue' | 'Assigned' | 'StatusChanged' | 'AiSuggestion';

// ============== Common DTOs ==============

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============== User DTOs ==============

export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  productivityScore: number;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

export interface UserSummaryDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
}

export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
  expiresIn: number;
}

export interface ProductivityDto {
  currentScore: number;
  history: ProductivityHistoryPoint[];
}

export interface ProductivityHistoryPoint {
  date: string;
  score: number;
}

// ============== Task DTOs ==============

export interface TaskDto {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  category: TaskCategory;
  assignedTo: UserSummaryDto;
  createdBy: UserSummaryDto;
  dueDate: string | null;
  completedAt: string | null;
  estimatedHours: number | null;
  actualHours: number | null;
  tags: string;
  aiPriorityScore: number | null;
  predictedCompletionProbability: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface TaskDetailDto extends TaskDto {
  comments: CommentDto[];
  auditLogs: TaskAuditLogDto[];
  subtasks: TaskDto[];
}

export interface CommentDto {
  id: string;
  taskItemId: string;
  user: UserSummaryDto;
  content: string;
  sentimentScore: number | null;
  createdAt: string;
}

export interface TaskAuditLogDto {
  id: string;
  taskItemId: string;
  changedBy: UserSummaryDto;
  fieldName: string;
  oldValue: string | null;
  newValue: string | null;
  changedAt: string;
}

// ============== Dashboard DTOs ==============

export interface DashboardSummaryDto {
  pendingCount: number;
  inProgressCount: number;
  completedCount: number;
  blockedCount: number;
  overdueCount: number;
  dueTodayCount: number;
}

export interface VelocityDataPoint {
  date: string;
  completedCount: number;
}

export interface WorkloadDistributionDto {
  user: UserSummaryDto;
  totalTaskCount: number;
  pendingCount: number;
  inProgressCount: number;
  completedCount: number;
  blockedCount: number;
}

// ============== Notification DTOs ==============

export interface NotificationDto {
  id: string;
  userId: string;
  type: NotificationType;
  message: string;
  isRead: boolean;
  createdAt: string;
  taskItemId: string | null;
}

// ============== AI DTOs ==============

export interface SubtaskSuggestion {
  title: string;
  estimatedHours: number | null;
}

export interface WorkloadSuggestion {
  taskId: string;
  suggestedAssigneeId: string;
  reason: string;
}

export interface TaskImportRow {
  title: string;
  description: string;
  priority: TaskPriority | null;
  category: TaskCategory | null;
  dueDate: string | null;
  estimatedHours: number | null;
  assigneeEmail: string | null;
  tags: string[];
}

// ============== Request Types ==============

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  priority: TaskPriority;
  category: TaskCategory;
  assignedToUserId: string | null;
  dueDate: string | null;
  estimatedHours: number | null;
  tags: string[];
  parentTaskId: string | null;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  priority?: TaskPriority;
  category?: TaskCategory;
  assignedToUserId?: string | null;
  dueDate?: string | null;
  estimatedHours?: number | null;
  actualHours?: number | null;
  tags?: string[];
}

export interface UpdateTaskStatusRequest {
  status: TaskStatus;
}

export interface CreateCommentRequest {
  content: string;
}

export interface UpdateCurrentUserRequest {
  firstName?: string;
  lastName?: string;
}

export interface UpdateUserRoleRequest {
  role: UserRole;
}

export interface TaskFilters {
  status?: TaskStatus[];
  priority?: TaskPriority[];
  category?: TaskCategory[];
  assignedToUserId?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface NotificationFilters {
  isRead?: boolean;
  page?: number;
  pageSize?: number;
}

export interface UserFilters {
  page?: number;
  pageSize?: number;
}

export interface SemanticSearchRequest {
  query: string;
  useSemanticSearch: boolean;
}

export interface NormalizeImportRequest {
  csvData: string;
}
