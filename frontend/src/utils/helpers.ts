/**
 * Helper Utilities
 * 
 * Task: 28.1
 * Requirements: All frontend requirements
 * 
 * General utility functions for common operations
 */

import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';
import type { TaskStatus, TaskPriority, TaskCategory, UserRole } from '../api/types';

// ============== Class Name Utilities ==============

/**
 * Merge Tailwind CSS classes with clsx and tailwind-merge
 * Handles conditional classes and resolves conflicts
 * 
 * @param inputs - Class values to merge
 * @returns Merged class string
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}

// ============== Array Utilities ==============

/**
 * Remove duplicates from an array
 * 
 * @param array - Array with potential duplicates
 * @returns Array with unique values
 */
export function unique<T>(array: T[]): T[] {
  return Array.from(new Set(array));
}

/**
 * Group array items by a key
 * 
 * @param array - Array to group
 * @param keyFn - Function to extract grouping key
 * @returns Object with grouped items
 */
export function groupBy<T, K extends string | number>(
  array: T[],
  keyFn: (item: T) => K
): Record<K, T[]> {
  return array.reduce((result, item) => {
    const key = keyFn(item);
    if (!result[key]) {
      result[key] = [];
    }
    result[key].push(item);
    return result;
  }, {} as Record<K, T[]>);
}

/**
 * Sort array by a key
 * 
 * @param array - Array to sort
 * @param keyFn - Function to extract sort key
 * @param order - Sort order ('asc' or 'desc')
 * @returns Sorted array
 */
export function sortBy<T>(
  array: T[],
  keyFn: (item: T) => string | number | Date,
  order: 'asc' | 'desc' = 'asc'
): T[] {
  return [...array].sort((a, b) => {
    const aVal = keyFn(a);
    const bVal = keyFn(b);
    
    if (aVal < bVal) return order === 'asc' ? -1 : 1;
    if (aVal > bVal) return order === 'asc' ? 1 : -1;
    return 0;
  });
}

/**
 * Chunk array into smaller arrays
 * 
 * @param array - Array to chunk
 * @param size - Chunk size
 * @returns Array of chunks
 */
export function chunk<T>(array: T[], size: number): T[][] {
  const chunks: T[][] = [];
  for (let i = 0; i < array.length; i += size) {
    chunks.push(array.slice(i, i + size));
  }
  return chunks;
}

// ============== Object Utilities ==============

/**
 * Deep clone an object
 * 
 * @param obj - Object to clone
 * @returns Cloned object
 */
export function deepClone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj));
}

/**
 * Pick specific keys from an object
 * 
 * @param obj - Source object
 * @param keys - Keys to pick
 * @returns New object with picked keys
 */
export function pick<T extends object, K extends keyof T>(obj: T, keys: K[]): Pick<T, K> {
  const result = {} as Pick<T, K>;
  keys.forEach(key => {
    if (key in obj) {
      result[key] = obj[key];
    }
  });
  return result;
}

/**
 * Omit specific keys from an object
 * 
 * @param obj - Source object
 * @param keys - Keys to omit
 * @returns New object without omitted keys
 */
export function omit<T extends object, K extends keyof T>(obj: T, keys: K[]): Omit<T, K> {
  const result = { ...obj };
  keys.forEach(key => {
    delete result[key];
  });
  return result;
}

/**
 * Check if object is empty
 * 
 * @param obj - Object to check
 * @returns True if object has no keys
 */
export function isEmpty(obj: object): boolean {
  return Object.keys(obj).length === 0;
}

// ============== String Utilities ==============

/**
 * Capitalize first letter of a string
 * 
 * @param str - String to capitalize
 * @returns Capitalized string
 */
export function capitalize(str: string): string {
  if (!str) return '';
  return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
}

/**
 * Convert string to kebab-case
 * 
 * @param str - String to convert
 * @returns Kebab-cased string
 */
export function toKebabCase(str: string): string {
  return str
    .replace(/([a-z])([A-Z])/g, '$1-$2')
    .replace(/[\s_]+/g, '-')
    .toLowerCase();
}

/**
 * Convert string to camelCase
 * 
 * @param str - String to convert
 * @returns CamelCased string
 */
export function toCamelCase(str: string): string {
  return str
    .replace(/[-_\s]+(.)?/g, (_, char) => (char ? char.toUpperCase() : ''))
    .replace(/^(.)/, (char) => char.toLowerCase());
}

/**
 * Generate a random string
 * 
 * @param length - Length of random string
 * @returns Random string
 */
export function randomString(length: number = 10): string {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let result = '';
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
}

// ============== URL Utilities ==============

