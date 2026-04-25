# Utility Usage Examples

**Task:** 28.1 - Create utility functions  
**Requirements:** All frontend requirements

This document provides practical examples of how to use the utility functions in real components.

## Example 1: Refactoring TaskCard Component

### Before (without utilities)
```typescript
import { clsx } from 'clsx';
import { format } from 'date-fns';

export function TaskCard({ task, getDueDateColor }) {
  return (
    <div>
      {/* Due Date */}
      {task.dueDate && (
        <div className={clsx('flex items-center gap-1', getDueDateColor(task.dueDate))}>
          <span>📅</span>
          <span>{format(new Date(task.dueDate), 'MMM dd, yyyy')}</span>
        </div>
      )}

      {/* AI Priority Score */}
      {task.aiPriorityScore !== null && (
        <div className="flex items-center gap-1 text-primary-600">
          <span>🤖</span>
          <span>{Math.round(task.aiPriorityScore * 100)}%</span>
        </div>
      )}

      {/* Tags */}
      {task.tags && (
        <div className="flex gap-2 mt-3 flex-wrap">
          {task.tags.split(',').filter(Boolean).map((tag, idx) => (
            <Badge key={idx}>{tag.trim()}</Badge>
          ))}
        </div>
      )}
    </div>
  );
}
```

### After (with utilities)
```typescript
import { formatDate, getDueDateColor, formatPercentage, parseTags } from '@/utils';

export function TaskCard({ task }) {
  return (
    <div>
      {/* Due Date */}
      {task.dueDate && (
        <div className={getDueDateColor(task.dueDate)}>
          <span>📅</span>
          <span>{formatDate(task.dueDate)}</span>
        </div>
      )}

      {/* AI Priority Score */}
      {task.aiPriorityScore !== null && (
        <div className="text-primary-600">
          <span>🤖</span>
          <span>{formatPercentage(task.aiPriorityScore)}</span>
        </div>
      )}

      {/* Tags */}
      {task.tags && (
        <div className="flex gap-2 mt-3 flex-wrap">
          {parseTags(task.tags).map((tag, idx) => (
            <Badge key={idx}>{tag}</Badge>
          ))}
        </div>
      )}
    </div>
  );
}
```

**Benefits:**
- Consistent date formatting across the app
- Centralized due date color logic
- Reusable percentage formatting
- Cleaner tag parsing

---

## Example 2: Task Form with Validation

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  createTaskSchema,
  type CreateTaskFormData,
  getTaskPriorityOptions,
  getTaskCategoryOptions,
  UI_CONFIG,
} from '@/utils';

