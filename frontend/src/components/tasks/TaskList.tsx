/**
 * TaskList Component
 * 
 * Requirements: 22.1
 * Task: 23.2
 * 
 * Displays a list of task cards with loading and empty states
 */

import { Spinner } from '../ui/Spinner';
import { TaskCard } from './TaskCard';
import type { TaskDto } from '../../api/types';

interface TaskListProps {
  tasks: TaskDto[];
  isLoading: boolean;
  error: Error | null;
  isAdmin: boolean;
  selectedTasks: Set<string>;
  onTaskSelect: (taskId: string) => void;
  onTaskClick: (taskId: string) => void;
  getDueDateColor: (dueDate: string | null) => string;
}

export function TaskList({
  tasks,
  isLoading,
  error,
  isAdmin,
  selectedTasks,
  onTaskSelect,
  onTaskClick,
  getDueDateColor,
}: TaskListProps) {
  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-12">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-danger-50 border border-danger-200 rounded-lg p-4 text-danger-800">
        Error loading tasks. Please try again.
      </div>
    );
  }

  if (tasks.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-12 text-center">
        <p className="text-neutral-500 text-lg">No tasks found</p>
        <p className="text-neutral-400 text-sm mt-2">
          Try adjusting your filters or create a new task
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {tasks.map((task) => (
        <TaskCard
          key={task.id}
          task={task}
          isAdmin={isAdmin}
          isSelected={selectedTasks.has(task.id)}
          onSelect={() => onTaskSelect(task.id)}
          onClick={() => onTaskClick(task.id)}
          getDueDateColor={getDueDateColor}
        />
      ))}
    </div>
  );
}
