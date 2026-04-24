import React from 'react';
import { clsx } from 'clsx';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
  fullWidth?: boolean;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      label,
      error,
      helperText,
      fullWidth = false,
      className,
      id,
      ...props
    },
    ref
  ) => {
    const inputId = id || `input-${Math.random().toString(36).substr(2, 9)}`;
    const errorId = error ? `${inputId}-error` : undefined;
    const helperId = helperText ? `${inputId}-helper` : undefined;

    return (
      <div className={clsx('flex flex-col gap-1', fullWidth && 'w-full')}>
        {label && (
          <label
            htmlFor={inputId}
            className="text-sm font-medium text-neutral-700"
          >
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          aria-invalid={error ? 'true' : 'false'}
          aria-describedby={clsx(errorId, helperId).trim() || undefined}
          className={clsx(
            'px-3 py-2 border rounded-md text-base transition-colors',
            'focus:outline-none focus:ring-2 focus:ring-offset-1',
            'disabled:bg-neutral-100 disabled:cursor-not-allowed',
            error
              ? 'border-danger-500 focus:ring-danger-500'
              : 'border-neutral-300 focus:ring-primary-500',
            className
          )}
          {...props}
        />
        {error && (
          <span
            id={errorId}
            className="text-sm text-danger-600"
            role="alert"
          >
            {error}
          </span>
        )}
        {helperText && !error && (
          <span
            id={helperId}
            className="text-sm text-neutral-500"
          >
            {helperText}
          </span>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';
