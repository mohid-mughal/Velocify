/**
 * Task Detail Page
 * 
 * Requirements: 23.1-23.5
 * Task: 24.1, 24.2
 * 
 * Displays complete task information with:
 * - Full task information display
 * - Inline status change dropdown
 * - Comment thread with sentiment indicators
 * - Task audit history timeline
 * - Subtasks list with add/complete/remove actions
 * - AI decomposition button with modal
 * 
 * Refactored to use modular components (Task 24.2)
 */

import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  useTask,
  useUpdateTaskStatus,
  useTaskComments,
  useCreateComment,
  useDeleteComment,
  useTaskHistory,
  useTaskSubtasks,
  useCreateTask,
  useDeleteTask,
} from '../hooks/useTasks';
import { useAiDecompose } from '../hooks/useAi';
import { useAuth } from '../hooks/useAuth';
import { useToast } from '../hooks/useToast';
import { Button } from '../components/ui/Button';
import { Spinner } from '../components/ui/Spinner';
import { PriorityBadge, StatusBadge, Badge } from '../components/ui/Badge';
import {
  TaskInfo,
  CommentThread,
  AuditTimeline,
  SubtasksList,
  DecompositionModal,
} from '../components/tasks';
import type { TaskStatus } from '../api/types';

export default function TaskDetailPage() {
  const { taskId } = useParams<{ taskId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { showSuccess, showError } = useToast();

  const [isDecomposeModalOpen, setIsDecomposeModalOpen] = useState(false);

  // Queries
  const { data: task, isLoading: isLoadingTask, error: taskError } = useTask(taskId!);
  const { data: comments = [], isLoading: isLoadingComments } = useTaskComments(taskId!);
  const { data: history = [], isLoading: isLoadingHistory } = useTaskHistory(taskId!);
  const { data: subtasks = [], isLoading: isLoadingSubtasks } = useTaskSubtasks(taskId!);

  // Mutations
  const updateStatusMutation = useUpdateTaskStatus();
  const createCommentMutation = useCreateComment();
  const deleteCommentMutation = useDeleteComment();
  const decomposeMutation = useAiDecompose();
  const createTaskMutation = useCreateTask();
  const deleteTaskMutation = useDeleteTask();

  // Handle status change
  const handleStatusChange = (newStatus: TaskStatus) => {
    if (!taskId) return;
    
    updateStatusMutation.mutate(
      { taskId, status: { status: newStatus } },
      {
        onSuccess: () => {
          showSuccess({ title: 'Status updated successfully' });
        },
        onError: () => {
          showError({ title: 'Failed to update status' });
        },
      }
    );
  };

  // Handle comment creation
  const handleCreateComment = (content: string) => {
    if (!taskId) return;

    createCommentMutation.mutate(
      { taskId, content: { content } },
      {
        onSuccess: () => {
          showSuccess({ title: 'Comment added' });
        },
        onError: () => {
          showError({ title: 'Failed to add comment' });
        },
      }
    );
  };

  // Handle comment deletion
  const handleDeleteComment = (commentId: string) => {
    if (!taskId) return;
    
    if (confirm('Are you sure you want to delete this comment?')) {
      deleteCommentMutation.mutate(
        { taskId, commentId },
        {
          onSuccess: () => {
            showSuccess({ title: 'Comment deleted' });
          },
          onError: () => {
            showError({ title: 'Failed to delete comment' });
          },
        }
      );
    }
  };

  // Handle AI decomposition
  const handleDecompose = () => {
    if (!taskId) return;

    decomposeMutation.mutate(taskId, {
      onSuccess: () => {
        setIsDecomposeModalOpen(true);
      },
      onError: () => {
        showError({ title: 'Failed to decompose task' });
      },
    });
  };

  // Handle creating subtasks from suggestions
  const handleCreateSubtasks = (selectedIndices: number[]) => {
    if (!taskId || !decomposeMutation.data) return;

    const suggestions = decomposeMutation.data.filter((_, idx) => 
      selectedIndices.includes(idx)
    );

    // Create subtasks sequentially
    Promise.all(
      suggestions.map((suggestion) =>
        createTaskMutation.mutateAsync({
          title: suggestion.title,
          description: '',
          priority: task?.priority || 'Medium',
          category: task?.category || 'Other',
          assignedToUserId: task?.assignedTo.id || null,
          dueDate: null,
          estimatedHours: suggestion.estimatedHours,
          tags: [],
          parentTaskId: taskId,
        })
      )
    )
      .then(() => {
        showSuccess({ title: 'Subtasks created successfully' });
        setIsDecomposeModalOpen(false);
      })
      .catch(() => {
        showError({ title: 'Failed to create some subtasks' });
      });
  };

  // Handle subtask deletion
  const handleDeleteSubtask = (subtaskId: string) => {
    if (confirm('Are you sure you want to delete this subtask?')) {
      deleteTaskMutation.mutate(subtaskId, {
        onSuccess: () => {
          showSuccess({ title: 'Subtask deleted' });
        },
        onError: () => {
          showError({ title: 'Failed to delete subtask' });
        },
      });
    }
  };

  // Loading state
  if (isLoadingTask) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Spinner size="lg" />
      </div>
    );
  }

  // Error state
  if (taskError || !task) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="bg-danger-50 border border-danger-200 rounded-lg p-4">
          <p className="text-danger-800">Failed to load task details.</p>
          <Button onClick={() => navigate('/tasks')} className="mt-4">
            Back to Tasks
          </Button>
        </div>
      </div>
    );
  }

  const isAdmin = user?.role === 'Admin' || user?.role === 'SuperAdmin';
  const canEdit = isAdmin || task.createdBy.id === user?.id || task.assignedTo.id === user?.id;

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="mb-6">
        <Button
          variant="secondary"
          size="sm"
          onClick={() => navigate('/tasks')}
          className="mb-4"
        >
          ← Back to Tasks
        </Button>
        
        <div className="flex items-start justify-between gap-4">
          <div className="flex-1">
            <h1 className="text-3xl font-bold text-neutral-900 mb-2">{task.title}</h1>
            <div className="flex items-center gap-3 flex-wrap">
              <PriorityBadge priority={task.priority} />
              <StatusBadge status={task.status} />
              <Badge variant="default">{task.category}</Badge>
            </div>
          </div>
          
          {canEdit && (
            <div className="flex gap-2">
              <Button
                variant="primary"
                size="sm"
                onClick={handleDecompose}
                isLoading={decomposeMutation.isPending}
              >
                🤖 AI Decompose
              </Button>
            </div>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          {/* Task Information */}
          <TaskInfo
            task={task}
            canEdit={canEdit}
            isUpdatingStatus={updateStatusMutation.isPending}
            onStatusChange={handleStatusChange}
          />

          {/* Comments Section */}
          <CommentThread
            comments={comments}
            isLoading={isLoadingComments}
            isCreating={createCommentMutation.isPending}
            isDeleting={deleteCommentMutation.isPending}
            currentUserId={user?.id}
            isAdmin={isAdmin}
            onCreateComment={handleCreateComment}
            onDeleteComment={handleDeleteComment}
          />

          {/* Subtasks Section */}
          <SubtasksList
            subtasks={subtasks}
            isLoading={isLoadingSubtasks}
            canEdit={canEdit}
            isDeleting={deleteTaskMutation.isPending}
            onDeleteSubtask={handleDeleteSubtask}
          />
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Audit History */}
          <AuditTimeline
            history={history}
            isLoading={isLoadingHistory}
          />
        </div>
      </div>

      {/* AI Decomposition Modal */}
      <DecompositionModal
        isOpen={isDecomposeModalOpen}
        isLoading={decomposeMutation.isPending}
        isCreating={createTaskMutation.isPending}
        suggestions={decomposeMutation.data}
        onClose={() => setIsDecomposeModalOpen(false)}
        onCreateSubtasks={handleCreateSubtasks}
      />
    </div>
  );
}
