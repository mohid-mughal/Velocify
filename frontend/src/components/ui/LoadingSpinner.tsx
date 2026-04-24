/**
 * Loading Spinner Component
 * 
 * Task: 28.3
 * Requirements: All UI requirements
 * 
 * Reusable loading spinner component with size variants
 */

import { cn } from '../../utils/helpers';

interface LoadingSpinnerProps {
  /**
   * Size variant
   */
  size?: 'sm' | 'md' | 'lg' | 'xl';
  
  /**
   * Custom className
   */
  className?: string;
  
  /**
   * Color variant
   */
  color?: 'primary' | 'white' | 'neutral';
}

/**
 * LoadingSpinner Component
 * 
 * Displays an animated loading spinner.
 * 
 * Usage:
 * ```tsx
 * <LoadingSpinner size="lg" />
 * <LoadingSpinner size="sm" color="white" />
 * ```
 */
export function LoadingSpinner({
  size = 'md',
  className,
  color = 'primary',
}: LoadingSpinnerProps) {
  const sizeClasses = {
    sm: 'w-4 h-4 border-2',
    md: 'w-6 h-6 border-2',
    lg: 'w-8 h-8 border-3',
    xl: 'w-12 h-12 border-4',
  };

  const colorClasses = {
    primary: 'border-primary-600 border-t-transparent',
    white: 'border-white border-t-transparent',
    neutral: 'border-neutral-600 border-t-transparent',
  };

  return (
    <div
      className={cn(
        'inline-block rounded-full animate-spin',
        sizeClasses[size],
        colorClasses[color],
        className
      )}
      role="status"
      aria-label="Loading"
    >
      <span className="sr-only">Loading...</span>
    </div>
  );
}
