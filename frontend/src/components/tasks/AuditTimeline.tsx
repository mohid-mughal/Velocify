/**
 * AuditTimeline Component
 * 
 * Displays a timeline of task audit history showing field changes.
 * 
 * Requirements: 23.5
 */

import React from 'react';
import { format } from 'date-fns';
import { Spinner } from '../ui/Spinner';
import type { TaskAuditLogDto } from '../../api/types';

export interface AuditTimelineProps {
  history: TaskAuditLogDto[];
  isLoading: boolean;
}

export const AuditTimeline: React.FC<AuditTimelineProps> = ({
  history,
  isLoading,
}) => {
  return (
    <div className="bg-white rounded-lg shadow-sm p-6">
      <h2 className="text-xl font-semibold mb-4">History</h2>
      
      {isLoading ? (
        <div className="flex justify-center py-4">
          <Spinner />
        </div>
      ) : history.length === 0 ? (
        <p className="text-neutral-500 text-sm">No history yet</p>
      ) : (
        <div className="space-y-4">
          {history.map((entry) => (
            <div
              key={entry.id}
              className="relative pl-6 pb-4 border-l-2 border-neutral-200 last:pb-0"
            >
              <div className="absolute left-0 top-0 -translate-x-1/2 w-3 h-3 rounded-full bg-primary-500" />
              <div className="text-xs text-neutral-500 mb-1">
                {format(new Date(entry.changedAt), 'MMM dd, HH:mm')}
              </div>
              <div className="text-sm">
                <span className="font-medium text-neutral-900">
                  {entry.changedBy.firstName} {entry.changedBy.lastName}
                </span>
                <span className="text-neutral-600"> changed </span>
                <span className="font-medium text-neutral-900">{entry.fieldName}</span>
              </div>
              {entry.oldValue && entry.newValue && (
                <div className="text-xs text-neutral-500 mt-1">
                  <span className="line-through">{entry.oldValue}</span>
                  {' → '}
                  <span className="font-medium">{entry.newValue}</span>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
