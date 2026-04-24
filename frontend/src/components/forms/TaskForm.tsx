/**
 * Task Form Component
 * 
 * Requirements: 24.1-24.5
 * 
 * Manual form mode with all task fields and validation
 */

import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Input } from '../ui/Input';
import { Select } from '../ui/Select';
import { DatePicker } from '../ui/DatePicker';
import { Button } from '../ui/Button';
import { UserSearchDropdown } from './UserSearchDropdown';
import { TagInput } from './TagInput';
import { taskFormSchema, type TaskFormData } from './taskFormSchema';
import type { UserSummaryDto } from '../../api/types';

export interface TaskFormProps {
  initialValues?: Partial<TaskFormData>;
  users: UserSummaryDto[];
  onSubmit: (data: TaskFormData) => Promise<void>;
  onCancel: () => void;
  isSubmitting?: boolean;
  submitLabel?: string;
}

/**
 * TaskForm component
 * 
 * Requirement 24.1: Manual Form mode with all fields
 * Requirement 24.3: Form validation with React Hook Form + Zod
 * Requirement 24.4: Field-level error messages
 * Requirement 24.5: Controlled inputs with React Hook Form
 * 
 * @example
 * ```tsx
 * <TaskForm
 *   initialValues={taskData}
 *   users={availableUsers}
 *   onSubmit={handleSubmit}
 *   onCancel={handleCancel}
 *   isSubmitting={isLoading}
 *   submitLabel="Update Task"
 * />
 * ```
 */
export const TaskForm: React.FC<TaskFormProps> = ({
  initialValues,
  users,
  onSubmit,
  onCancel,
  isSubmitting = false,
  submitLabel = 'Create Task',
}) => {
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<TaskFormData>({
    resolver: zodResolver(taskFormSchema),
    defaultValues: initialValues || {
      title: '',
      description: '',
      priority: 'Medium',
      category: 'Development',
      assignedToUserId: null,
      dueDate: null,
      estimatedHours: null,
      tags: [],
    },
  });

  // Update form when initialValues change (for AI parsing)
  useEffect(() => {
    if (initialValues) {
      Object.entries(initialValues).forEach(([key, value]) => {
        setValue(key as keyof TaskFormData, value);
      });
    }
  }, [initialValues, setValue]);

  const handleFormSubmit = async (data: TaskFormData) => {
    try {
      await onSubmit(data);
    } catch (error) {
      // Error handling is done by parent component
    }
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
      {/* Title */}
      <Input
        {...register('title')}
        label="Title"
        placeholder="Enter task title"
        error={errors.title?.message}
        fullWidth
        disabled={isSubmitting}
        required
      />

      {/* Description */}
      <div className="flex flex-col gap-1">
        <label htmlFor="description" className="text-sm font-medium text-neutral-700">
          Description <span className="text-danger-600">*</span>
        </label>
        <textarea
          {...register('description')}
          id="description"
          placeholder="Enter task description"
          rows={4}
          disabled={isSubmitting}
          className={`px-3 py-2 border rounded-md text-base transition-colors resize-y focus:outline-none focus:ring-2 focus:ring-offset-1 disabled:bg-neutral-100 disabled:cursor-not-allowed ${
            errors.description
              ? 'border-danger-500 focus:ring-danger-500'
              : 'border-neutral-300 focus:ring-primary-500'
          }`}
        />
        {errors.description && (
          <span className="text-sm text-danger-600" role="alert">
            {errors.description.message}
          </span>
        )}
      </div>

      {/* Priority and Category - Side by side */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Controller
          name="priority"
          control={control}
          render={({ field }) => (
            <Select
              {...field}
              label="Priority"
              options={[
                { value: 'Critical', label: 'Critical' },
                { value: 'High', label: 'High' },
                { value: 'Medium', label: 'Medium' },
                { value: 'Low', label: 'Low' },
              ]}
              error={errors.priority?.message}
              fullWidth
              disabled={isSubmitting}
              required
            />
          )}
        />

        <Controller
          name="category"
          control={control}
          render={({ field }) => (
            <Select
              {...field}
              label="Category"
              options={[
                { value: 'Development', label: 'Development' },
                { value: 'Design', label: 'Design' },
                { value: 'Marketing', label: 'Marketing' },
                { value: 'Operations', label: 'Operations' },
                { value: 'Research', label: 'Research' },
                { value: 'Other', label: 'Other' },
              ]}
              error={errors.category?.message}
              fullWidth
              disabled={isSubmitting}
              required
            />
          )}
        />
      </div>

      {/* Assignee */}
      <Controller
        name="assignedToUserId"
        control={control}
        render={({ field }) => (
          <UserSearchDropdown
            name="assignedToUserId"
            label="Assign To"
            users={users}
            value={field.value}
            onChange={field.onChange}
            error={errors.assignedToUserId?.message}
            placeholder="Search users..."
            fullWidth
            disabled={isSubmitting}
            allowClear
          />
        )}
      />

      {/* Due Date and Estimated Hours - Side by side */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Controller
          name="dueDate"
          control={control}
          render={({ field }) => (
            <DatePicker
              {...field}
              value={field.value || ''}
              label="Due Date"
              error={errors.dueDate?.message}
              fullWidth
              disabled={isSubmitting}
            />
          )}
        />

        <Controller
          name="estimatedHours"
          control={control}
          render={({ field }) => (
            <Input
              {...field}
              type="number"
              value={field.value ?? ''}
              onChange={(e) => {
                const value = e.target.value;
                field.onChange(value === '' ? null : parseFloat(value));
              }}
              label="Estimated Hours"
              placeholder="0"
              min="0"
              max="1000"
              step="0.5"
              error={errors.estimatedHours?.message}
              fullWidth
              disabled={isSubmitting}
            />
          )}
        />
      </div>

      {/* Tags */}
      <Controller
        name="tags"
        control={control}
        render={({ field }) => (
          <TagInput
            name="tags"
            label="Tags"
            value={field.value}
            onChange={field.onChange}
            error={errors.tags?.message}
            placeholder="Type and press Enter..."
            fullWidth
            disabled={isSubmitting}
            maxTags={10}
          />
        )}
      />

      {/* Form Actions */}
      <div className="flex gap-3 pt-4">
        <Button
          type="submit"
          variant="primary"
          isLoading={isSubmitting}
          disabled={isSubmitting}
        >
          {submitLabel}
        </Button>
        <Button
          type="button"
          variant="secondary"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancel
        </Button>
      </div>
    </form>
  );
};

TaskForm.displayName = 'TaskForm';
