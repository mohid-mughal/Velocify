import React, { useState, KeyboardEvent } from 'react';
import { clsx } from 'clsx';

export interface TagInputProps {
  name: string;
  label?: string;
  value: string[];
  onChange: (value: string[]) => void;
  error?: string;
  helperText?: string;
  placeholder?: string;
  disabled?: boolean;
  fullWidth?: boolean;
  maxTags?: number;
  allowDuplicates?: boolean;
}

/**
 * TagInput component allows users to input multiple tags/chips
 * Tags are added by pressing Enter or comma
 * 
 * @example
 * ```tsx
 * <TagInput
 *   name="tags"
 *   label="Tags"
 *   value={tags}
 *   onChange={setTags}
 *   placeholder="Type and press Enter..."
 *   maxTags={10}
 * />
 * ```
 */
export const TagInput: React.FC<TagInputProps> = ({
  name,
  label,
  value,
  onChange,
  error,
  helperText,
  placeholder = 'Type and press Enter...',
  disabled = false,
  fullWidth = false,
  maxTags,
  allowDuplicates = false,
}) => {
  const [inputValue, setInputValue] = useState('');
  const inputId = `taginput-${name}`;
  const errorId = error ? `${inputId}-error` : undefined;
  const helperId = helperText ? `${inputId}-helper` : undefined;

  const addTag = (tag: string) => {
    const trimmedTag = tag.trim();
    
    if (!trimmedTag) return;
    
    if (maxTags && value.length >= maxTags) {
      return;
    }
    
    if (!allowDuplicates && value.includes(trimmedTag)) {
      return;
    }
    
    onChange([...value, trimmedTag]);
    setInputValue('');
  };

  const removeTag = (indexToRemove: number) => {
    onChange(value.filter((_, index) => index !== indexToRemove));
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' || e.key === ',') {
      e.preventDefault();
      addTag(inputValue);
    } else if (e.key === 'Backspace' && !inputValue && value.length > 0) {
      // Remove last tag when backspace is pressed on empty input
      removeTag(value.length - 1);
    }
  };

  const handleBlur = () => {
    // Add tag on blur if there's input
    if (inputValue.trim()) {
      addTag(inputValue);
    }
  };

  const isMaxReached = maxTags ? value.length >= maxTags : false;

  return (
    <div className={clsx('flex flex-col gap-1', fullWidth && 'w-full')}>
      {label && (
        <label htmlFor={inputId} className="text-sm font-medium text-neutral-700">
          {label}
          {maxTags && (
            <span className="ml-2 text-xs text-neutral-500">
              ({value.length}/{maxTags})
            </span>
          )}
        </label>
      )}

      <div
        className={clsx(
          'min-h-[42px] px-3 py-2 border rounded-md transition-colors',
          'focus-within:ring-2 focus-within:ring-offset-1',
          'disabled:bg-neutral-100',
          error
            ? 'border-danger-500 focus-within:ring-danger-500'
            : 'border-neutral-300 focus-within:ring-primary-500',
          'bg-white'
        )}
      >
        <div className="flex flex-wrap gap-1 items-center">
          {value.map((tag, index) => (
            <span
              key={index}
              className="inline-flex items-center gap-1 px-2 py-1 bg-primary-100 text-primary-800 rounded text-sm"
            >
              {tag}
              <button
                type="button"
                onClick={() => removeTag(index)}
                disabled={disabled}
                className="hover:text-primary-900 focus:outline-none focus:ring-1 focus:ring-primary-500 rounded"
                aria-label={`Remove tag ${tag}`}
              >
                <svg
                  className="w-3 h-3"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </button>
            </span>
          ))}
          
          {!isMaxReached && (
            <input
              type="text"
              id={inputId}
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyDown={handleKeyDown}
              onBlur={handleBlur}
              disabled={disabled}
              placeholder={value.length === 0 ? placeholder : ''}
              aria-describedby={clsx(errorId, helperId).trim() || undefined}
              className="flex-1 min-w-[120px] outline-none bg-transparent text-base disabled:cursor-not-allowed"
            />
          )}
        </div>
      </div>

      {error && (
        <span id={errorId} className="text-sm text-danger-600" role="alert">
          {error}
        </span>
      )}
      {helperText && !error && (
        <span id={helperId} className="text-sm text-neutral-500">
          {helperText}
        </span>
      )}
    </div>
  );
};

TagInput.displayName = 'TagInput';
