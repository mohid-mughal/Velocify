import React from 'react';
import { useForm, Controller } from 'react-hook-form';
import { FormField, MultiSelect, TagInput, UserSearchDropdown } from './index';
import { Button } from '../ui/Button';

/**
 * Example component demonstrating usage of all form components
 * This file shows integration with React Hook Form
 */

interface TaskFormData {
  title: string;
  description: string;
  categories: string[];
  tags: string[];
  assignedTo: string | null;
}

const categoryOptions = [
  { value: 'development', label: 'Development' },
  { value: 'design', label: 'Design' },
  { value: 'marketing', label: 'Marketing' },
  { value: 'operations', label: 'Operations' },
  { value: 'research', label: 'Research' },
];

const mockUsers = [
  { id: '1', firstName: 'John', lastName: 'Doe', email: 'john.doe@example.com', role: 'Admin' },
  { id: '2', firstName: 'Jane', lastName: 'Smith', email: 'jane.smith@example.com', role: 'Member' },
  { id: '3', firstName: 'Bob', lastName: 'Johnson', email: 'bob.johnson@example.com', role: 'Member' },
  { id: '4', firstName: 'Alice', lastName: 'Williams', email: 'alice.williams@example.com', role: 'Admin' },
];

export const FormComponentsExample: React.FC = () => {
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<TaskFormData>({
    defaultValues: {
      title: '',
      description: '',
      categories: [],
      tags: [],
      assignedTo: null,
    },
  });

  const onSubmit = (data: TaskFormData) => {
    console.log('Form submitted:', data);
    alert('Form submitted! Check console for data.');
  };

  return (
    <div className="max-w-2xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">Form Components Example</h1>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* FormField Example */}
        <FormField
          label="Task Title"
          placeholder="Enter task title..."
          error={errors.title?.message}
          required
          {...register('title', {
            required: 'Title is required',
            minLength: { value: 3, message: 'Title must be at least 3 characters' },
          })}
        />

        <FormField
          label="Description"
          placeholder="Enter task description..."
          error={errors.description?.message}
          helperText="Provide a detailed description of the task"
          {...register('description')}
        />

        {/* MultiSelect Example */}
        <Controller
          name="categories"
          control={control}
          rules={{ validate: (value) => value.length > 0 || 'Select at least one category' }}
          render={({ field }) => (
            <MultiSelect
              name="categories"
              label="Categories"
              options={categoryOptions}
              value={field.value}
              onChange={field.onChange}
              error={errors.categories?.message}
              placeholder="Select categories..."
              helperText="Choose one or more categories for this task"
              fullWidth
            />
          )}
        />

        {/* TagInput Example */}
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
              placeholder="Type and press Enter to add tags..."
              helperText="Add relevant tags to help organize and search for this task"
              maxTags={10}
              fullWidth
            />
          )}
        />

        {/* UserSearchDropdown Example */}
        <Controller
          name="assignedTo"
          control={control}
          rules={{ required: 'Please assign this task to a user' }}
          render={({ field }) => (
            <UserSearchDropdown
              name="assignedTo"
              label="Assign To"
              users={mockUsers}
              value={field.value}
              onChange={field.onChange}
              error={errors.assignedTo?.message}
              placeholder="Search for a user..."
              helperText="Select a team member to assign this task"
              allowClear
              fullWidth
            />
          )}
        />

        <div className="flex gap-3 pt-4">
          <Button type="submit" variant="primary">
            Submit Task
          </Button>
          <Button type="button" variant="secondary" onClick={() => console.log('Cancel clicked')}>
            Cancel
          </Button>
        </div>
      </form>

      <div className="mt-8 p-4 bg-neutral-100 rounded-md">
        <h2 className="text-lg font-semibold mb-2">Component Features:</h2>
        <ul className="list-disc list-inside space-y-1 text-sm">
          <li><strong>FormField:</strong> Wraps Input with label and error display</li>
          <li><strong>MultiSelect:</strong> Multi-selection dropdown with badge display</li>
          <li><strong>TagInput:</strong> Chip-based input (Enter or comma to add)</li>
          <li><strong>UserSearchDropdown:</strong> Searchable user selector with avatars</li>
        </ul>
      </div>
    </div>
  );
};
