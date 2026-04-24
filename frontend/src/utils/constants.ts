/**
 * Constants and Configuration
 * 
 * Task: 28.1
 * Requirements: All frontend requirements
 * 
 * Centralized constants for API URLs, enum mappings, and configuration values
 */

// ============== API Configuration ==============

export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL || '/api/v1',
  TIMEOUT: 30000, // 30 seconds
  RETRY_ATTEMPTS: 3,
  RETRY_DELAY: 1000, // 1 second
} as const;

// ============== API Endpoints ==============

export const API_ENDPOINTS = {
  // Auth
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    REFRESH: '/auth/refresh',
    LOGOUT: '/auth/logout',
    REVOKE_ALL: '/auth/revoke-all-sessions',
  },
  
  // Users
  USERS: {
    ME: '/users/me',
    LIST: '/users',
    BY_ID: (id: string) => `/users/${id}`,
    UPDATE_ROLE: (id: string) => `/users/${id}/role`,
    PRODUCTIVITY: (id: string) => `/users/${id}/productivity`,
  },
  
  // Tasks
  TASKS: {
    LIST: '/tasks',
    BY_ID: (id: string) => `/tasks/${id}`,
    STATUS: (id: string) => `/tasks/${id}/status`,
    COMMENTS: (id: string) => `/tasks/${id}/comments`,
    COMMENT_BY_ID: (taskId: string, commentId: string) => `/tasks/${taskId}/comments/${commentId}`,
    SUBTASKS: (id: string) => `/tasks/${id}/subtasks`,
    HISTORY: (id: string) => `/tasks/${id}/history`,
    EXPORT: '/tasks/export',
    IMPORT: '/tasks/import',
  },
  
  // Dashboard
  DASHBOARD: {
    SUMMARY: '/dashboard/summary',
    VELOCITY: '/dashboard/velocity',
    WORKLOAD: '/dashboard/workload',
    OVERDUE: '/dashboard/overdue',
  },
  
  // AI
  AI: {
    PARSE_TASK: '/ai/parse-task',
    DECOMPOSE: (id: string) => `/ai/decompose/${id}`,
    SEARCH: '/ai/search',
    WORKLOAD_SUGGESTIONS: '/ai/workload-suggestions',
    NORMALIZE_IMPORT: '/ai/normalize-import',
    MY_DIGEST: '/ai/my-digest',
  },
  
  // Notifications
  NOTIFICATIONS: {
    LIST: '/notifications',
    MARK_READ: (id: string) => `/notifications/${id}/read`,
    MARK_ALL_READ: '/notifications/mark-all-read',
  },
  
  // Health
  HEALTH: '/health',
} as const;

// ============== SignalR Configuration ==============

export const SIGNALR_CONFIG = {
  HUB_URL: '/hubs/tasks',
  RECONNECT_DELAYS: [0, 2000, 5000, 10000, 30000], // Exponential backoff
  EVENTS: {
    TASK_ASSIGNED: 'TaskAssigned',
    STATUS_CHANGED: 'StatusChanged',
    COMMENT_ADDED: 'CommentAdded',
    AI_SUGGESTION_READY: 'AiSuggestionReady',
  },
} as const;

// ============== Enum Display Mappings ==============

export const TASK_STATUS_LABELS: Record<string, string> = {
  Pending: 'Pending',
  InProgress: 'In Progress',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  Blocked: 'Blocked',
} as const;

export const TASK_STATUS_COLORS: Record<string, string> = {
  Pending: 'bg-gray-100 text-gray-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Completed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
  Blocked: 'bg-yellow-100 text-yellow-800',
} as const;

export const TASK_PRIORITY_LABELS: Record<string, string> = {
  Critical: 'Critical',
  High: 'High',
  Medium: 'Medium',
  Low: 'Low',
} as const;

export const TASK_PRIORITY_COLORS: Record<string, string> = {
  Critical: 'bg-red-100 text-red-800',
  High: 'bg-orange-100 text-orange-800',
  Medium: 'bg-yellow-100 text-yellow-800',
  Low: 'bg-green-100 text-green-800',
} as const;

export const TASK_CATEGORY_LABELS: Record<string, string> = {
  Development: 'Development',
  Design: 'Design',
  Marketing: 'Marketing',
  Operations: 'Operations',
  Research: 'Research',
  Other: 'Other',
} as const;

export const TASK_CATEGORY_ICONS: Record<string, string> = {
  Development: '💻',
  Design: '🎨',
  Marketing: '📢',
  Operations: '⚙️',
  Research: '🔬',
  Other: '📋',
} as const;

export const USER_ROLE_LABELS: Record<string, string> = {
  SuperAdmin: 'Super Admin',
  Admin: 'Admin',
  Member: 'Member',
} as const;

export const USER_ROLE_COLORS: Record<string, string> = {
  SuperAdmin: 'bg-purple-100 text-purple-800',
  Admin: 'bg-blue-100 text-blue-800',
  Member: 'bg-gray-100 text-gray-800',
} as const;

export const NOTIFICATION_TYPE_LABELS: Record<string, string> = {
  DueSoon: 'Due Soon',
  Overdue: 'Overdue',
  Assigned: 'Assigned',
  StatusChanged: 'Status Changed',
  AiSuggestion: 'AI Suggestion',
} as const;

