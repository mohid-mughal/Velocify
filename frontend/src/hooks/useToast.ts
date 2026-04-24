/**
 * Toast Notification Hook
 * 
 * Custom hook for displaying toast notifications.
 * Provides success, error, warning, and info toast types.
 */

import { useState, useCallback } from 'react';

/**
 * Toast type variants
 */
export type ToastType = 'success' | 'error' | 'warning' | 'info';

/**
 * Toast notification item
 */
export interface Toast {
  /** Unique identifier for the toast */
  id: string;
  /** Type of toast (determines styling) */
  type: ToastType;
  /** Toast title */
  title: string;
  /** Optional toast message */
  message?: string;
  /** Duration in milliseconds (default: 5000) */
  duration?: number;
  /** Whether the toast can be dismissed */
  dismissible?: boolean;
}

/**
 * Options for creating a toast
 */
export interface ToastOptions {
  /** Toast title */
  title: string;
  /** Optional toast message */
  message?: string;
  /** Duration in milliseconds (default: 5000, set to 0 for persistent) */
  duration?: number;
  /** Whether the toast can be dismissed (default: true) */
  dismissible?: boolean;
}

/**
 * Toast store state
 */
interface ToastState {
  toasts: Toast[];
}

/**
 * Generate a unique ID for toasts
 */
const generateId = (): string => {
  return `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
};

/**
 * Hook to manage toast notifications
 * 
 * Provides methods to show different types of toasts and manage the toast queue.
 * 
 * @returns Object with toasts array and methods to show/dismiss toasts
 * 
 * @example
 * const { toasts, showSuccess, showError, dismiss } = useToast();
 * 
 * // Show a success toast
 * showSuccess({ title: 'Task created', message: 'Your task has been created successfully.' });
 * 
 * // Show an error toast
 * showError({ title: 'Error', message: 'Failed to create task.' });
 * 
 * // Render toasts
 * return (
 *   <div className="toast-container">
 *     {toasts.map(toast => (
 *       <div key={toast.id} className={`toast toast-${toast.type}`}>
 *         <strong>{toast.title}</strong>
 *         {toast.message && <p>{toast.message}</p>}
 *         <button onClick={() => dismiss(toast.id)}>×</button>
 *       </div>
 *     ))}
 *   </div>
 * );
 */
export function useToast() {
  const [state, setState] = useState<ToastState>({ toasts: [] });

  /**
   * Add a toast to the queue
   */
  const addToast = useCallback(
    (type: ToastType, options: ToastOptions): string => {
      const id = generateId();
      const duration = options.duration ?? 5000;
      const dismissible = options.dismissible ?? true;

      const toast: Toast = {
        id,
        type,
        title: options.title,
        message: options.message,
        duration,
        dismissible,
      };

      setState((prev) => ({
        toasts: [...prev.toasts, toast],
      }));

      // Auto-dismiss after duration (if duration > 0)
      if (duration > 0) {
        setTimeout(() => {
          dismiss(id);
        }, duration);
      }

      return id;
    },
    []
  );

  /**
   * Dismiss a toast by ID
   */
  const dismiss = useCallback((id: string) => {
    setState((prev) => ({
      toasts: prev.toasts.filter((toast) => toast.id !== id),
    }));
  }, []);

  /**
   * Dismiss all toasts
   */
  const dismissAll = useCallback(() => {
    setState({ toasts: [] });
  }, []);

  /**
   * Show a success toast
   */
  const showSuccess = useCallback(
    (options: ToastOptions): string => addToast('success', options),
    [addToast]
  );

  /**
   * Show an error toast
   */
  const showError = useCallback(
    (options: ToastOptions): string => addToast('error', options),
    [addToast]
  );

  /**
   * Show a warning toast
   */
  const showWarning = useCallback(
    (options: ToastOptions): string => addToast('warning', options),
    [addToast]
  );

  /**
   * Show an info toast
   */
  const showInfo = useCallback(
    (options: ToastOptions): string => addToast('info', options),
    [addToast]
  );

  return {
    /** Current toasts in the queue */
    toasts: state.toasts,
    /** Show a success toast */
    showSuccess,
    /** Show an error toast */
    showError,
    /** Show a warning toast */
    showWarning,
    /** Show an info toast */
    showInfo,
    /** Dismiss a specific toast by ID */
    dismiss,
    /** Dismiss all toasts */
    dismissAll,
  };
}

/**
 * Toast type for use without the hook (e.g., in components)
 */
export type ToastHookReturn = ReturnType<typeof useToast>;
