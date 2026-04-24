/**
 * NotificationBell Component
 * 
 * Requirements: 28.1-28.5
 * Task: 27.3
 * 
 * Notification bell icon with unread count badge
 */

import React from 'react';

export interface NotificationBellProps {
  unreadCount: number;
  onClick: () => void;
  className?: string;
}

/**
 * NotificationBell Component
 * 
 * Requirement 28.4: Display unread count badge on notification bell
 */
export const NotificationBell: React.FC<NotificationBellProps> = ({
  unreadCount,
  onClick,
  className,
}) => {
  return (
    <div className={className}>
      <button
        onClick={onClick}
        className="relative p-2 rounded-md text-neutral-600 hover:bg-neutral-100 focus:outline-none focus:ring-2 focus:ring-primary-500"
        aria-label="Notifications"
      >
        <svg
          className="w-6 h-6"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
          />
        </svg>
        {unreadCount > 0 && (
          <span className="absolute top-1 right-1 w-5 h-5 bg-danger-500 text-white text-xs font-bold rounded-full flex items-center justify-center">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>
    </div>
  );
};

export default NotificationBell;
