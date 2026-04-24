/**
 * TaskInfo Component
 * 
 * Displays editable task information fields including status dropdown,
 * description, metadata, and tags.
 * 
 * Requirements: 23.1, 23.2
 */

import React from 'react';
import { clsx } from 'clsx';
import { format } from 'date-fns';
import { Select } from '../ui/Select';
import { Avatar } from '../ui/Avatar';
import { Badge } from '../ui/Badge';
import type { TaskDetailDto, TaskStatus } from '../../api/types';

export interface TaskInfoProps {
  task: TaskDetailDto;
  canEdit: boolean;
  isUpdatingStatus: boolean;
  onStatusChange: (status: TaskStatus) => void;
}

export const TaskInfo: React.FC<TaskInfoProps> = ({
  task,
  canEdit,
  isUpdatingStatus,
  onStatusChange,
}) => {
  const getDueDateColor = (dueDate: string | null) => {
    if (!dueDate) return 'text-neutral-500';
    const date = new Date(dueDate);
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const taskDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    
    if (taskDate < today) return 'text-danger-600';
    if (taskDate.getTime() === today.getTime()) return 'text-warning-600';
    return 'text-neutral-500';
  };

  return (
    <div className="bg-white rounded-lg shadow-sm p-6">
      <h2 className="text-xl font-semibold mb-4">Task Details</h2>
      
      <div className="space-y-4">
        {/* Status Dropdown */}
        <div>
          <Select
            label="Status"
            value={task.status}
            onChange={(e) => onStatusChange(e.target.value as TaskStatus)}
            disabled={!canEdit || isUpdatingStatus}
            options={[
              { value: 'Pending', label: 'Pending' },
              { value: 'InProgress', label: 'In Progress' },
              { value: 'Completed', label: 'Completed' },
              { value: 'Blocked', label: 'Blocked' },
              { value: 'Cancelled', label: 'Cancelled' },
            ]}
          />
        </div>

        {/* Description */}
        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Description
          </label>
          <p className="text-neutral-600 whitespace-pre-wrap">
            {task.description || 'No description provided'}
          </p>
        </div>

        {/* Metadata Grid */}
        <div className="grid grid-cols-2 gap-4 pt-4 border-t">
          <div>
            <span className="text-sm text-neutral-500">Assigned To</span>
            <div className="flex items-center gap-2 mt-1">
              <Avatar
                name={`${task.assignedTo.firstName} ${task.assignedTo.lastName}`}
                size="sm"
              />
              <span className="text-neutral-900">
                {task.assignedTo.firstName} {task.assignedTo.lastName}
              </span>
            </div>
          </div>

          <div>
            <span className="text-sm text-neutral-500">Created By</span>
            <div className="flex items-center gap-2 mt-1">
              <Avatar
                name={`${task.createdBy.firstName} ${task.createdBy.lastName}`}
                size="sm"
              />
              <span className="text-neutral-900">
                {task.createdBy.firstName} {task.createdBy.lastName}
              </span>
            </div>
          </div>

          {task.dueDate && (
            <div>
              <span className="text-sm text-neutral-500">Due Date</span>
              <p className={clsx('mt-1 font-medium', getDueDateColor(task.dueDate))}>
                {format(new Date(task.dueDate), 'MMM dd, yyyy')}
              </p>
            </div>
          )}

          {task.completedAt && (
            <div>
              <span className="text-sm text-neutral-500">Completed At</span>
              <p className="text-neutral-900 mt-1">
                {format(new Date(task.completedAt), 'MMM dd, yyyy HH:mm')}
              </p>
            </div>
          )}

          {task.estimatedHours && (
            <div>
              <span className="text-sm text-neutral-500">Estimated Hours</span>
              <p className="text-neutral-900 mt-1">{task.estimatedHours}h</p>
            </div>
          )}

          {task.actualHours && (
            <div>
              <span className="text-sm text-neutral-500">Actual Hours</span>
              <p className="text-neutral-900 mt-1">{task.actualHours}h</p>
            </div>
          )}

          {task.aiPriorityScore !== null && (
            <div>
              <span className="text-sm text-neutral-500">AI Priority Score</span>
              <p className="text-primary-600 mt-1 font-medium">
                {Math.round(task.aiPriorityScore * 100)}%
              </p>
            </div>
          )}

          {task.predictedCompletionProbability !== null && (
            <div>
              <span className="text-sm text-neutral-500">Completion Probability</span>
              <p className="text-primary-600 mt-1 font-medium">
                {Math.round(task.predictedCompletionProbability * 100)}%
              </p>
            </div>
          )}
        </div>

        {/* Tags */}
        {task.tags && (
          <div className="pt-4 border-t">
            <span className="text-sm text-neutral-500 block mb-2">Tags</span>
            <div className="flex gap-2 flex-wrap">
              {task.tags.split(',').filter(Boolean).map((tag, idx) => (
                <Badge key={idx} variant="secondary" size="sm">
                  {tag.trim()}
                </Badge>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
