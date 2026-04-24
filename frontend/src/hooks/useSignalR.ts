/**
 * SignalR Hook for Real-Time Notifications
 * 
 * Requirements: 6.7, 6.8, 25.1-25.6
 * 
 * Custom hook for managing SignalR connections and handling real-time events:
 * - Establishes connection on login, disconnects on logout
 * - Implements automatic reconnection with exponential backoff
 * - Handles TaskAssigned, StatusChanged, CommentAdded, AiSuggestionReady events
 * - Invalidates TanStack Query cache keys on events
 */

import { useEffect, useRef, useCallback } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/authStore';
import { useNotificationStore } from '../store/notificationStore';
import { useToast } from './useToast';
import { taskKeys, notificationKeys, dashboardKeys, aiKeys } from '../api/queryKeys';

/**
 * SignalR event payloads matching backend TaskHubService
 */
interface TaskAssignedEvent {
  taskId: string;
  taskTitle: string;
  timestamp: string;
}

interface StatusChangedEvent {
  taskId: string;
  taskTitle: string;
  newStatus: string;
  timestamp: string;
}

interface CommentAddedEvent {
  taskId: string;
  taskTitle: string;
  commenterName: string;
  timestamp: string;
}

interface AiSuggestionReadyEvent {
  suggestionType: string;
  message: string;
  timestamp: string;
}

/**
 * Connection states for SignalR
 */
export type ConnectionState = 
  | 'Disconnected'
  | 'Connecting'
  | 'Connected'
  | 'Disconnecting'
  | 'Reconnecting';

/**
 * Hook return type
 */
interface UseSignalRReturn {
  connectionState: ConnectionState;
  isConnected: boolean;
  isReconnecting: boolean;
}

/**
 * Exponential backoff delays in seconds
 * Pattern: 0s, 2s, 4s, 8s, 16s, 30s (max)
 * 
 * Requirement 6.8: Automatic reconnection with exponential backoff
 * Requirement 25.5: Attempt reconnection with exponential backoff on connection loss
 */
const RECONNECTION_DELAYS = [0, 2000, 4000, 8000, 16000, 30000];

/**
 * Get the SignalR hub URL from environment or default
 */
const getHubUrl = (): string => {
  return (
    import.meta.env.VITE_SIGNALR_HUB_URL ||
    `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'}/hubs/task`
  );
};

/**
 * Hook to manage SignalR connection for real-time task notifications
 * 
 * This hook automatically:
 * 1. Establishes connection when user is authenticated (Requirement 6.7)
 * 2. Disconnects when user logs out (Requirement 6.7)
 * 3. Reconnects with exponential backoff on connection loss (Requirement 6.8)
 * 4. Handles all SignalR events and invalidates relevant caches (Requirements 25.1-25.4)
 * 
 * @returns Connection state information
 * 
 * @example
 * // In your App.tsx or root component
 * function App() {
 *   const { isConnected, isReconnecting } = useSignalR();
 *   
 *   return (
 *     <div>
 *       {isReconnecting && <ReconnectionBanner />}
 *     </div>
 *   );
 * }
 */
