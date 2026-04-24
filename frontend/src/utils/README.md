# Utilities Documentation

**Task:** 28.1 - Create utility functions  
**Requirements:** All frontend requirements

This directory contains utility functions, constants, formatters, validators, and helpers used throughout the Velocify frontend application.

## Files Overview

### 📋 constants.ts
Centralized constants for API URLs, enum mappings, and configuration values.

**Key Exports:**
- `API_CONFIG` - API configuration (base URL, timeout, retry settings)
- `API_ENDPOINTS` - All API endpoint paths organized by feature
- `SIGNALR_CONFIG` - SignalR hub configuration and event names
- `TASK_STATUS_LABELS` / `TASK_STATUS_COLORS` - Status display mappings
- `TASK_PRIORITY_LABELS` / `TASK_PRIORITY_COLORS` - Priority display mappings
- `TASK_CATEGORY_LABELS` / `TASK_CATEGORY_ICONS` - Category display mappings
- `USER_ROLE_LABELS` / `USER_ROLE_COLORS` - Role display mappings
- `NOTIFICATION_TYPE_LABELS` / `NOTIFICATION_TYPE_ICONS` - Notification type mappings
- `UI_CONFIG` - UI configuration (pagination, debounce, validation limits)
- `ROUTES` - Application route paths
- `STORAGE_KEYS` - Local storage key names
- `CHART_COLORS` - Chart color palette
- `SENTIMENT_CONFIG` - Sentiment analysis thresholds and colors
- `DATE_FORMATS` - Date format strings
- `ERROR_MESSAGES` / `SUCCESS_MESSAGES` - User-facing messages

**Usage Example:**
```typescript
import { API_ENDPOINTS, TASK_STATUS_COLORS, UI_CONFIG } from '@/utils/constants';

// Use API endpoint
const url = API_ENDPOINTS.TASKS.BY_ID(taskId);

// Get status color
const colorClass = TASK_STATUS_COLORS['InProgress']; // 'bg-blue-100 text-blue-800'

// Use UI config
const debounceMs = UI_CONFIG.SEARCH_DEBOUNCE_MS; // 300
```

---

### 🎨 formatters.ts
Functions for formatting dates, numbers, status values, and other display data.

**Key Exports:**

**Date Formatters:**
- `formatDate(date, format?)` - Format date to readable string
- `formatDateTime(date)` - Format date with time
- `formatRelativeDate(date)` - Format as relative time ("2 hours ago")
- `formatSmartDate(date)` - Smart format (Today, Yesterday, or date)
- `getDueDateColor(dueDate)` - Get Tailwind color class for due date
- `isOverdue(dueDate)` - Check if date is overdue

**Number Formatters:**
- `formatNumber(value)` - Format with commas
- `formatDecimal(value, decimals?)` - Format with decimal places
- `formatPercentage(value, asPercentage?)` - Format as percentage
- `formatHours(hours)` - Format hours with unit
- `formatProductivityScore(score)` - Format score with color

**Enum Formatters:**
- `formatTaskStatus(status)` - Format status to label
- `formatTaskPriority(priority)` - Format priority to label
- `formatTaskCategory(category)` - Format category to label
- `formatUserRole(role)` - Format role to label
- `formatNotificationType(type)` - Format notification type to label

**User Formatters:**
- `formatUserName(firstName, lastName)` - Format full name
- `getUserInitials(firstName, lastName)` - Get initials for avatar

**Other Formatters:**
- `formatSentiment(score)` - Format sentiment with icon and color
- `parseTags(tags)` - Parse tags string to array
- `formatTags(tags)` - Format tags array to string
- `formatFileSize(bytes)` - Format file size
- `truncateText(text, maxLength)` - Truncate with ellipsis
- `formatAiPriorityScore(score)` - Format AI priority score
- `formatCompletionProbability(probability)` - Format completion probability

**Usage Example:**
```typescript
import { formatDate, getDueDateColor, formatPercentage } from '@/utils/formatters';

// Format date
const displayDate = formatDate(task.dueDate); // "Dec 25, 2024"

// Get due date color
const colorClass = getDueDateColor(task.dueDate); // "text-red-600 font-semibold"

// Format percentage
const scoreText = formatPercentage(0.85); // "85%"
```

---

### ✅ validators.ts
Zod schemas for form validation across the application.

**Key Exports:**

**Auth Schemas:**
- `loginSchema` - Login form validation
- `registerSchema` - Registration form validation with password strength

**Task Schemas:**
- `createTaskSchema` - Create task form validation
- `updateTaskSchema` - Update task form validation
- `updateTaskStatusSchema` - Status change validation

