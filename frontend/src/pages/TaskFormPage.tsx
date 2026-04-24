/**
 * Task Form Page
 * 
 * Requirements: 24.1-24.5
 * 
 * Page for creating and editing tasks with two modes:
 * - Natural Language mode: AI-powered task parsing
 * - Manual Form mode: Traditional form fields
 */

import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { MainLayout } from '../components/layout/MainLayout';
import { NaturalLanguageInput } from '../components/forms/NaturalLanguageInput';
import { TaskForm } from '../components/forms/TaskForm';
import { aiService } from '../api/ai.service';
import { tasksService } from '../api/tasks.service';
import { usersService } from '../api/users.service';
import { queryKeys } from '../api/queryKeys';
import type { TaskFormData } from '../components/forms/taskFormSchema';
import type { CreateTaskRequest } from '../api/types';

type FormMode = 'natural' | 'manual';

/**
 * TaskFormPage component
 * 
 * Requirement 24.1: Two modes - Natural Language and Manual Form
 * Requirement 24.2: AI parses natural language and populates form
 * Requirement 24.3: Form validation with React Hook Form + Zod
 * Requirement 24.4: Field-level error messages
 * Requirement 24.5: Controlled inputs with React Hook Form
 */
export default function TaskFormPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const isEditMode = Boolean(id);

  const [mode, setMode] = useState<FormMode>('natural');
  const [parsedData, setParsedData] = useState<Partial<TaskFormData> | null>(null);
  const [parseError, setParseError] = useState<string | null>(null);

  // Fetch users for assignee dropdown
  const { data: usersData } = useQuery({
    queryKey: queryKeys.users.list(),
    queryFn: () => usersService.getUsers({ page: 1, pageSize: 100 }),
  });

  // Fetch existing task for edit mode
  const { data: existingTask, isLoading: isLoadingTask } = useQuery({
    queryKey: queryKeys.tasks.detail(id!),
    queryFn: () => tasksService.getTaskById(id!),
    enabled: isEditMode,
  });

  // Create task mutation
  const createMutation = useMutation({
    mutationFn: (data: CreateTaskRequest) => tasksService.createTask(data),
    onSuccess: (task) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.lists() });
      navigate(`/tasks/${task.id}`);
    },
  });

  // Update task mutation
  const updateMutation = useMutation({
    mutationFn: (data: TaskFormData) =>
      tasksService.updateTask(id!, {
        title: data.title,
        description: data.description,
        priority: data.priority,
        category: data.category,
        assignedToUserId: data.assignedToUserId,
        dueDate: data.dueDate,
        estimatedHours: data.estimatedHours,
        tags: data.tags,
      }),
    onSuccess: (task) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.detail(id!) });
      queryClient.invalidateQueries({ queryKey: queryKeys.tasks.lists() });
      navigate(`/tasks/${task.id}`);
    },
  });

  // Parse natural language input
  const handleParse = async (input: string) => {
    setParseError(null);
    try {
      const parsed = await aiService.parseTask(input);
      
      // Convert CreateTaskRequest to TaskFormData format
      const formData: Partial<TaskFormData> = {
        title: parsed.title || '',
        description: parsed.description || '',
        priority: parsed.priority || 'Medium',
        category: parsed.category || 'Development',
        assignedToUserId: parsed.assignedToUserId || null,
        dueDate: parsed.dueDate || null,
        estimatedHours: parsed.estimatedHours || null,
        tags: parsed.tags || [],
      };

      setParsedData(formData);
      setMode('manual'); // Switch to manual mode to show parsed fields
    } catch (error) {
      setParseError(
        error instanceof Error
          ? error.message
          : 'Failed to parse task. Please try again or use manual mode.'
      );
    }
  };

  // Submit form
  const handleSubmit = async (data: TaskFormData) => {
    if (isEditMode) {
      await updateMutation.mutateAsync(data);
    } else {
      const createRequest: CreateTaskRequest = {
        title: data.title,
        description: data.description,
        priority: data.priority,
        category: data.category,
        assignedToUserId: data.assignedToUserId,
        dueDate: data.dueDate,
        estimatedHours: data.estimatedHours,
        tags: data.tags,
        parentTaskId: null,
      };
      await createMutation.mutateAsync(createRequest);
    }
  };

  // Cancel and go back
  const handleCancel = () => {
    navigate(-1);
  };

  // Prepare initial values for edit mode
  const initialValues: Partial<TaskFormData> | undefined = existingTask
    ? {
        title: existingTask.title,
        description: existingTask.description,
        priority: existingTask.priority,
        category: existingTask.category,
        assignedToUserId: existingTask.assignedTo?.id || null,
        dueDate: existingTask.dueDate || null,
        estimatedHours: existingTask.estimatedHours || null,
        tags: existingTask.tags ? existingTask.tags.split(',').filter(Boolean) : [],
      }
    : parsedData || undefined;

  // In edit mode, always use manual mode
  useEffect(() => {
    if (isEditMode) {
      setMode('manual');
    }
  }, [isEditMode]);

  if (isEditMode && isLoadingTask) {
    return (
      <MainLayout>
        <div className="max-w-4xl mx-auto py-8 px-4">
          <div className="text-center">Loading task...</div>
        </div>
      </MainLayout>
    );
  }

  const users = usersData?.items || [];
  const isSubmitting = createMutation.isPending || updateMutation.isPending;

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto py-8 px-4">
        {/* Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-neutral-900">
            {isEditMode ? 'Edit Task' : 'Create New Task'}
          </h1>
          <p className="text-neutral-600 mt-2">
            {isEditMode
              ? 'Update task details below'
              : 'Create a task using natural language or fill out the form manually'}
          </p>
        </div>

        {/* Mode Toggle (only for create mode) */}
        {!isEditMode && (
          <div className="mb-6 flex gap-2 border-b border-neutral-200">
            <button
              type="button"
              onClick={() => setMode('natural')}
              className={`px-4 py-2 font-medium transition-colors ${
                mode === 'natural'
                  ? 'text-primary-600 border-b-2 border-primary-600'
                  : 'text-neutral-600 hover:text-neutral-900'
              }`}
            >
              Natural Language
            </button>
            <button
              type="button"
              onClick={() => setMode('manual')}
              className={`px-4 py-2 font-medium transition-colors ${
                mode === 'manual'
                  ? 'text-primary-600 border-b-2 border-primary-600'
                  : 'text-neutral-600 hover:text-neutral-900'
              }`}
            >
              Manual Form
            </button>
          </div>
        )}

        {/* Form Content */}
        <div className="bg-white rounded-lg shadow-sm border border-neutral-200 p-6">
          {mode === 'natural' && !isEditMode ? (
            <NaturalLanguageInput
              onParse={handleParse}
              disabled={isSubmitting}
              error={parseError || undefined}
            />
          ) : (
            <TaskForm
              initialValues={initialValues}
              users={users}
              onSubmit={handleSubmit}
              onCancel={handleCancel}
              isSubmitting={isSubmitting}
              submitLabel={isEditMode ? 'Update Task' : 'Create Task'}
            />
          )}
        </div>

        {/* Error Messages */}
        {createMutation.isError && (
          <div className="mt-4 p-4 bg-danger-50 border border-danger-200 rounded-md">
            <p className="text-danger-800">
              Failed to create task:{' '}
              {createMutation.error instanceof Error
                ? createMutation.error.message
                : 'Unknown error'}
            </p>
          </div>
        )}
        {updateMutation.isError && (
          <div className="mt-4 p-4 bg-danger-50 border border-danger-200 rounded-md">
            <p className="text-danger-800">
              Failed to update task:{' '}
              {updateMutation.error instanceof Error
                ? updateMutation.error.message
                : 'Unknown error'}
            </p>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
