import React, { useState, useRef, useEffect } from 'react';
import { clsx } from 'clsx';

export interface MultiSelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface MultiSelectProps {
  name: string;
  label?: string;
  options: MultiSelectOption[];
  value: string[];
  onChange: (value: string[]) => void;
  error?: string;
  helperText?: string;
  placeholder?: string;
  disabled?: boolean;
  fullWidth?: boolean;
}

/**
 * MultiSelect component allows selecting multiple options from a dropdown
 * Displays selected items as badges with remove buttons
 * 
 * @example
 * ```tsx
 * <MultiSelect
 *   name="categories"
 *   label="Categories"
 *   options={categoryOptions}
 *   value={selectedCategories}
 *   onChange={setSelectedCategories}
 *   placeholder="Select categories..."
 * />
 * ```
 */
export const MultiSelect: React.FC<MultiSelectProps> = ({
  name,
  label,
  options,
  value,
  onChange,
  error,
  helperText,
  placeholder = 'Select options...',
  disabled = false,
  fullWidth = false,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const selectId = `multiselect-${name}`;
  const errorId = error ? `${selectId}-error` : undefined;
  const helperId = helperText ? `${selectId}-helper` : undefined;

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      return () => document.removeEventListener('mousedown', handleClickOutside);
    }
  }, [isOpen]);

  const handleToggleOption = (optionValue: string) => {
    if (value.includes(optionValue)) {
      onChange(value.filter((v) => v !== optionValue));
    } else {
      onChange([...value, optionValue]);
    }
  };

  const handleRemoveOption = (optionValue: string, e: React.MouseEvent) => {
    e.stopPropagation();
    onChange(value.filter((v) => v !== optionValue));
  };

  const selectedOptions = options.filter((opt) => value.includes(opt.value));
  const availableOptions = options.filter((opt) => !opt.disabled);

  return (
    <div className={clsx('flex flex-col gap-1', fullWidth && 'w-full')} ref={containerRef}>
      {label && (
        <label htmlFor={selectId} className="text-sm font-medium text-neutral-700">
          {label}
        </label>
      )}

      <div className="relative">
        <button
          type="button"
          id={selectId}
          onClick={() => !disabled && setIsOpen(!isOpen)}
          disabled={disabled}
          aria-expanded={isOpen}
          aria-haspopup="listbox"
          aria-describedby={clsx(errorId, helperId).trim() || undefined}
          className={clsx(
            'w-full min-h-[42px] px-3 py-2 border rounded-md text-base transition-colors text-left',
            'focus:outline-none focus:ring-2 focus:ring-offset-1',
            'disabled:bg-neutral-100 disabled:cursor-not-allowed',
            error
              ? 'border-danger-500 focus:ring-danger-500'
              : 'border-neutral-300 focus:ring-primary-500',
            'bg-white'
          )}
        >
          {selectedOptions.length === 0 ? (
            <span className="text-neutral-400">{placeholder}</span>
          ) : (
            <div className="flex flex-wrap gap-1">
              {selectedOptions.map((option) => (
                <span
                  key={option.value}
                  className="inline-flex items-center gap-1 px-2 py-0.5 bg-primary-100 text-primary-800 rounded text-sm"
                >
                  {option.label}
                  <button
                    type="button"
                    onClick={(e) => handleRemoveOption(option.value, e)}
                    disabled={disabled}
                    className="hover:text-primary-900 focus:outline-none"
                    aria-label={`Remove ${option.label}`}
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
            </div>
          )}
        </button>

        {isOpen && (
          <div
            className="absolute z-10 w-full mt-1 bg-white border border-neutral-300 rounded-md shadow-lg max-h-60 overflow-auto"
            role="listbox"
          >
            {availableOptions.length === 0 ? (
              <div className="px-3 py-2 text-sm text-neutral-500">No options available</div>
            ) : (
              availableOptions.map((option) => {
                const isSelected = value.includes(option.value);
                return (
                  <button
                    key={option.value}
                    type="button"
                    onClick={() => handleToggleOption(option.value)}
                    role="option"
                    aria-selected={isSelected}
                    className={clsx(
                      'w-full px-3 py-2 text-left text-sm transition-colors',
                      'hover:bg-neutral-100 focus:bg-neutral-100 focus:outline-none',
                      isSelected && 'bg-primary-50 text-primary-900 font-medium'
                    )}
                  >
                    <div className="flex items-center justify-between">
                      <span>{option.label}</span>
                      {isSelected && (
                        <svg
                          className="w-4 h-4 text-primary-600"
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M5 13l4 4L19 7"
                          />
                        </svg>
                      )}
                    </div>
                  </button>
                );
              })
            )}
          </div>
        )}
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

MultiSelect.displayName = 'MultiSelect';