/**
 * Build URL with query parameters
 * 
 * @param baseUrl - Base URL
 * @param params - Query parameters object
 * @returns URL with query string
 */
export function buildUrl(baseUrl: string, params: Record<string, any>): string {
  const url = new URL(baseUrl, window.location.origin);
  
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      if (Array.isArray(value)) {
        value.forEach(v => url.searchParams.append(key, String(v)));
      } else {
        url.searchParams.append(key, String(value));
      }
    }
  });
  
  return url.toString();
}

/**
 * Parse query string to object
 * 
 * @param queryString - Query string (with or without leading '?')
 * @returns Object with query parameters
 */
export function parseQueryString(queryString: string): Record<string, string> {
  const params = new URLSearchParams(queryString.startsWith('?') ? queryString.slice(1) : queryString);
  const result: Record<string, string> = {};
  
  params.forEach((value, key) => {
    result[key] = value;
  });
  
  return result;
}

// ============== Local Storage Utilities ==============

/**
 * Get item from local storage with JSON parsing
 * 
 * @param key - Storage key
 * @param defaultValue - Default value if key doesn't exist
 * @returns Parsed value or default
 */
export function getStorageItem<T>(key: string, defaultValue: T): T {
  try {
    const item = localStorage.getItem(key);
    return item ? JSON.parse(item) : defaultValue;
  } catch (error) {
    console.error(`Error reading from localStorage key "${key}":`, error);
    return defaultValue;
  }
}

/**
 * Set item in local storage with JSON stringification
 * 
 * @param key - Storage key
 * @param value - Value to store
 */
export function setStorageItem<T>(key: string, value: T): void {
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch (error) {
    console.error(`Error writing to localStorage key "${key}":`, error);
  }
}

/**
 * Remove item from local storage
 * 
 * @param key - Storage key
 */
export function removeStorageItem(key: string): void {
  try {
    localStorage.removeItem(key);
  } catch (error) {
    console.error(`Error removing from localStorage key "${key}":`, error);
  }
}

// ============== Debounce and Throttle ==============

/**
 * Debounce a function
 * 
 * @param fn - Function to debounce
 * @param delay - Delay in milliseconds
 * @returns Debounced function
 */
export function debounce<T extends (...args: any[]) => any>(
  fn: T,
  delay: number
): (...args: Parameters<T>) => void {
  let timeoutId: ReturnType<typeof setTimeout>;
  
  return function (this: any, ...args: Parameters<T>) {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => fn.apply(this, args), delay);
  };
}

/**
 * Throttle a function
 * 
 * @param fn - Function to throttle
 * @param limit - Time limit in milliseconds
 * @returns Throttled function
 */
export function throttle<T extends (...args: any[]) => any>(
  fn: T,
  limit: number
): (...args: Parameters<T>) => void {
  let inThrottle: boolean;
  
  return function (this: any, ...args: Parameters<T>) {
    if (!inThrottle) {
      fn.apply(this, args);
      inThrottle = true;
      setTimeout(() => (inThrottle = false), limit);
    }
  };
}

// ============== Async Utilities ==============

/**
 * Sleep for specified milliseconds
 * 
 * @param ms - Milliseconds to sleep
 * @returns Promise that resolves after delay
 */
export function sleep(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Retry an async function with exponential backoff
 * 
 * @param fn - Async function to retry
 * @param maxAttempts - Maximum retry attempts
 * @param delay - Initial delay in milliseconds
 * @returns Promise with function result
 */
export async function retry<T>(
  fn: () => Promise<T>,
  maxAttempts: number = 3,
  delay: number = 1000
): Promise<T> {
  let lastError: Error;
  
  for (let attempt = 1; attempt <= maxAttempts; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error as Error;
      
      if (attempt < maxAttempts) {
        await sleep(delay * Math.pow(2, attempt - 1)); // Exponential backoff
      }
    }
  }
  
  throw lastError!;
}

// ============== Color Utilities ==============

/**
 * Generate a consistent color from a string (for avatars, etc.)
 * 
 * @param str - Input string
 * @returns Hex color code
 */
export function stringToColor(str: string): string {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  
  const color = Math.abs(hash).toString(16).substring(0, 6);
  return `#${'0'.repeat(6 - color.length)}${color}`;
}

/**
 * Check if a color is light or dark
 * 
 * @param hexColor - Hex color code
 * @returns True if color is light
 */
export function isLightColor(hexColor: string): boolean {
  const hex = hexColor.replace('#', '');
  const r = parseInt(hex.substring(0, 2), 16);
  const g = parseInt(hex.substring(2, 4), 16);
  const b = parseInt(hex.substring(4, 6), 16);
  
  // Calculate relative luminance
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  
  return luminance > 0.5;
}

