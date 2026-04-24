/**
 * SubtasksList Component
 * 
 * Displays a list of subtasks with actions to navigate or remove them.
 * 
 * Requirements: 23.1
 */

import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../ui/Button';
import { Spinner } from '../ui/Spinner';
import { StatusBadge } from '../ui/Badge';
import type { TaskDto } from '../../api/types';

export interface SubtasksListProps {
  subtasks: TaskDto[];
  isLoading: boolean;
  canEdit: boolean;
  isDeleting: boolean;
  onDeleteSubtask: (subtaskId: string) => void;
}

export const SubtasksList: React.FC<SubtasksListProps> = ({
  subtasks,
  isLoading,
  canEdit,
  isDeleting,
  onDeleteSubtask,
}) => {
  const navigate = useNavigate();

  return (
    <div className="bg-white rounded-lg shadow-sm p-6">
      <h2 className="text-xl font-semibold mb-4">Subtasks</h2>
      
      {isLoading ? (
        <div className="flex justify-center py-4">
          <Spinner />
        </div>
      ) : subtasks.length === 0 ? (
        <p className="text-neutral-500 text-center py-4">No subtasks yet</p>
      ) : (
        <div className="space-y-3">
          {subtasks.map((subtask) => (
            <div
              key={subtask.id}
              className="flex items-center justify-between p-3 border border-neutral-200 rounded-lg hover:bg-neutral-50 cursor-pointer"
              onClick={() => navigate(`/tasks/${subtask.id}`)}
            >
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-1">
                  <h3 className="font-medium text-neutral-900">{subtask.title}</h3>
                  <StatusBadge status={subtask.status} size="sm" />
                </div>
                {subtask.estimatedHours && (
                  <p className="text-sm text-neutral-500">
                    Estimated: {subtask.estimatedHours}h
                  </p>
                )}
              </div>
              
              {canEdit && (
                <Button
                  variant="danger"
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    onDeleteSubtask(subtask.id);
                  }}
                  disabled={isDeleting}
                >
                  Remove
                </Button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