**Comment Schema:**
- `createCommentSchema` - Comment content validation

**User Schemas:**
- `updateProfileSchema` - Profile update validation
- `updateUserRoleSchema` - Role assignment validation

**Filter Schema:**
- `taskFilterSchema` - Task filter validation

**AI Schemas:**
- `parseTaskSchema` - Natural language input validation
- `semanticSearchSchema` - Search query validation
- `csvImportSchema` - CSV file validation

**Utility Functions:**
- `isValidEmail(email)` - Validate email format
- `isValidUuid(uuid)` - Validate UUID format
- `isValidDate(dateString)` - Validate date string
- `validatePasswordStrength(password)` - Check password strength
- `isValidFileSize(file, maxSize?)` - Validate file size
- `isValidFileType(file, allowedTypes?)` - Validate file type
- `sanitizeHtml(html)` - Sanitize HTML to prevent XSS
- `isValidUrl(url)` - Validate URL format

**Usage Example:**
```typescript
import { createTaskSchema, type CreateTaskFormData } from '@/utils/validators';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

const form = useForm<CreateTaskFormData>({
  resolver: zodResolver(createTaskSchema),
  defaultValues: {
    title: '',
    priority: 'Medium',
    category: 'Development',
    tags: [],
  },
});
```

---

### 🛠️ helpers.ts
General utility functions for common operations.

**Key Exports:**

**Class Name Utilities:**
- `cn(...inputs)` - Merge Tailwind classes with conflict resolution

**Array Utilities:**
- `unique(array)` - Remove duplicates
- `groupBy(array, keyFn)` - Group items by key
- `sortBy(array, keyFn, order?)` - Sort by key
- `chunk(array, size)` - Split into chunks

**Object Utilities:**
- `deepClone(obj)` - Deep clone object
- `pick(obj, keys)` - Pick specific keys
- `omit(obj, keys)` - Omit specific keys
- `isEmpty(obj)` - Check if empty

**String Utilities:**
- `capitalize(str)` - Capitalize first letter
- `toKebabCase(str)` - Convert to kebab-case
- `toCamelCase(str)` - Convert to camelCase
- `randomString(length?)` - Generate random string

**URL Utilities:**
- `buildUrl(baseUrl, params)` - Build URL with query params
- `parseQueryString(queryString)` - Parse query string to object

**Local Storage Utilities:**
- `getStorageItem(key, defaultValue)` - Get with JSON parsing
- `setStorageItem(key, value)` - Set with JSON stringification
- `removeStorageItem(key)` - Remove item

**Debounce and Throttle:**
- `debounce(fn, delay)` - Debounce function
- `throttle(fn, limit)` - Throttle function

**Async Utilities:**
- `sleep(ms)` - Sleep for milliseconds
- `retry(fn, maxAttempts?, delay?)` - Retry with exponential backoff

**Color Utilities:**
- `stringToColor(str)` - Generate color from string
- `isLightColor(hexColor)` - Check if color is light

**Clipboard Utilities:**
- `copyToClipboard(text)` - Copy text to clipboard

**Download Utilities:**
- `downloadFile(data, filename, mimeType?)` - Download data as file

**Enum Utilities:**
- `getEnumValues(enumObj)` - Get all enum values
- `getTaskStatusOptions()` - Get status options for select
- `getTaskPriorityOptions()` - Get priority options for select
- `getTaskCategoryOptions()` - Get category options for select
- `getUserRoleOptions()` - Get role options for select

**Error Handling:**
- `getErrorMessage(error)` - Extract error message
- `isNetworkError(error)` - Check if network error

**Permission Utilities:**
- `isAdmin(role)` - Check if admin
- `isSuperAdmin(role)` - Check if super admin
- `canEditTask(userRole, taskCreatorId, currentUserId)` - Check edit permission
- `canDeleteComment(userRole, commentUserId, currentUserId)` - Check delete permission

**Usage Example:**
```typescript
import { cn, debounce, getTaskStatusOptions, isAdmin } from '@/utils/helpers';

// Merge classes
const className = cn('base-class', isActive && 'active-class', 'text-blue-500');

// Debounce search
const debouncedSearch = debounce((query: string) => {
  searchTasks(query);
}, 300);

// Get options for select
const statusOptions = getTaskStatusOptions();

// Check permissions
if (isAdmin(user.role)) {
  // Show admin features
}
```

---

## Import Patterns

### Named Imports (Recommended)
```typescript
import { formatDate, API_ENDPOINTS, createTaskSchema } from '@/utils';
```

