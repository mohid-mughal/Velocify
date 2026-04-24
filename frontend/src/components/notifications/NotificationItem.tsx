/**
 * NotificationItem Component
 * 
 * Requirements: 28.1-28.5
 * Task: 27.3
 * 
 * Individual notification item with icon, message, timestamp, and read/unread indicator
 */

import React from 'react';
import { clsx } from 'clsx';
import { format } from 'date-fns';
import type { NotificationType } from '../../api/types';

export interface NotificationItemProps {
  id: string;
  type: NotificationType;
  message: string;
  createdAt: string;
  isRead: boolean;
  onMarkAsRead: (id: string) => void;
  isMarkingAsRead?: boolean;
}

/**
 * Get icon for notification type
 */
const getNotificationIcon = (type: NotificationType): React.ReactNode => {
  switch (type) {
    case 'Assigned':
      return (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
        </svg>
      );
    case 'StatusChanged':
      return (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
      );
    case 'DueSoon':
      return (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
      );
    case 'Overdue':
      return (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
      );
    case 'AiSuggestion':
      return (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
      );
  }
};

/**
 * Get color classes for notification type
 */
const getNotificationColor = (type: NotificationType): string => {
  switch (type) {
    case 'Assigned':
      return 'text-primary-600 bg-primary-50';
    case 'StatusChanged':
      return 'text-success-600 bg-success-50';
    case 'DueSoon':
      return 'text-warning-600 bg-warning-50';
    case 'Overdue':
      return 'text-danger-600 bg-danger-50';
    case 'AiSuggestion':
      return 'text-secondary-600 bg-secondary-50';
  }
};

/**
 * NotificationItem Component
 * 
 * Requirement 28.1: Display notification with icon, message, timestamp, read/unread indicator
 * Requirement 28.2: Mark notification as read
 */
export const NotificationItem: React.FC<NotificationItemProps> = ({
  id,
  type,
  message,
  createdAt,
  isRead,
  onMarkAsRead,
  isMarkingAsRead = false,
}) => {
  return (
    <div
      className={clsx(
        'px-6 py-4 hover:bg-neutral-50 transition-colors',
        !isRead && 'bg-primary-50/30'
      )}
    >
      <div className="flex gap-3">
        {/* Icon */}
        <div className={clsx('flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center', getNotificationColor(type))}>
          {getNotificationIcon(type)}
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <p className={clsx('text-sm', isRead ? 'text-neutral-600' : 'text-neutral-900 font-medium')}>
            {message}
          </p>
          <p className="text-xs text-neutral-500 mt-1">
            {format(new Date(createdAt), 'MMM d, yyyy h:mm a')}
          </p>
          
          {/* Mark as read button */}
          {!isRead && (
            <button
              onClick={() => onMarkAsRead(id)}
              disabled={isMarkingAsRead}
              className="text-xs text-primary-600 hover:text-primary-700 font-medium mt-2 disabled:opacity-50"
            >
              Mark as read
            </button>
          )}
        </div>

        {/* Unread indicator */}
        {!isRead && (
          <div className="flex-shrink-0">
            <div className="w-2 h-2 bg-primary-600 rounded-full" />
          </div>
        )}
      </div>
    </div>
  );
};

export default NotificationItem;
