export { default as axiosInstance } from './axios';
export { default as api } from './axios';
export { queryClient } from './queryClient';
export { queryKeys, taskKeys, userKeys, dashboardKeys, aiKeys, notificationKeys, authKeys } from './queryKeys';

// API Services
export { authService } from './auth.service';
export { tasksService } from './tasks.service';
export { dashboardService } from './dashboard.service';
export { aiService } from './ai.service';
export { notificationsService } from './notifications.service';
export { usersService } from './users.service';

// Types
export * from './types';
