/**
 * OverdueAlert Component
 * 
 * Requirements: 7.5
 * Task: 22.2
 * 
 * Displays alert for overdue tasks with task list
 */

import { format } from 'date-fns';

interface OverdueTask {
  id: string;
  title: string;
  dueDate: string | null;
}

interface OverdueAlertProps {
  tasks: OverdueTask[];
  loading?: boolean;
}

export function OverdueAlert({ tasks, loading }: OverdueAlertProps) {
  if (loading) {
    return (
      <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-8">
        <div className="flex items-start">
          <div className="flex-shrink-0">
            <span className="text-2xl">⚠️</span>
          </div>
          <div className="ml-3 flex-1">
            <div className="h-6 w-32 bg-red-200 animate-pulse rounded mb-2"></div>
            <div className="h-16 bg-red-100 animate-pulse rounded"></div>
          </div>
        </div>
      </div>
    );
  }

  if (!tasks || tasks.length === 0) {
    return null;
  }

  return (
    <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-8">
      <div className="flex items-start">
        <div className="flex-shrink-0">
          <span className="text-2xl">⚠️</span>
        </div>
        <div className="ml-3 flex-1">
          <h3 className="text-sm font-medium text-red-800">
            {tasks.length} Overdue Task{tasks.length !== 1 ? 's' : ''}
          </h3>
          <div className="mt-2 text-sm text-red-700">
            <ul className="list-disc list-inside space-y-1">
              {tasks.slice(0, 5).map(task => (
                <li key={task.id}>
                  {task.title} - Due: {task.dueDate ? format(new Date(task.dueDate), 'MMM dd, yyyy') : 'N/A'}
                </li>
              ))}
              {tasks.length > 5 && (
                <li className="text-red-600 font-medium">
                  And {tasks.length - 5} more...
                </li>
              )}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