### Category Imports
```typescript
import { formatDate, formatPercentage } from '@/utils/formatters';
import { API_ENDPOINTS, ROUTES } from '@/utils/constants';
import { createTaskSchema, loginSchema } from '@/utils/validators';
import { cn, debounce, isAdmin } from '@/utils/helpers';
```

### Wildcard Imports (Not Recommended)
```typescript
import * as utils from '@/utils';
// Use: utils.formatDate(...)
```

---

## Common Use Cases

### 1. Task Card Display
```typescript
import { formatDate, getDueDateColor, formatPercentage } from '@/utils';

function TaskCard({ task }) {
  return (
    <div>
      <h3>{task.title}</h3>
      <span className={getDueDateColor(task.dueDate)}>
        {formatDate(task.dueDate)}
      </span>
      {task.aiPriorityScore && (
        <span>{formatPercentage(task.aiPriorityScore)}</span>
      )}
    </div>
  );
}
```

### 2. Form Validation
```typescript
import { createTaskSchema, type CreateTaskFormData } from '@/utils';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

function TaskForm() {
  const form = useForm<CreateTaskFormData>({
    resolver: zodResolver(createTaskSchema),
  });
  
  // Form implementation
}
```

### 3. API Calls
```typescript
import { API_ENDPOINTS } from '@/utils';
import axiosInstance from '@/api/axios';

async function getTask(id: string) {
  const response = await axiosInstance.get(API_ENDPOINTS.TASKS.BY_ID(id));
  return response.data;
}
```

### 4. Permission Checks
```typescript
import { isAdmin, canEditTask } from '@/utils';

function TaskActions({ task, currentUser }) {
  const canEdit = canEditTask(
    currentUser.role,
    task.createdBy.id,
    currentUser.id
  );
  
  return (
    <>
      {canEdit && <EditButton />}
      {isAdmin(currentUser.role) && <AdminActions />}
    </>
  );
}
```

### 5. Debounced Search
```typescript
import { debounce, UI_CONFIG } from '@/utils';
import { useState, useCallback } from 'react';

function SearchBar() {
  const [query, setQuery] = useState('');
  
  const debouncedSearch = useCallback(
    debounce((value: string) => {
      // Perform search
    }, UI_CONFIG.SEARCH_DEBOUNCE_MS),
    []
  );
  
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setQuery(e.target.value);
    debouncedSearch(e.target.value);
  };
  
  return <input value={query} onChange={handleChange} />;
}
```

---

## Testing

All utility functions are pure functions (except for side-effect utilities like localStorage and clipboard) and can be easily unit tested.

Example test structure:
```typescript
import { formatDate, isValidEmail, cn } from '@/utils';

describe('formatters', () => {
  it('should format date correctly', () => {
    expect(formatDate('2024-12-25')).toBe('Dec 25, 2024');
  });
});

describe('validators', () => {
  it('should validate email', () => {
    expect(isValidEmail('test@example.com')).toBe(true);
    expect(isValidEmail('invalid')).toBe(false);
  });
});

describe('helpers', () => {
  it('should merge classes', () => {
    expect(cn('base', 'extra')).toBe('base extra');
  });
});
```

---

## Best Practices

1. **Use constants instead of magic strings/numbers**
   ```typescript
   // ❌ Bad
   const pageSize = 20;
   
   // ✅ Good
   import { UI_CONFIG } from '@/utils';
   const pageSize = UI_CONFIG.DEFAULT_PAGE_SIZE;
   ```

2. **Use formatters for consistent display**
   ```typescript
   // ❌ Bad
   <span>{new Date(task.dueDate).toLocaleDateString()}</span>
   
   // ✅ Good
   import { formatDate } from '@/utils';
   <span>{formatDate(task.dueDate)}</span>
   ```

3. **Use validators for all forms**
   ```typescript
   // ❌ Bad
   const isValid = email.includes('@');
   
   // ✅ Good
   import { isValidEmail } from '@/utils';
   const isValid = isValidEmail(email);
   ```

4. **Use helpers for common operations**
   ```typescript
   // ❌ Bad
   const className = `base ${isActive ? 'active' : ''} text-blue-500`;
   
   // ✅ Good
   import { cn } from '@/utils';
   const className = cn('base', isActive && 'active', 'text-blue-500');
   ```

---

## Dependencies

- `zod` - Schema validation
- `date-fns` - Date manipulation
- `clsx` - Class name utility
- `tailwind-merge` - Tailwind class merging

All dependencies are already installed in the project.
