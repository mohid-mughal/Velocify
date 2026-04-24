// Export all stores from a single entry point
export { useAuthStore, useUser, useIsAuthenticated, useUserRole, useAccessToken } from './authStore';
export type { User } from './authStore';

export {
  useNotificationStore,
  useNotifications,
  useUnreadCount,
  useNotificationActions,
} from './notificationStore';
export type { Notification, NotificationType } from './notificationStore';
