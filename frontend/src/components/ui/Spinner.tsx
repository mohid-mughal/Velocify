import React from 'react';
import { clsx } from 'clsx';

export interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg' | 'xl';
  color?: 'primary' | 'secondary' | 'white';
  className?: string;
}

export const Spinner: React.FC<SpinnerProps> = ({
  size = 'md',
  color = 'primary',
  className,
}) => {
  const sizeStyles = {
    sm: 'w-4 h-4',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
    xl: 'w-16 h-16',
  };

  const colorStyles = {
    primary: 'text-primary-600',
    secondary: 'text-secondary-600',
    white: 'text-white',
  };

  return (
    <svg
      className={clsx(
        'animate-spin',
        sizeStyles[size],
        colorStyles[color],
        className
      )}
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
      role="status"
      aria-label="Loading"
    >
      <circle
        className="opacity-25"
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        strokeWidth="4"
      />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
      />
    </svg>
  );
};

export interface LoadingOverlayProps {
  isLoading: boolean;
  message?: string;
  children?: React.ReactNode;
}

export const LoadingOverlay: React.FC<LoadingOverlayProps> = ({
  isLoading,
  message,
  children,
}) => {
  if (!isLoading) return <>{children}</>;

  return (
    <div className="relative">
      {children && <div className="opacity-50 pointer-events-none">{children}</div>}
      <div className="absolute inset-0 flex flex-col items-center justify-center bg-white bg-opacity-75">
        <Spinner size="lg" />
        {message && (
          <p className="mt-4 text-sm font-medium text-neutral-700">{message}</p>
        )}
      </div>
    </div>
  );
};
