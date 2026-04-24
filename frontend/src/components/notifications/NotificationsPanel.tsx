/**
 * NotificationsPanel Component
 * 
 * Requirements: 28.1-28.5, 27.1
 * 
 * Slide-in drawer from right side displaying notifications with:
 * - Notification list ordered by creation time
 * - Icon, message, timestamp, read/unread indicator per notification
 * - Mark as read button per notification
 * - Mark all as read button
 * - Real-time updates via SignalR
 * - Unread count badge on notification bell
 */

import React, { useEffect } from 'react';
import { clsx } from 'clsx';
import { Button } from '../ui/Button';
import { Badge } from '../ui/Badge';
import { Spinner } from '../ui/Spinner';
import { NotificationItem } from './NotificationItem';
import { useNotificationStore } from '../../store/notificationStore';
import { notificationsService } from '../../api/notifications.service';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationKeys } from '../../api/queryKeys';

export interface NotificationsPanelProps {
  isOpen: boolean;
  onClose: () => void;
}

/**
 * NotificationsPanel Component
 * 
 * Requirement 28.5: Implement notifications panel as slide-in drawer
 * Requirement 27.1: Slide-in drawer from right side
 */
export const NotificationsPanel: React.FC<NotificationsPanelProps> = ({ isOpen, onClose }) => {
  const queryClient = useQueryClient();
  
  // Get notifications from Zustand store
  const notifications = useNotificationStore((state) => state.notifications);
  const unreadCount = useNotificationStore((state) => state.unreadCount);
  const setNotifications = useNotificationStore((state) => state.setNotifications);
  const markAsReadInStore = useNotificationStore((state) => state.markAsRead);
  const markAllAsReadInStore = useNotificationStore((state) => state.markAllAsRead);

  // Fetch notifications from API
  // Requirement 28.1: Display all notifications ordered by creation time
  const { data, isLoading, error } = useQuery({
    queryKey: notificationKeys.lists(),
    queryFn: () => notificationsService.getNotifications({ pageSize: 50 }),
    enabled: isOpen, // Only fetch when panel is open
  });

  // Update store when data is fetched
  useEffect(() => {
    if (data?.items) {
      setNotifications(data.items);
    }
  }, [data, setNotifications]);

  // Mark as read mutation
  // Requirement 28.2: Mark notification as read and decrement unread count
  const markAsReadMutation = useMutation({
    mutationFn: (notificationId: string) => notificationsService.markAsRead(notificationId),
    onSuccess: (_, notificationId) => {
      // Update store optimistically
      markAsReadInStore(notificationId);
      // Invalidate query to refetch
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() });
    },
  });

  // Mark all as read mutation
  // Requirement 28.3: Mark all as read and reset unread count to zero
  const markAllAsReadMutation = useMutation({
    mutationFn: () => notificationsService.markAllAsRead(),
    onSuccess: () => {
      // Update store optimistically
      markAllAsReadInStore();
      // Invalidate query to refetch
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() });
    },
  });

  const handleMarkAsRead = (notificationId: string) => {
    markAsReadMutation.mutate(notificationId);
  };

  const handleMarkAllAsRead = () => {
    markAllAsReadMutation.mutate();
  };

  // Close panel when clicking outside
  const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <>
      {/* Backdrop */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
          onClick={handleBackdropClick}
          aria-hidden="true"
        />
      )}

      {/* Slide-in drawer */}
      <div
        className={clsx(
          'fixed top-0 right-0 h-full w-full sm:w-96 bg-white shadow-xl z-50 transform transition-transform duration-300 ease-in-out',
          isOpen ? 'translate-x-0' : 'translate-x-full'
        )}
        role="dialog"
        aria-modal="true"
        aria-labelledby="notifications-title"
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-neutral-200">
          <div className="flex items-center gap-3">
            <h2 id="notifications-title" className="text-lg font-semibold text-neutral-900">
              Notifications
            </h2>
            {unreadCount > 0 && (
              <Badge variant="danger" size="sm">
                {unreadCount}
              </Badge>
            )}
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-md text-neutral-500 hover:bg-neutral-100 focus:outline-none focus:ring-2 focus:ring-primary-500"
            aria-label="Close notifications"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Mark all as read button */}
        {unreadCount > 0 && (
          <div className="px-6 py-3 border-b border-neutral-200">
            <Button
              variant="secondary"
              size="sm"
              onClick={handleMarkAllAsRead}
              isLoading={markAllAsReadMutation.isPending}
              fullWidth
            >
              Mark all as read
            </Button>
          </div>
        )}

        {/* Notifications list */}
        <div className="flex-1 overflow-y-auto h-[calc(100vh-8rem)]">
          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <Spinner size="lg" />
            </div>
          ) : error ? (
            <div className="px-6 py-12 text-center">
              <p className="text-sm text-danger-600">Failed to load notifications</p>
            </div>
          ) : notifications.length === 0 ? (
            <div className="px-6 py-12 text-center">
              <svg
                className="w-16 h-16 mx-auto text-neutral-300 mb-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={1.5}
                  d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
                />
              </svg>
              <p className="text-sm text-neutral-500">No notifications yet</p>
            </div>
          ) : (
            <div className="divide-y divide-neutral-100">
              {notifications.map((notification) => (
                <NotificationItem
                  key={notification.id}
                  id={notification.id}
                  type={notification.type}
                  message={notification.message}
                  createdAt={notification.createdAt}
                  isRead={notification.isRead}
                  onMarkAsRead={handleMarkAsRead}
                  isMarkingAsRead={markAsReadMutation.isPending}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default NotificationsPanel;