// ============== Clipboard Utilities ==============

/**
 * Copy text to clipboard
 * 
 * @param text - Text to copy
 * @returns Promise that resolves when copied
 */
export async function copyToClipboard(text: string): Promise<void> {
  try {
    await navigator.clipboard.writeText(text);
  } catch (error) {
    // Fallback for older browsers
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.style.position = 'fixed';
    textarea.style.opacity = '0';
    document.body.appendChild(textarea);
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
  }
}

// ============== Download Utilities ==============

/**
 * Download data as a file
 * 
 * @param data - Data to download
 * @param filename - File name
 * @param mimeType - MIME type
 */
export function downloadFile(data: string | Blob, filename: string, mimeType: string = 'text/plain'): void {
  const blob = data instanceof Blob ? data : new Blob([data], { type: mimeType });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

// ============== Enum Utilities ==============

/**
 * Get all enum values as an array
 * 
 * @param enumObj - Enum object
 * @returns Array of enum values
 */
export function getEnumValues<T extends Record<string, string>>(enumObj: T): T[keyof T][] {
  return Object.values(enumObj) as T[keyof T][];
}

/**
 * Get task status options for select inputs
 * 
 * @returns Array of status options
 */
export function getTaskStatusOptions(): { value: TaskStatus; label: string }[] {
  return [
    { value: 'Pending', label: 'Pending' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Cancelled', label: 'Cancelled' },
    { value: 'Blocked', label: 'Blocked' },
  ];
}

/**
 * Get task priority options for select inputs
 * 
 * @returns Array of priority options
 */
export function getTaskPriorityOptions(): { value: TaskPriority; label: string }[] {
  return [
    { value: 'Critical', label: 'Critical' },
    { value: 'High', label: 'High' },
    { value: 'Medium', label: 'Medium' },
    { value: 'Low', label: 'Low' },
  ];
}

/**
 * Get task category options for select inputs
 * 
 * @returns Array of category options
 */
export function getTaskCategoryOptions(): { value: TaskCategory; label: string }[] {
  return [
    { value: 'Development', label: 'Development' },
    { value: 'Design', label: 'Design' },
    { value: 'Marketing', label: 'Marketing' },
    { value: 'Operations', label: 'Operations' },
    { value: 'Research', label: 'Research' },
    { value: 'Other', label: 'Other' },
  ];
}

/**
 * Get user role options for select inputs
 * 
 * @returns Array of role options
 */
export function getUserRoleOptions(): { value: UserRole; label: string }[] {
  return [
    { value: 'SuperAdmin', label: 'Super Admin' },
    { value: 'Admin', label: 'Admin' },
    { value: 'Member', label: 'Member' },
  ];
}

// ============== Error Handling Utilities ==============

/**
 * Extract error message from various error types
 * 
 * @param error - Error object
 * @returns Error message string
 */
export function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }
  
  if (typeof error === 'string') {
    return error;
  }
  
  if (error && typeof error === 'object' && 'message' in error) {
    return String(error.message);
  }
  
  return 'An unknown error occurred';
}

/**
 * Check if error is a network error
 * 
 * @param error - Error object
 * @returns True if network error
 */
export function isNetworkError(error: unknown): boolean {
  if (error instanceof Error) {
    return error.message.toLowerCase().includes('network') ||
           error.message.toLowerCase().includes('fetch');
  }
  return false;
}

// ============== Permission Utilities ==============

/**
 * Check if user has admin privileges
 * 
 * @param role - User role
 * @returns True if user is Admin or SuperAdmin
 */
export function isAdmin(role: UserRole): boolean {
  return role === 'Admin' || role === 'SuperAdmin';
}

/**
 * Check if user is SuperAdmin
 * 
 * @param role - User role
 * @returns True if user is SuperAdmin
 */
export function isSuperAdmin(role: UserRole): boolean {
  return role === 'SuperAdmin';
}

/**
 * Check if user can edit task
 * 
 * @param userRole - Current user's role
 * @param taskCreatorId - Task creator's ID
 * @param currentUserId - Current user's ID
 * @returns True if user can edit
 */
export function canEditTask(userRole: UserRole, taskCreatorId: string, currentUserId: string): boolean {
  return isAdmin(userRole) || taskCreatorId === currentUserId;
}

/**
 * Check if user can delete comment
 * 
 * @param userRole - Current user's role
 * @param commentUserId - Comment author's ID
 * @param currentUserId - Current user's ID
 * @returns True if user can delete
 */
export function canDeleteComment(userRole: UserRole, commentUserId: string, currentUserId: string): boolean {
  return isAdmin(userRole) || commentUserId === currentUserId;
}
