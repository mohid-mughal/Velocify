import React from 'react';
import { clsx } from 'clsx';

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  helperText?: string;
  fullWidth?: boolean;
  options: SelectOption[];
  placeholder?: string;
}

export const Select = React.forwardRef<HTMLSelectElement, SelectProps>(
  (
    {
      label,
      error,
      helperText,
      fullWidth = false,
      options,
      placeholder,
      className,
      id,
      ...props
    },
    ref
  ) => {
    const selectId = id || `select-${Math.random().toString(36).substr(2, 9)}`;
    const errorId = error ? `${selectId}-error` : undefined;
    const helperId = helperText ? `${selectId}-helper` : undefined;

    return (
      <div className={clsx('flex flex-col gap-1', fullWidth && 'w-full')}>
        {label && (
          <label
            htmlFor={selectId}
            className="text-sm font-medium text-neutral-700"
          >
            {label}
          </label>
        )}
        <select
          ref={ref}
          id={selectId}
          aria-invalid={error ? 'true' : 'false'}
          aria-describedby={clsx(errorId, helperId).trim() || undefined}
          className={clsx(
            'px-3 py-2 border rounded-md text-base transition-colors',
            'focus:outline-none focus:ring-2 focus:ring-offset-1',
            'disabled:bg-neutral-100 disabled:cursor-not-allowed',
            'bg-white',
            error
              ? 'border-danger-500 focus:ring-danger-500'
              : 'border-neutral-300 focus:ring-primary-500',
            className
          )}
          {...props}
        >
          {placeholder && (
            <option value="" disabled>
              {placeholder}
            </option>
          )}
          {options.map((option) => (
            <option
              key={option.value}
              value={option.value}
              disabled={option.disabled}
            >
              {option.label}
            </option>
          ))}
        </select>
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

Select.displayName = 'Select';