export function useSignalR(): UseSignalRReturn {
  const queryClient = useQueryClient();
  const { showSuccess, showInfo } = useToast();
  
  // Auth state
  const accessToken = useAuthStore((state) => state.accessToken);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  
  // Notification store actions
  const incrementUnread = useNotificationStore((state) => state.incrementUnread);
  
  // Connection ref to persist across renders
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const connectionStateRef = useRef<ConnectionState>('Disconnected');
  
  // Track if we've already set up this connection
  const isInitializedRef = useRef(false);

  /**
   * Handle TaskAssigned event
   * 
   * Requirement 25.1: Invalidate relevant TanStack Query cache keys
   * Requirement 25.2: Display toast notification and update task list
   * Requirement 28.4: Increment unread count badge when new notification arrives via SignalR
   */
  const handleTaskAssigned = useCallback(
    (event: TaskAssignedEvent) => {
      // Invalidate task lists to show the new task
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      
      // Invalidate dashboard to update task counts
      queryClient.invalidateQueries({ queryKey: dashboardKeys.summary() });
      
      // Invalidate notifications in case a notification was created
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() });
      
      // Increment unread count in notification store
      incrementUnread();
      
      // Show toast notification
      showSuccess({
        title: 'New Task Assigned',
        message: `"${event.taskTitle}" has been assigned to you.`,
      });
    },
    [queryClient, showSuccess, incrementUnread]
  );

  /**
   * Handle StatusChanged event
   * 
   * Requirement 25.1: Invalidate relevant TanStack Query cache keys
   * Requirement 25.3: Update task detail view if currently displayed
   * Requirement 28.4: Increment unread count badge when new notification arrives via SignalR
   */
  const handleStatusChanged = useCallback(
    (event: StatusChangedEvent) => {
      // Invalidate the specific task detail
      queryClient.invalidateQueries({ queryKey: taskKeys.detail(event.taskId) });
      
      // Invalidate task lists since status affects filtering/sorting
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      
      // Invalidate dashboard to update status counts
      queryClient.invalidateQueries({ queryKey: dashboardKeys.summary() });
      
      // Invalidate task history since status change is logged
      queryClient.invalidateQueries({ queryKey: taskKeys.history(event.taskId) });
      
      // Invalidate notifications
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() });
      
      // Increment unread count in notification store
      incrementUnread();
      
      // Show info notification
      showInfo({
        title: 'Task Status Updated',
        message: `"${event.taskTitle}" status changed to ${event.newStatus}.`,
      });
    },
    [queryClient, showInfo, incrementUnread]
  );

  /**
   * Handle CommentAdded event
   * 
   * Requirement 25.1: Invalidate relevant TanStack Query cache keys
   * Requirement 25.4: Append comment to comment thread
   * Requirement 28.4: Increment unread count badge when new notification arrives via SignalR
   */
  const handleCommentAdded = useCallback(
    (event: CommentAddedEvent) => {
      // Invalidate comments for this task
      queryClient.invalidateQueries({ queryKey: taskKeys.comments(event.taskId) });
      
      // Invalidate the task detail since it includes comments
      queryClient.invalidateQueries({ queryKey: taskKeys.detail(event.taskId) });
      
      // Invalidate notifications
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() });
      
      // Increment unread count in notification store
      incrementUnread();
      
      // Show info notification
      showInfo({
        title: 'New Comment',
        message: `${event.commenterName} commented on "${event.taskTitle}".`,
      });
    },
    [queryClient, showInfo, incrementUnread]
  );

  /**
   * Handle AiSuggestionReady event
   * 
   * Requirement 25.1: Invalidate relevant TanStack Query cache keys
   * Requirement 28.4: Increment unread count badge when new notification arrives via SignalR
   */
  const handleAiSuggestionReady = useCallback(
    (event: AiSuggestionReadyEvent) => {
      // Invalidate AI-related queries based on suggestion type
      if (event.suggestionType === 'DailyDigest') {
        queryClient.invalidateQueries({ queryKey: aiKeys.digest() });
      } else if (event.suggestionType === 'WorkloadBalancing') {
        queryClient.invalidateQueries({ queryKey: aiKeys.workloadSuggestions() });
      }
      
      // Invalidate notifications since AI suggestions may create notifications
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() });
      
      // Increment unread count in notification store
      incrementUnread();
      
      // Show info notification
      showInfo({
        title: 'AI Suggestion Ready',
        message: event.message,
      });
    },
    [queryClient, showInfo, incrementUnread]
  );

  /**
   * Set up event handlers for the SignalR connection
   */
  const setupEventHandlers = useCallback(
    (connection: signalR.HubConnection) => {
      // TaskAssigned event (Requirement 25.2)
      connection.on('TaskAssigned', handleTaskAssigned);
      
      // StatusChanged event (Requirement 25.3)
      connection.on('StatusChanged', handleStatusChanged);
      
      // CommentAdded event (Requirement 25.4)
      connection.on('CommentAdded', handleCommentAdded);
      
      // AiSuggestionReady event
      connection.on('AiSuggestionReady', handleAiSuggestionReady);
    },
    [handleTaskAssigned, handleStatusChanged, handleCommentAdded, handleAiSuggestionReady]
  );

  /**
   * Clean up event handlers from the SignalR connection
   */
  const cleanupEventHandlers = useCallback(
    (connection: signalR.HubConnection) => {
      connection.off('TaskAssigned', handleTaskAssigned);
      connection.off('StatusChanged', handleStatusChanged);
      connection.off('CommentAdded', handleCommentAdded);
      connection.off('AiSuggestionReady', handleAiSuggestionReady);
    },
    [handleTaskAssigned, handleStatusChanged, handleCommentAdded, handleAiSuggestionReady]
  );

  /**
   * Create and configure a new SignalR connection
   */
  const createConnection = useCallback((): signalR.HubConnection => {
    const hubUrl = getHubUrl();
    
    // Create connection with JWT authentication
    // Requirement 6.5: Authenticate connections using JWT token
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => {
          // Return the current access token from auth store
          // This is called by SignalR when establishing the connection
          return accessToken || '';
        },
        // Skip negotiation for CORS simplicity (WebSocket only)
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      // Configure automatic reconnection with exponential backoff
      // Requirement 6.8: Automatic reconnection with exponential backoff
      // Requirement 25.5: Attempt reconnection with exponential backoff
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext: signalR.RetryContext) => {
          // If we've exceeded our predefined delays, use the max (30s)
          if (retryContext.previousRetryCount >= RECONNECTION_DELAYS.length) {
            return RECONNECTION_DELAYS[RECONNECTION_DELAYS.length - 1];
          }
          return RECONNECTION_DELAYS[retryContext.previousRetryCount];
        },
      })
      // Configure logging based on environment
      .configureLogging(
        import.meta.env.VITE_DEBUG === 'true' 
          ? signalR.LogLevel.Debug 
          : signalR.LogLevel.Warning
      )
      .build();

    return connection;
  }, [accessToken]);

  /**
   * Start the SignalR connection
   */
  const startConnection = useCallback(async (connection: signalR.HubConnection) => {
    try {
      connectionStateRef.current = 'Connecting';
      await connection.start();
      connectionStateRef.current = 'Connected';
      isInitializedRef.current = true;
    } catch (error) {
      connectionStateRef.current = 'Disconnected';
      console.error('SignalR connection error:', error);
      // Connection will be retried by withAutomaticReconnect
    }
  }, []);

  /**
   * Stop the SignalR connection
   */
  const stopConnection = useCallback(async (connection: signalR.HubConnection) => {
    try {
      connectionStateRef.current = 'Disconnecting';
      await connection.stop();
      connectionStateRef.current = 'Disconnected';
      isInitializedRef.current = false;
    } catch (error) {
      console.error('SignalR disconnection error:', error);
      connectionStateRef.current = 'Disconnected';
    }
  }, []);

  /**
   * Main effect: Manage connection lifecycle based on auth state
   * 
   * Requirement 6.7: Establish connection on login, close on logout
   * Requirement 25.6: Establish SignalR connection on login and close on logout
   */
  useEffect(() => {
    // Only establish connection if user is authenticated
    if (!isAuthenticated || !accessToken) {
      // If we have an existing connection, stop it
      if (connectionRef.current && connectionStateRef.current !== 'Disconnected') {
        cleanupEventHandlers(connectionRef.current);
        stopConnection(connectionRef.current);
        connectionRef.current = null;
      }
      return;
    }

    // Skip if we already have an active connection
    if (connectionRef.current && connectionStateRef.current === 'Connected') {
      return;
    }

    // Create new connection
    const connection = createConnection();
    connectionRef.current = connection;

    // Set up event handlers
    setupEventHandlers(connection);

    // Set up connection lifecycle handlers
    connection.onreconnecting(() => {
      connectionStateRef.current = 'Reconnecting';
    });

    connection.onreconnected(() => {
      connectionStateRef.current = 'Connected';
    });

    connection.onclose(() => {
      connectionStateRef.current = 'Disconnected';
    });

    // Start the connection
    startConnection(connection);

    // Cleanup on unmount or when auth state changes
    return () => {
      if (connectionRef.current) {
        cleanupEventHandlers(connectionRef.current);
        stopConnection(connectionRef.current);
        connectionRef.current = null;
      }
    };
  }, [
    isAuthenticated,
    accessToken,
    createConnection,
    startConnection,
    stopConnection,
    setupEventHandlers,
    cleanupEventHandlers,
  ]);

  // Get current connection state from the connection object if available
  const getConnectionState = useCallback((): ConnectionState => {
    if (!connectionRef.current) {
      return 'Disconnected';
    }
    
    // Map SignalR's ConnectionState to our type
    const state = connectionRef.current.state;
    switch (state) {
      case signalR.HubConnectionState.Disconnected:
        return 'Disconnected';
      case signalR.HubConnectionState.Connected:
        return 'Connected';
      case signalR.HubConnectionState.Connecting:
        return 'Connecting';
      case signalR.HubConnectionState.Disconnecting:
        return 'Disconnecting';
      case signalR.HubConnectionState.Reconnecting:
        return 'Reconnecting';
      default:
        return 'Disconnected';
    }
  }, []);

  const connectionState = getConnectionState();

  return {
    connectionState,
    isConnected: connectionState === 'Connected',
    isReconnecting: connectionState === 'Reconnecting',
  };
}

export default useSignalR;
