/**
 * Error Boundary Component
 * 
 * Task: 28.3
 * Requirements: 21.4 - Global error handling
 * 
 * Catches JavaScript errors anywhere in the component tree and displays
 * a fallback UI instead of crashing the entire application.
 */

import { Component, ErrorInfo, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

/**
 * ErrorBoundary Component
 * 
 * Catches errors in child components and displays a fallback UI.
 * Logs error details to console for debugging.
 * 
 * Usage:
 * ```tsx
 * <ErrorBoundary>
 *   <App />
 * </ErrorBoundary>
 * ```
 * 
 * Requirements:
 * - 21.4: Global error handling for unhandled exceptions
 * - 19.1: Consistent error display
 */
export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
    };
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    // Update state so the next render will show the fallback UI
    return {
      hasError: true,
      error,
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    // Log error details to console
    console.error('ErrorBoundary caught an error:', error, errorInfo);

    // Update state with error info
    this.setState({
      error,
      errorInfo,
    });

    // In production, you might want to log to an error reporting service
    // Example: logErrorToService(error, errorInfo);
  }

  handleReset = (): void => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
    });
  };

  render(): ReactNode {
    if (this.state.hasError) {
      // Custom fallback UI provided
      if (this.props.fallback) {
        return this.props.fallback;
      }

      // Default fallback UI
      return (
        <div className="min-h-screen bg-neutral-50 flex items-center justify-center p-4">
          <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-6">
            <div className="flex items-center justify-center w-12 h-12 mx-auto bg-danger-100 rounded-full">
              <svg
                className="w-6 h-6 text-danger-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
            </div>

            <h1 className="mt-4 text-xl font-semibold text-center text-neutral-900">
              Something went wrong
            </h1>

            <p className="mt-2 text-sm text-center text-neutral-600">
              We're sorry, but something unexpected happened. Please try refreshing the page.
            </p>

            {/* Show error details in development */}
            {import.meta.env.DEV && this.state.error && (
              <details className="mt-4 p-4 bg-neutral-50 rounded-md">
                <summary className="text-sm font-medium text-neutral-700 cursor-pointer">
                  Error Details (Development Only)
                </summary>
                <div className="mt-2 text-xs text-neutral-600 font-mono">
                  <p className="font-semibold text-danger-600">
                    {this.state.error.toString()}
                  </p>
                  {this.state.errorInfo && (
                    <pre className="mt-2 overflow-x-auto whitespace-pre-wrap">
                      {this.state.errorInfo.componentStack}
                    </pre>
                  )}
                </div>
              </details>
            )}

            <div className="mt-6 flex gap-3">
              <button
                onClick={this.handleReset}
                className="flex-1 btn btn-outline"
              >
                Try Again
              </button>
              <button
                onClick={() => window.location.href = '/'}
                className="flex-1 btn btn-primary"
              >
                Go Home
              </button>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