export function TaskForm() {
  const form = useForm<CreateTaskFormData>({
    resolver: zodResolver(createTaskSchema),
    defaultValues: {
      title: '',
      description: '',
      priority: 'Medium',
      category: 'Development',
      tags: [],
    },
  });

  const priorityOptions = getTaskPriorityOptions();
  const categoryOptions = getTaskCategoryOptions();

  const onSubmit = async (data: CreateTaskFormData) => {
    try {
      await createTask(data);
      toast.success(SUCCESS_MESSAGES.TASK_CREATED);
    } catch (error) {
      toast.error(getErrorMessage(error));
    }
  };

  return (
    <form onSubmit={form.handleSubmit(onSubmit)}>
      <input
        {...form.register('title')}
        maxLength={UI_CONFIG.MAX_TITLE_LENGTH}
        placeholder="Task title"
      />
      {form.formState.errors.title && (
        <span className="text-red-600">
          {form.formState.errors.title.message}
        </span>
      )}

      <select {...form.register('priority')}>
        {priorityOptions.map(option => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>

      <select {...form.register('category')}>
        {categoryOptions.map(option => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>

      <button type="submit">Create Task</button>
    </form>
  );
}
```

**Benefits:**
- Type-safe form data with Zod
- Automatic validation
- Consistent error messages
- Reusable select options

---

## Example 3: Dashboard with Formatters

```typescript
import {
  formatNumber,
  formatProductivityScore,
  formatDate,
  CHART_COLORS,
} from '@/utils';
import { useDashboard } from '@/hooks';

export function DashboardPage() {
  const { summary, velocity, productivity } = useDashboard();

  const scoreData = formatProductivityScore(productivity.currentScore);

  return (
    <div>
      {/* Summary Cards */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <h3>Pending</h3>
          <p className="text-3xl font-bold">
            {formatNumber(summary.pendingCount)}
          </p>
        </Card>
        <Card>
          <h3>In Progress</h3>
          <p className="text-3xl font-bold">
            {formatNumber(summary.inProgressCount)}
          </p>
        </Card>
        <Card>
          <h3>Completed</h3>
          <p className="text-3xl font-bold">
            {formatNumber(summary.completedCount)}
          </p>
        </Card>
        <Card>
          <h3>Overdue</h3>
          <p className="text-3xl font-bold text-red-600">
            {formatNumber(summary.overdueCount)}
          </p>
        </Card>
      </div>

      {/* Productivity Score */}
      <Card>
        <h3>Productivity Score</h3>
        <p className={`text-4xl font-bold ${scoreData.color}`}>
          {scoreData.text}
        </p>
      </Card>

      {/* Velocity Chart */}
      <Card>
        <h3>Task Completion Velocity</h3>
        <LineChart data={velocity}>
          <Line
            dataKey="completedCount"
            stroke={CHART_COLORS.PRIMARY}
            strokeWidth={2}
          />
          <XAxis
            dataKey="date"
            tickFormatter={(date) => formatDate(date, 'MMM dd')}
          />
        </LineChart>
      </Card>
    </div>
  );
}
```

**Benefits:**
- Consistent number formatting
- Dynamic color based on score
- Centralized chart colors
- Flexible date formatting

---

## Example 4: Search with Debounce

```typescript
import { useState, useCallback } from 'react';
import { debounce, UI_CONFIG } from '@/utils';
import { useTasks } from '@/hooks';

export function TaskSearch() {
  const [query, setQuery] = useState('');
  const { searchTasks } = useTasks();

  // Debounce search to avoid excessive API calls
  const debouncedSearch = useCallback(
    debounce((searchQuery: string) => {
      searchTasks(searchQuery);
    }, UI_CONFIG.SEARCH_DEBOUNCE_MS),
    []
  );

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setQuery(value);
    debouncedSearch(value);
  };

  return (
    <input
      type="text"
      value={query}
      onChange={handleChange}
      placeholder="Search tasks..."
      className="w-full px-4 py-2 border rounded-lg"
    />
  );
}
```

**Benefits:**
- Prevents excessive API calls
- Configurable debounce delay
- Clean implementation

---

## Example 5: Permission-Based UI

```typescript
import { isAdmin, canEditTask, canDeleteComment } from '@/utils';
import { useAuthStore } from '@/store';

export function TaskActions({ task }) {
  const currentUser = useAuthStore(state => state.user);

  const canEdit = canEditTask(
    currentUser.role,
    task.createdBy.id,
    currentUser.id
  );

  return (
    <div className="flex gap-2">
      {canEdit && (
        <button onClick={() => editTask(task.id)}>
          Edit
        </button>
      )}

      {isAdmin(currentUser.role) && (
        <>
          <button onClick={() => reassignTask(task.id)}>
            Reassign
          </button>
          <button onClick={() => deleteTask(task.id)}>
            Delete
          </button>
        </>
      )}
    </div>
  );
}

export function CommentItem({ comment }) {
  const currentUser = useAuthStore(state => state.user);

  const canDelete = canDeleteComment(
    currentUser.role,
    comment.user.id,
    currentUser.id
  );

  return (
    <div className="comment">
      <p>{comment.content}</p>
      {canDelete && (
        <button onClick={() => deleteComment(comment.id)}>
          Delete
        </button>
      )}
    </div>
  );
}
```

**Benefits:**
- Centralized permission logic
- Consistent across components
- Easy to test

---

## Example 6: API Service with Constants

```typescript
import axiosInstance from './axios';
import { API_ENDPOINTS } from '@/utils';
import type { TaskDto, CreateTaskRequest } from './types';

export async function getTasks(filters: TaskFilters): Promise<PagedResult<TaskDto>> {
  const response = await axiosInstance.get(API_ENDPOINTS.TASKS.LIST, {
    params: filters,
  });
  return response.data;
}

export async function getTaskById(id: string): Promise<TaskDto> {
  const response = await axiosInstance.get(API_ENDPOINTS.TASKS.BY_ID(id));
  return response.data;
}

export async function createTask(data: CreateTaskRequest): Promise<TaskDto> {
  const response = await axiosInstance.post(API_ENDPOINTS.TASKS.LIST, data);
  return response.data;
}

export async function updateTaskStatus(
  id: string,
  status: TaskStatus
): Promise<TaskDto> {
  const response = await axiosInstance.patch(
    API_ENDPOINTS.TASKS.STATUS(id),
    { status }
  );
  return response.data;
}
```

**Benefits:**
- No hardcoded URLs
- Type-safe endpoints
- Easy to update

---

## Example 7: Error Handling

```typescript
import { getErrorMessage, isNetworkError, ERROR_MESSAGES } from '@/utils';
import { useToast } from '@/hooks';

export function TaskList() {
  const toast = useToast();

  const handleError = (error: unknown) => {
    if (isNetworkError(error)) {
      toast.error(ERROR_MESSAGES.NETWORK_ERROR);
    } else {
      toast.error(getErrorMessage(error));
    }
  };

  const loadTasks = async () => {
    try {
      const tasks = await getTasks();
      setTasks(tasks);
    } catch (error) {
      handleError(error);
    }
  };

  return (
    <div>
      {/* Task list UI */}
    </div>
  );
}
```

**Benefits:**
- Consistent error messages
- Network error detection
- User-friendly messages

---

## Example 8: Sentiment Display

```typescript
import { formatSentiment } from '@/utils';

export function CommentItem({ comment }) {
  const sentiment = formatSentiment(comment.sentimentScore);

  return (
    <div className="comment">
      <div className="flex items-center gap-2">
        <span className={sentiment.color}>
          {sentiment.icon}
        </span>
        <span className="text-sm text-gray-500">
          {sentiment.label}
        </span>
      </div>
      <p>{comment.content}</p>
    </div>
  );
}
```

**Benefits:**
- Consistent sentiment display
- Icon and color mapping
- Easy to understand

---

## Example 9: File Upload with Validation

```typescript
import { isValidFileSize, isValidFileType, formatFileSize, UI_CONFIG } from '@/utils';
import { useToast } from '@/hooks';

export function CsvImportForm() {
  const toast = useToast();

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file size
    if (!isValidFileSize(file)) {
      toast.error(
        `File size must be less than ${formatFileSize(UI_CONFIG.MAX_FILE_SIZE)}`
      );
      return;
    }

    // Validate file type
    if (!isValidFileType(file)) {
      toast.error('Only CSV files are allowed');
      return;
    }

    // Process file
    uploadFile(file);
  };

  return (
    <input
      type="file"
      accept=".csv"
      onChange={handleFileChange}
    />
  );
}
```

**Benefits:**
- Consistent validation
- User-friendly error messages
- Configurable limits

---

## Example 10: Export Functionality

```typescript
import { downloadFile, formatDate } from '@/utils';

export function TaskExport({ tasks }) {
  const handleExport = () => {
    // Convert tasks to CSV
    const headers = ['Title', 'Status', 'Priority', 'Due Date'];
    const rows = tasks.map(task => [
      task.title,
      task.status,
      task.priority,
      formatDate(task.dueDate),
    ]);

    const csv = [
      headers.join(','),
      ...rows.map(row => row.join(',')),
    ].join('\n');

    // Download file
    const filename = `tasks-export-${formatDate(new Date(), 'yyyy-MM-dd')}.csv`;
    downloadFile(csv, filename, 'text/csv');
  };

  return (
    <button onClick={handleExport}>
      Export Tasks
    </button>
  );
}
```

**Benefits:**
- Easy file download
- Consistent date formatting
- Clean implementation

---

## Best Practices Summary

1. **Always use utilities for:**
   - Date formatting
   - Number formatting
   - Enum display
   - Form validation
   - Permission checks
   - API endpoints
   - Error handling

2. **Import only what you need:**
   ```typescript
   // ✅ Good
   import { formatDate, getDueDateColor } from '@/utils';
   
   // ❌ Avoid
   import * as utils from '@/utils';
   ```

3. **Use constants instead of magic values:**
   ```typescript
   // ✅ Good
   import { UI_CONFIG } from '@/utils';
   const debounceMs = UI_CONFIG.SEARCH_DEBOUNCE_MS;
   
   // ❌ Avoid
   const debounceMs = 300;
   ```

4. **Leverage TypeScript types:**
   ```typescript
   import { createTaskSchema, type CreateTaskFormData } from '@/utils';
   
   const form = useForm<CreateTaskFormData>({
     resolver: zodResolver(createTaskSchema),
   });
   ```

5. **Keep components clean:**
   - Move formatting logic to utilities
   - Move validation logic to schemas
   - Move constants to constants file
   - Move helper functions to helpers file
