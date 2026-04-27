/**
 * Validation Schemas and Utilities
 * 
 * Task: 28.1
 * Requirements: All frontend requirements
 * 
 * Zod schemas for form validation across the application
 */

import { z } from 'zod';
import { UI_CONFIG } from './constants';

// ============== Auth Schemas ==============

/**
 * Login form validation schema
 * Requirement 1.3: Valid email and password required
 */
export const loginSchema = z.object({
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Invalid email address'),
  password: z
    .string()
    .min(1, 'Password is required'),
});

export type LoginFormData = z.infer<typeof loginSchema>;

/**
 * Registration form validation schema
 * Requirement 1.1: Valid email, password, first name, and last name required
 */
export const registerSchema = z.object({
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(100, 'First name must be less than 100 characters'),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(100, 'Last name must be less than 100 characters'),
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Invalid email address'),
  password: z
    .string()
    .min(UI_CONFIG.MIN_PASSWORD_LENGTH, `Password must be at least ${UI_CONFIG.MIN_PASSWORD_LENGTH} characters`)
    .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
    .regex(/[0-9]/, 'Password must contain at least one number')
    .regex(/[^A-Za-z0-9]/, 'Password must contain at least one special character'),
  confirmPassword: z
    .string()
    .min(1, 'Please confirm your password'),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

export type RegisterFormData = z.infer<typeof registerSchema>;

// ============== Task Schemas ==============

/**
 * Create task form validation schema
 * Requirement 3.1: Title, priority, and category required
 * Requirement 24.1: All task fields with validation
 */
export const createTaskSchema = z.object({
  title: z
    .string()
    .min(1, 'Title is required')
    .max(UI_CONFIG.MAX_TITLE_LENGTH, `Title must be less than ${UI_CONFIG.MAX_TITLE_LENGTH} characters`),
  description: z
    .string()
    .max(UI_CONFIG.MAX_DESCRIPTION_LENGTH, `Description must be less than ${UI_CONFIG.MAX_DESCRIPTION_LENGTH} characters`)
    .optional()
    .or(z.literal('')),
  priority: z.enum(['Critical', 'High', 'Medium', 'Low'], {
    required_error: 'Priority is required',
  }),
  category: z.enum(['Development', 'Design', 'Marketing', 'Operations', 'Research', 'Other'], {
    required_error: 'Category is required',
  }),
  assignedToUserId: z
    .string()
    .uuid('Invalid user ID')
    .nullable()
    .optional(),
  dueDate: z
    .string()
    .nullable()
    .optional()
    .refine((date) => {
      if (!date) return true;
      const parsedDate = new Date(date);
      return !isNaN(parsedDate.getTime());
    }, 'Invalid date format'),
  estimatedHours: z
    .number()
    .min(0, 'Estimated hours must be positive')
    .max(1000, 'Estimated hours must be less than 1000')
    .nullable()
    .optional(),
  tags: z
    .array(z.string())
    .max(UI_CONFIG.MAX_TAGS, `Maximum ${UI_CONFIG.MAX_TAGS} tags allowed`)
    .optional()
    .default([]),
  parentTaskId: z
    .string()
    .uuid('Invalid parent task ID')
    .nullable()
    .optional(),
});

export type CreateTaskFormData = z.infer<typeof createTaskSchema>;

/**
 * Update task form validation schema
 * Requirement 3.2: All fields optional for partial updates
 */
export const updateTaskSchema = z.object({
  title: z
    .string()
    .min(1, 'Title is required')
    .max(UI_CONFIG.MAX_TITLE_LENGTH, `Title must be less than ${UI_CONFIG.MAX_TITLE_LENGTH} characters`)
    .optional(),
  description: z
    .string()
    .max(UI_CONFIG.MAX_DESCRIPTION_LENGTH, `Description must be less than ${UI_CONFIG.MAX_DESCRIPTION_LENGTH} characters`)
    .optional()
    .or(z.literal('')),
  priority: z.enum(['Critical', 'High', 'Medium', 'Low']).optional(),
  category: z.enum(['Development', 'Design', 'Marketing', 'Operations', 'Research', 'Other']).optional(),
  assignedToUserId: z
    .string()
    .uuid('Invalid user ID')
    .nullable()
    .optional(),
  dueDate: z
    .string()
    .nullable()
    .optional()
    .refine((date) => {
      if (!date) return true;
      const parsedDate = new Date(date);
      return !isNaN(parsedDate.getTime());
    }, 'Invalid date format'),
  estimatedHours: z
    .number()
    .min(0, 'Estimated hours must be positive')
    .max(1000, 'Estimated hours must be less than 1000')
    .nullable()
    .optional(),
  actualHours: z
    .number()
    .min(0, 'Actual hours must be positive')
    .max(1000, 'Actual hours must be less than 1000')
    .nullable()
    .optional(),
  tags: z
    .array(z.string())
    .max(UI_CONFIG.MAX_TAGS, `Maximum ${UI_CONFIG.MAX_TAGS} tags allowed`)
    .optional(),
});

export type UpdateTaskFormData = z.infer<typeof updateTaskSchema>;

/**
 * Update task status validation schema
 * Requirement 3.3: Status change validation
 */
export const updateTaskStatusSchema = z.object({
  status: z.enum(['Pending', 'InProgress', 'Completed', 'Cancelled', 'Blocked'], {
    required_error: 'Status is required',
  }),
});

export type UpdateTaskStatusFormData = z.infer<typeof updateTaskStatusSchema>;

// ============== Comment Schema ==============

/**
 * Create comment validation schema
 * Requirement 5.1: Comment content required
 */
export const createCommentSchema = z.object({
  content: z
    .string()
    .min(1, 'Comment cannot be empty')
    .max(UI_CONFIG.MAX_COMMENT_LENGTH, `Comment must be less than ${UI_CONFIG.MAX_COMMENT_LENGTH} characters`),
});

export type CreateCommentFormData = z.infer<typeof createCommentSchema>;

// ============== User Profile Schema ==============

/**
 * Update user profile validation schema
 * Requirement 26.4: Profile update validation
 */
export const updateProfileSchema = z.object({
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(100, 'First name must be less than 100 characters')
    .optional(),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(100, 'Last name must be less than 100 characters')
    .optional(),
});

export type UpdateProfileFormData = z.infer<typeof updateProfileSchema>;

/**
 * Update user role validation schema (Admin only)
 * Requirement 2.1: Role assignment validation
 */
export const updateUserRoleSchema = z.object({
  role: z.enum(['SuperAdmin', 'Admin', 'Member'], {
    required_error: 'Role is required',
  }),
});

export type UpdateUserRoleFormData = z.infer<typeof updateUserRoleSchema>;

// ============== Task Filter Schema ==============

/**
 * Task filter validation schema
 * Requirement 4.1-4.7: Filter validation
 */
export const taskFilterSchema = z.object({
  status: z.array(z.enum(['Pending', 'InProgress', 'Completed', 'Cancelled', 'Blocked'])).optional(),
  priority: z.array(z.enum(['Critical', 'High', 'Medium', 'Low'])).optional(),
  category: z.array(z.enum(['Development', 'Design', 'Marketing', 'Operations', 'Research', 'Other'])).optional(),
  assignedToUserId: z.string().uuid('Invalid user ID').optional(),
  dueDateFrom: z
    .string()
    .optional()
    .refine((date) => {
      if (!date) return true;
      const parsedDate = new Date(date);
      return !isNaN(parsedDate.getTime());
    }, 'Invalid date format'),
  dueDateTo: z
    .string()
    .optional()
    .refine((date) => {
      if (!date) return true;
      const parsedDate = new Date(date);
      return !isNaN(parsedDate.getTime());
    }, 'Invalid date format'),
  searchTerm: z.string().optional(),
  page: z.number().min(1, 'Page must be at least 1').optional(),
  pageSize: z
    .number()
    .min(1, 'Page size must be at least 1')
    .max(UI_CONFIG.MAX_PAGE_SIZE, `Page size must be less than ${UI_CONFIG.MAX_PAGE_SIZE}`)
    .optional(),
});

export type TaskFilterFormData = z.infer<typeof taskFilterSchema>;

// ============== AI Schemas ==============

/**
 * Natural language task parsing validation schema
 * Requirement 8.1: Natural language input validation
 */
export const parseTaskSchema = z.object({
  input: z
    .string()
    .min(1, 'Please enter a task description')
    .max(2000, 'Input must be less than 2000 characters'),
});

export type ParseTaskFormData = z.infer<typeof parseTaskSchema>;

/**
 * Semantic search validation schema
 * Requirement 12.1: Semantic search query validation
 */
export const semanticSearchSchema = z.object({
  query: z
    .string()
    .min(1, 'Search query is required')
    .max(500, 'Search query must be less than 500 characters'),
  useSemanticSearch: z.boolean().default(false),
});

export type SemanticSearchFormData = z.infer<typeof semanticSearchSchema>;

/**
 * CSV import validation schema
 * Requirement 13.1: CSV import validation
 */
export const csvImportSchema = z.object({
  file: z
    .instanceof(File)
    .refine((file) => file.size <= UI_CONFIG.MAX_FILE_SIZE, 'File size must be less than 5MB')
    .refine(
      (file) => file.name.toLowerCase().endsWith('.csv'),
      'File must be a CSV file'
    ),
});

export type CsvImportFormData = z.infer<typeof csvImportSchema>;

// ============== Utility Validation Functions ==============

/**
 * Validate email format
 * 
 * @param email - Email string to validate
 * @returns True if valid email
 */
export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Validate UUID format
 * 
 * @param uuid - UUID string to validate
 * @returns True if valid UUID
 */
export function isValidUuid(uuid: string): boolean {
  const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
  return uuidRegex.test(uuid);
}

/**
 * Validate date string format
 * 
 * @param dateString - Date string to validate
 * @returns True if valid date
 */
export function isValidDate(dateString: string): boolean {
  const date = new Date(dateString);
  return !isNaN(date.getTime());
}

/**
 * Validate password strength
 * 
 * @param password - Password to validate
 * @returns Object with validation result and messages
 */
export function validatePasswordStrength(password: string): {
  isValid: boolean;
  messages: string[];
} {
  const messages: string[] = [];
  
  if (password.length < UI_CONFIG.MIN_PASSWORD_LENGTH) {
    messages.push(`Password must be at least ${UI_CONFIG.MIN_PASSWORD_LENGTH} characters`);
  }
  
  if (!/[A-Z]/.test(password)) {
    messages.push('Password must contain at least one uppercase letter');
  }
  
  if (!/[a-z]/.test(password)) {
    messages.push('Password must contain at least one lowercase letter');
  }
  
  if (!/[0-9]/.test(password)) {
    messages.push('Password must contain at least one number');
  }
  
  if (!/[^A-Za-z0-9]/.test(password)) {
    messages.push('Password must contain at least one special character');
  }
  
  return {
    isValid: messages.length === 0,
    messages,
  };
}

/**
 * Validate file size
 * 
 * @param file - File to validate
 * @param maxSize - Maximum file size in bytes (default: 5MB)
 * @returns True if file size is valid
 */
export function isValidFileSize(file: File, maxSize: number = UI_CONFIG.MAX_FILE_SIZE): boolean {
  return file.size <= maxSize;
}

/**
 * Validate file type
 * 
 * @param file - File to validate
 * @param allowedTypes - Array of allowed file extensions
 * @returns True if file type is valid
 */
export function isValidFileType(file: File, allowedTypes: readonly string[] = UI_CONFIG.ALLOWED_FILE_TYPES): boolean {
  return allowedTypes.some(type => file.name.toLowerCase().endsWith(type));
}

/**
 * Sanitize HTML to prevent XSS
 * 
 * @param html - HTML string to sanitize
 * @returns Sanitized HTML string
 */
export function sanitizeHtml(html: string): string {
  const div = document.createElement('div');
  div.textContent = html;
  return div.innerHTML;
}

/**
 * Validate URL format
 * 
 * @param url - URL string to validate
 * @returns True if valid URL
 */
export function isValidUrl(url: string): boolean {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
}
