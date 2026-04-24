import { create } from 'zustand';

// Notification type matching backend NotificationType enum
export type NotificationType = 'DueSoon' | 'Overdue' | 'Assigned' | 'StatusChanged' | 'AiSuggestion';

// Notification interface matching backend Notification entity
export interface Notification {
  id: string;
  userId: string;
  type: NotificationType;
  message: string;
  isRead: boolean;
  createdAt: string;
  taskItemId: string | null;
}

// Notification state interface
interface NotificationState {
  // State (Requirements 28.1-28.5)
  unreadCount: number;
  notifications: Notification[];

  // Actions (Requirements 28.1-28.5)
  addNotification: (notification: Notification) => void;
  markAsRead: (notificationId: string) => void;
  markAllAsRead: () => void;
  incrementUnread: () => void;
  setNotifications: (notifications: Notification[]) => void;
  setUnreadCount: (count: number) => void;
}

/**
 * Zustand notification store for managing notification state
 * 
 * Requirements 28.1-28.5:
 * - 28.1: Display all notifications ordered by creation time
 * - 28.2: Mark notification as read and decrement unread count
 * - 28.3: Mark all as read and reset unread count to zero
 * - 28.4: Increment unread count badge when new notification arrives via SignalR
 * - 28.5: Implement notifications panel as slide-in drawer
 * 
 * State Management:
 * - unreadCount: Number of unread notifications for badge display
 * - notifications: Array of all notifications ordered by creation time
 * 
 * Real-time Updates (Requirement 6.4):
 * - addNotification: Called when SignalR pushes new notification
 * - incrementUnread: Called when new notification arrives
 * 
 * Integration with SignalR (Requirement 25.1-25.5):
 * - Store will be updated by SignalR event handlers
 * - TanStack Query cache will be invalidated on updates
 */
export const useNotificationStore = create<NotificationState>((set) => ({
  // Initial state
  unreadCount: 0,
  notifications: [],

  // Add a new notification (typically from SignalR event)
  // Requirement 28.4: Increment unread count when new notification arrives
  addNotification: (notification: Notification) => {
    set((state) => ({
      notifications: [notification, ...state.notifications], // Prepend to maintain order
      unreadCount: notification.isRead ? state.unreadCount : state.unreadCount + 1,
    }));
  },

  // Mark a single notification as read
  // Requirement 28.2: Update notification status and decrement unread count
  markAsRead: (notificationId: string) => {
    set((state) => {
      const notification = state.notifications.find((n) => n.id === notificationId);
      if (!notification || notification.isRead) {
        return state; // No change if not found or already read
      }

      return {
        notifications: state.notifications.map((n) =>
          n.id === notificationId ? { ...n, isRead: true } : n
        ),
        unreadCount: Math.max(0, state.unreadCount - 1), // Ensure non-negative
      };
    });
  },

  // Mark all notifications as read
  // Requirement 28.3: Update all notifications and reset unread count to zero
  markAllAsRead: () => {
    set((state) => ({
      notifications: state.notifications.map((n) => ({ ...n, isRead: true })),
      unreadCount: 0,
    }));
  },

  // Increment unread count (for SignalR events when notification details aren't immediately available)
  // Requirement 28.4: Increment unread count badge
  incrementUnread: () => {
    set((state) => ({
      unreadCount: state.unreadCount + 1,
    }));
  },

  // Set notifications (typically from API fetch)
  // Requirement 28.1: Display all notifications ordered by creation time
  setNotifications: (notifications: Notification[]) => {
    const unreadCount = notifications.filter((n) => !n.isRead).length;
    set({
      notifications,
      unreadCount,
    });
  },

  // Set unread count directly (for initial load or sync)
  setUnreadCount: (count: number) => {
    set({ unreadCount: Math.max(0, count) }); // Ensure non-negative
  },
}));

// Selector hooks for common use cases
export const useNotifications = () => useNotificationStore((state) => state.notifications);
export const useUnreadCount = () => useNotificationStore((state) => state.unreadCount);
export const useNotificationActions = () =>
  useNotificationStore((state) => ({
    addNotification: state.addNotification,
    markAsRead: state.markAsRead,
    markAllAsRead: state.markAllAsRead,
    incrementUnread: state.incrementUnread,
    setNotifications: state.setNotifications,
    setUnreadCount: state.setUnreadCount,
  }));
