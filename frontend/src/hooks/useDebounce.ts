/**
 * Debounce Hook
 * 
 * Requirements: 22.5
 * 
 * Custom hook for debouncing values with configurable delay.
 * Used primarily for search input to avoid excessive API calls.
 */

import { useState, useEffect } from 'react';

/**
 * Hook to debounce a value with configurable delay
 * 
 * Requirement 22.5: WHEN a user types in the search input THEN the Frontend 
 * SHALL debounce the input and trigger search after 300ms
 * 
 * @param value - The value to debounce
 * @param delay - Delay in milliseconds (default: 300ms for search)
 * @returns The debounced value
 * 
 * @example
 * const [searchTerm, setSearchTerm] = useState('');
 * const debouncedSearch = useDebounce(searchTerm, 300);
 * 
 * useEffect(() => {
 *   if (debouncedSearch) {
 *     // Trigger search API call
 *     searchTasks(debouncedSearch);
 *   }
 * }, [debouncedSearch]);
 */
export function useDebounce<T>(value: T, delay: number = 300): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    // Set up a timer to update the debounced value after the delay
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    // Clean up the timer if value changes or component unmounts
    // This ensures only the latest value is debounced
    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
}
