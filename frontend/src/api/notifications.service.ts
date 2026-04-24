/**
 * Notifications API Service
 * 
 * Requirements: 28.1-28.5
 * 
 * Handles all notification-related API calls:
 * - Get notifications (paginated, filterable)
 * - Mark notification as read
 * - Mark all notifications as read
 */

import axiosInstance from './axios';
import type { NotificationDto, PagedResult, NotificationFilters } from './types';

/**
 * Get paginated list of notifications for the current user
 * 
 * Requirement 28.1: Users can view their notifications
 * Requirement 28.2: Notifications are ordered by creation time
 * Requirement 27.1: Notification list ordered by creation time
 * 
 * @param filters - Optional filters for isRead, page, pageSize
 * @returns PagedResult of NotificationDto
 */
export async function getNotifications(filters?: NotificationFilters): Promise<PagedResult<NotificationDto>> {
  const response = await axiosInstance.get<PagedResult<NotificationDto>>('/notifications', {
    params: filters,
  });
  return response.data;
}

/**
 * Mark a single notification as read
 * 
 * Requirement 28.3: Users can mark notifications as read
 * Requirement 27.1: Mark as read button per notification
 * 
 * @param id - Notification ID
 */
export async function markAsRead(id: string): Promise<void> {
  await axiosInstance.patch(`/notifications/${id}/read`);
}

/**
 * Mark all notifications as read for the current user
 * 
 * Requirement 28.4: Users can mark all notifications as read
 * Requirement 27.1: Mark all as read button
 */
export async function markAllAsRead(): Promise<void> {
  await axiosInstance.patch('/notifications/read-all');
}

/**
 * Notifications service object with all notification-related methods
 */
export const notificationsService = {
  getNotifications,
  markAsRead,
  markAllAsRead,
};

export default notificationsService;