export const NOTIFICATION_TYPE_ICONS: Record<string, string> = {
  DueSoon: '⏰',
  Overdue: '🚨',
  Assigned: '📌',
  StatusChanged: '🔄',
  AiSuggestion: '🤖',
} as const;

// ============== UI Configuration ==============

export const UI_CONFIG = {
  // Pagination
  DEFAULT_PAGE_SIZE: 20,
  PAGE_SIZE_OPTIONS: [10, 20, 50, 100],
  MAX_PAGE_SIZE: 100,
  
  // Debounce
  SEARCH_DEBOUNCE_MS: 300,
  INPUT_DEBOUNCE_MS: 500,
  
  // Toast
  TOAST_DURATION: 5000, // 5 seconds
  TOAST_ERROR_DURATION: 7000, // 7 seconds
  
  // Date Ranges
  DEFAULT_VELOCITY_DAYS: 30,
  PRODUCTIVITY_HISTORY_WEEKS: 12,
  
  // File Upload
  MAX_FILE_SIZE: 5 * 1024 * 1024, // 5MB
  ALLOWED_FILE_TYPES: ['.csv'],
  
  // Validation
  MIN_PASSWORD_LENGTH: 8,
  MAX_TITLE_LENGTH: 200,
  MAX_DESCRIPTION_LENGTH: 5000,
  MAX_COMMENT_LENGTH: 2000,
  MAX_TAGS: 10,
  
  // AI
  MAX_SUBTASKS: 8,
  MIN_AI_SCORE: 0,
  MAX_AI_SCORE: 1,
} as const;

// ============== Route Paths ==============

export const ROUTES = {
  HOME: '/',
  LOGIN: '/login',
  REGISTER: '/register',
  DASHBOARD: '/dashboard',
  TASKS: '/tasks',
  TASK_DETAIL: (id: string) => `/tasks/${id}`,
  TASK_NEW: '/tasks/new',
  TASK_EDIT: (id: string) => `/tasks/${id}/edit`,
  PROFILE: '/profile',
  NOTIFICATIONS: '/notifications',
  ADMIN: '/admin',
  NOT_FOUND: '/404',
} as const;

// ============== Local Storage Keys ==============

export const STORAGE_KEYS = {
  THEME: 'velocify_theme',
  LANGUAGE: 'velocify_language',
  TASK_FILTERS: 'velocify_task_filters',
  DASHBOARD_LAYOUT: 'velocify_dashboard_layout',
} as const;

// ============== Chart Colors ==============

export const CHART_COLORS = {
  PRIMARY: '#3b82f6', // blue-500
  SUCCESS: '#10b981', // green-500
  WARNING: '#f59e0b', // amber-500
  DANGER: '#ef4444', // red-500
  INFO: '#06b6d4', // cyan-500
  NEUTRAL: '#6b7280', // gray-500
  
  // Status colors for charts
  PENDING: '#9ca3af', // gray-400
  IN_PROGRESS: '#3b82f6', // blue-500
  COMPLETED: '#10b981', // green-500
  BLOCKED: '#f59e0b', // amber-500
  CANCELLED: '#ef4444', // red-500
  
  // Priority colors for charts
  CRITICAL: '#dc2626', // red-600
  HIGH: '#f97316', // orange-500
  MEDIUM: '#eab308', // yellow-500
  LOW: '#22c55e', // green-500
} as const;

// ============== Sentiment Thresholds ==============

export const SENTIMENT_CONFIG = {
  POSITIVE_THRESHOLD: 0.6,
  NEGATIVE_THRESHOLD: 0.4,
  COLORS: {
    POSITIVE: 'text-green-600',
    NEUTRAL: 'text-gray-600',
    NEGATIVE: 'text-red-600',
  },
  ICONS: {
    POSITIVE: '😊',
    NEUTRAL: '😐',
    NEGATIVE: '😟',
  },
} as const;

// ============== Date Formats ==============

export const DATE_FORMATS = {
  DISPLAY: 'MMM dd, yyyy',
  DISPLAY_WITH_TIME: 'MMM dd, yyyy HH:mm',
  INPUT: 'yyyy-MM-dd',
  ISO: "yyyy-MM-dd'T'HH:mm:ss",
  RELATIVE: 'relative', // Special marker for relative dates
} as const;

// ============== Error Messages ==============

export const ERROR_MESSAGES = {
  NETWORK_ERROR: 'Network error. Please check your connection.',
  UNAUTHORIZED: 'You are not authorized to perform this action.',
  NOT_FOUND: 'The requested resource was not found.',
  SERVER_ERROR: 'An unexpected error occurred. Please try again.',
  VALIDATION_ERROR: 'Please check your input and try again.',
  SESSION_EXPIRED: 'Your session has expired. Please log in again.',
} as const;

// ============== Success Messages ==============

export const SUCCESS_MESSAGES = {
  TASK_CREATED: 'Task created successfully',
  TASK_UPDATED: 'Task updated successfully',
  TASK_DELETED: 'Task deleted successfully',
  COMMENT_ADDED: 'Comment added successfully',
  COMMENT_DELETED: 'Comment deleted successfully',
  STATUS_UPDATED: 'Status updated successfully',
  PROFILE_UPDATED: 'Profile updated successfully',
  NOTIFICATION_READ: 'Notification marked as read',
  ALL_NOTIFICATIONS_READ: 'All notifications marked as read',
} as const;
