/**
 * TaskCard Component
 * 
 * Requirements: 22.1, 22.2, 22.3
 * Task: 23.2
 * 
 * Displays a task card with title, badges, avatar, due date, and AI probability
 */

import { clsx } from 'clsx';
import { format } from 'date-fns';
import { PriorityBadge, StatusBadge, Badge } from '../ui/Badge';
import { Avatar } from '../ui/Avatar';
import type { TaskDto } from '../../api/types';

interface TaskCardProps {
  task: TaskDto;
  isAdmin: boolean;
  isSelected: boolean;
  onSelect: () => void;
  onClick: () => void;
  getDueDateColor: (dueDate: string | null) => string;
}

export function TaskCard({ 
  task, 
  isAdmin, 
  isSelected, 
  onSelect, 
  onClick, 
  getDueDateColor 
}: TaskCardProps) {
  return (
    <div
      className={clsx(
        'bg-white rounded-lg shadow-sm p-4 transition-all hover:shadow-md cursor-pointer',
        isSelected && 'ring-2 ring-primary-500'
      )}
    >
      <div className="flex items-start gap-4">
        {/* Checkbox (Admin Only) */}
        {isAdmin && (
          <input
            type="checkbox"
            checked={isSelected}
            onChange={(e) => {
              e.stopPropagation();
              onSelect();
            }}
            className="mt-1 rounded border-neutral-300 text-primary-600 focus:ring-primary-500"
          />
        )}

        {/* Task Content */}
        <div className="flex-1 min-w-0" onClick={onClick}>
          {/* Title and Badges */}
          <div className="flex items-start justify-between gap-4 mb-2">
            <h3 className="text-lg font-semibold text-gray-900 truncate">
              {task.title}
            </h3>
            <div className="flex gap-2 flex-shrink-0">
              <PriorityBadge priority={task.priority} size="sm" />
              <StatusBadge status={task.status} size="sm" />
            </div>
          </div>

          {/* Description */}
          {task.description && (
            <p className="text-sm text-neutral-600 mb-3 line-clamp-2">
              {task.description}
            </p>
          )}

          {/* Metadata Row */}
          <div className="flex items-center gap-4 text-sm text-neutral-500">
            {/* Assignee */}
            <div className="flex items-center gap-2">
              <Avatar
                name={`${task.assignedTo.firstName} ${task.assignedTo.lastName}`}
                size="sm"
              />
              <span>
                {task.assignedTo.firstName} {task.assignedTo.lastName}
              </span>
            </div>

            {/* Category */}
            <Badge variant="default" size="sm">
              {task.category}
            </Badge>

            {/* Due Date */}
            {task.dueDate && (
              <div className={clsx('flex items-center gap-1', getDueDateColor(task.dueDate))}>
                <span>📅</span>
                <span>{format(new Date(task.dueDate), 'MMM dd, yyyy')}</span>
              </div>
            )}

            {/* Estimated Hours */}
            {task.estimatedHours && (
              <div className="flex items-center gap-1">
                <span>⏱️</span>
                <span>{task.estimatedHours}h</span>
              </div>
            )}

            {/* AI Priority Score */}
            {task.aiPriorityScore !== null && (
              <div className="flex items-center gap-1 text-primary-600">
                <span>🤖</span>
                <span>{Math.round(task.aiPriorityScore * 100)}%</span>
              </div>
            )}
          </div>

          {/* Tags */}
          {task.tags && (
            <div className="flex gap-2 mt-3 flex-wrap">
              {task.tags.split(',').filter(Boolean).map((tag, idx) => (
                <Badge key={idx} variant="secondary" size="sm">
                  {tag.trim()}
                </Badge>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
