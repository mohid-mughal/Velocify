/**
 * Hooks Index
 * 
 * Central export point for all custom hooks.
 */

// Authentication hooks
export {
  useAuth,
  useLogin,
  useRegister,
  useLogout,
} from './useAuth';

// Task hooks
export {
  useTasks,
  useTask,
  useCreateTask,
  useUpdateTask,
  useUpdateTaskStatus,
  useDeleteTask,
  useTaskComments,
  useCreateComment,
  useDeleteComment,
  useTaskHistory,
  useTaskSubtasks,
} from './useTasks';
export type { TaskDto, TaskDetailDto, CommentDto, TaskFilters } from './useTasks';

// AI hooks
export {
  useAiParse,
  useAiDecompose,
  useSemanticSearch,
  useWorkloadSuggestions,
  useDigest,
  useNormalizeImport,
} from './useAi';
export type {
  CreateTaskRequest,
  SubtaskSuggestion,
  WorkloadSuggestion,
  TaskImportRow,
} from './useAi';

// Dashboard hooks
export {
  useDashboardSummary,
  useVelocity,
  useWorkload,
  useOverdue,
} from './useDashboard';
export type { DashboardSummaryDto, VelocityDataPoint, WorkloadDistributionDto } from './useDashboard';

// User hooks
export {
  useCurrentUser,
  useUpdateCurrentUser,
  useUsers,
  useUserById,
  useUpdateUserRole,
  useDeleteUser,
  useUserProductivity,
} from './useUsers';
export type { UserDto, ProductivityDto, UserFilters } from './useUsers';

// Utility hooks
export { useDebounce } from './useDebounce';
export { useInfiniteScroll } from './useInfiniteScroll';
export { useToast } from './useToast';
export type { Toast, ToastOptions, ToastType, ToastHookReturn } from './useToast';

// SignalR hook
export { useSignalR } from './useSignalR';
export type { ConnectionState } from './useSignalR';
