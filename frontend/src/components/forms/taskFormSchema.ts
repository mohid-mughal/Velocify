/**
 * Task Form Validation Schema
 * 
 * Requirements: 24.3, 24.4
 * 
 * Zod schema for task form validation
 * Validates all task fields before submission
 */

import { z } from 'zod';

/**
 * Zod schema for task form validation
 * 
 * Requirement 24.3: Form validation with Zod schema
 * Requirement 24.4: Field-level error messages
 */
export const taskFormSchema = z.object({
  title: z
    .string()
    .min(1, 'Title is required')
    .max(200, 'Title must be 200 characters or less'),
  
  description: z
    .string()
    .min(1, 'Description is required')
    .max(5000, 'Description must be 5000 characters or less'),
  
  priority: z.enum(['Critical', 'High', 'Medium', 'Low'] as const, {
    errorMap: () => ({ message: 'Please select a priority' }),
  }),
  
  category: z.enum(
    ['Development', 'Design', 'Marketing', 'Operations', 'Research', 'Other'] as const,
    {
      errorMap: () => ({ message: 'Please select a category' }),
    }
  ),
  
  assignedToUserId: z.string().nullable(),
  
  dueDate: z.string().nullable(),
  
  estimatedHours: z
    .number()
    .min(0, 'Estimated hours must be 0 or greater')
    .max(1000, 'Estimated hours must be 1000 or less')
    .nullable(),
  
  tags: z.array(z.string()).default([]),
});

/**
 * TypeScript type inferred from Zod schema
 */
export type TaskFormData = z.infer<typeof taskFormSchema>;

/**
 * Default values for new task form
 */
export const defaultTaskFormValues: TaskFormData = {
  title: '',
  description: '',
  priority: 'Medium',
  category: 'Development',
  assignedToUserId: null,
  dueDate: null,
  estimatedHours: null,
  tags: [],
};
