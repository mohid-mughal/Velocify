import React from 'react';
import { Input, InputProps } from '../ui/Input';

export interface FormFieldProps extends Omit<InputProps, 'error'> {
  name?: string;
  label: string;
  error?: string;
  required?: boolean;
}

/**
 * FormField component wraps an Input with label and error display
 * Designed for use with React Hook Form
 * 
 * @example
 * ```tsx
 * <FormField
 *   name="title"
 *   label="Task Title"
 *   error={errors.title?.message}
 *   required
 *   {...register('title')}
 * />
 * ```
 */
export const FormField = React.forwardRef<HTMLInputElement, FormFieldProps>(
  ({ name, label, error, required, ...props }, ref) => {
    return (
      <Input
        ref={ref}
        id={name}
        name={name}
        label={label}
        error={error}
        aria-required={required}
        fullWidth
        {...props}
      />
    );
  }
);

FormField.displayName = 'FormField';
