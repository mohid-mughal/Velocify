/**
 * Natural Language Input Component
 * 
 * Requirements: 24.1, 24.2
 * 
 * Provides a textarea for natural language task input with AI parsing
 */

import React, { useState } from 'react';
import { clsx } from 'clsx';
import { Button } from '../ui/Button';

export interface NaturalLanguageInputProps {
  onParse: (input: string) => Promise<void>;
  disabled?: boolean;
  error?: string;
}

/**
 * NaturalLanguageInput component
 * 
 * Requirement 24.1: Natural Language mode with textarea + Parse button
 * Requirement 24.2: AI parses input and populates form fields
 * 
 * @example
 * ```tsx
 * <NaturalLanguageInput
 *   onParse={handleParse}
 *   disabled={isParsing}
 *   error={parseError}
 * />
 * ```
 */
export const NaturalLanguageInput: React.FC<NaturalLanguageInputProps> = ({
  onParse,
  disabled = false,
  error,
}) => {
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleParse = async () => {
    if (!input.trim()) return;

    setIsLoading(true);
    try {
      await onParse(input);
      // Clear input on successful parse
      setInput('');
    } catch (err) {
      // Error is handled by parent component
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    // Allow Ctrl+Enter or Cmd+Enter to trigger parse
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
      e.preventDefault();
      handleParse();
    }
  };

  return (
    <div className="space-y-3">
      <div className="flex flex-col gap-1">
        <label
          htmlFor="natural-language-input"
          className="text-sm font-medium text-neutral-700"
        >
          Describe your task in natural language
        </label>
        <textarea
          id="natural-language-input"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={disabled || isLoading}
          placeholder="Example: Create a high priority design task for Sarah to redesign the login page by next Friday, estimated 8 hours"
          rows={4}
          className={clsx(
            'px-3 py-2 border rounded-md text-base transition-colors resize-y',
            'focus:outline-none focus:ring-2 focus:ring-offset-1',
            'disabled:bg-neutral-100 disabled:cursor-not-allowed',
            error
              ? 'border-danger-500 focus:ring-danger-500'
              : 'border-neutral-300 focus:ring-primary-500'
          )}
        />
        <span className="text-xs text-neutral-500">
          Tip: Press Ctrl+Enter (Cmd+Enter on Mac) to parse
        </span>
      </div>

      {error && (
        <div className="text-sm text-danger-600" role="alert">
          {error}
        </div>
      )}

      <Button
        type="button"
        onClick={handleParse}
        disabled={!input.trim() || disabled || isLoading}
        isLoading={isLoading}
        variant="primary"
      >
        {isLoading ? 'Parsing...' : 'Parse with AI'}
      </Button>

      <div className="text-xs text-neutral-500 space-y-1">
        <p className="font-medium">The AI can extract:</p>
        <ul className="list-disc list-inside space-y-0.5 ml-2">
          <li>Task title and description</li>
          <li>Priority (critical, high, medium, low)</li>
          <li>Category (development, design, marketing, etc.)</li>
          <li>Assignee (by name or email)</li>
          <li>Due date (relative or absolute dates)</li>
          <li>Estimated hours</li>
        </ul>
      </div>
    </div>
  );
};

NaturalLanguageInput.displayName = 'NaturalLanguageInput';
