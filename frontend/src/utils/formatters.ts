/**
 * Formatting Utilities
 * 
 * Task: 28.1
 * Requirements: All frontend requirements
 * 
 * Functions for formatting dates, numbers, status values, and other display data
 */

import { format, formatDistanceToNow, isToday, isTomorrow, isYesterday, isPast, parseISO } from 'date-fns';
import {
  TASK_STATUS_LABELS,
  TASK_PRIORITY_LABELS,
  TASK_CATEGORY_LABELS,
  USER_ROLE_LABELS,
  NOTIFICATION_TYPE_LABELS,
  DATE_FORMATS,
  SENTIMENT_CONFIG,
} from './constants';
import type {
  TaskStatus,
  TaskPriority,
  TaskCategory,
  UserRole,
  NotificationType,
} from '../api/types';

// ============== Date Formatters ==============

/**
 * Format a date string or Date object to a readable format
 * 
 * @param date - ISO date string or Date object
 * @param formatStr - Format string (defaults to 'MMM dd, yyyy')
 * @returns Formatted date string
 */
export function formatDate(date: string | Date | null | undefined, formatStr: string = DATE_FORMATS.DISPLAY): string {
  if (!date) return 'N/A';
  
  try {
    const dateObj = typeof date === 'string' ? parseISO(date) : date;
    return format(dateObj, formatStr);
  } catch (error) {
    console.error('Error formatting date:', error);
    return 'Invalid date';
  }
}

/**
 * Format a date with time
 * 
 * @param date - ISO date string or Date object
 * @returns Formatted date with time string
 */
export function formatDateTime(date: string | Date | null | undefined): string {
  return formatDate(date, DATE_FORMATS.DISPLAY_WITH_TIME);
}

/**
 * Format a date as relative time (e.g., "2 hours ago", "in 3 days")
 * 
 * @param date - ISO date string or Date object
 * @returns Relative time string
 */
export function formatRelativeDate(date: string | Date | null | undefined): string {
  if (!date) return 'N/A';
  
  try {
    const dateObj = typeof date === 'string' ? parseISO(date) : date;
    return formatDistanceToNow(dateObj, { addSuffix: true });
  } catch (error) {
    console.error('Error formatting relative date:', error);
    return 'Invalid date';
  }
}

/**
 * Format a date with smart context (Today, Yesterday, Tomorrow, or date)
 * 
 * @param date - ISO date string or Date object
 * @returns Smart formatted date string
 */
export function formatSmartDate(date: string | Date | null | undefined): string {
  if (!date) return 'N/A';
  
  try {
    const dateObj = typeof date === 'string' ? parseISO(date) : date;
    
    if (isToday(dateObj)) {
      return 'Today';
    } else if (isTomorrow(dateObj)) {
      return 'Tomorrow';
    } else if (isYesterday(dateObj)) {
      return 'Yesterday';
    } else {
      return format(dateObj, DATE_FORMATS.DISPLAY);
    }
  } catch (error) {
    console.error('Error formatting smart date:', error);
    return 'Invalid date';
  }
}

/**
 * Get due date color class based on date
 * Requirement 22.2: Overdue dates in red, due today in orange
 * 
 * @param dueDate - ISO date string or Date object
 * @returns Tailwind color class
 */
export function getDueDateColor(dueDate: string | Date | null | undefined): string {
  if (!dueDate) return 'text-neutral-500';
  
  try {
    const dateObj = typeof dueDate === 'string' ? parseISO(dueDate) : dueDate;
    
    if (isPast(dateObj) && !isToday(dateObj)) {
      return 'text-red-600 font-semibold'; // Overdue
    } else if (isToday(dateObj)) {
      return 'text-orange-600 font-semibold'; // Due today
    } else {
      return 'text-neutral-600'; // Future
    }
  } catch (error) {
    return 'text-neutral-500';
  }
}

/**
 * Check if a date is overdue
 * 
 * @param dueDate - ISO date string or Date object
 * @returns True if overdue
 */
export function isOverdue(dueDate: string | Date | null | undefined): boolean {
  if (!dueDate) return false;
  
  try {
    const dateObj = typeof dueDate === 'string' ? parseISO(dueDate) : dueDate;
    return isPast(dateObj) && !isToday(dateObj);
  } catch (error) {
    return false;
  }
}

// ============== Number Formatters ==============

/**
 * Format a number with commas
 * 
 * @param value - Number to format
 * @returns Formatted number string
 */
export function formatNumber(value: number | null | undefined): string {
  if (value === null || value === undefined) return 'N/A';
  return value.toLocaleString();
}

/**
 * Format a decimal number with specified precision
 * 
 * @param value - Number to format
 * @param decimals - Number of decimal places (default: 2)
 * @returns Formatted number string
 */
export function formatDecimal(value: number | null | undefined, decimals: number = 2): string {
  if (value === null || value === undefined) return 'N/A';
  return value.toFixed(decimals);
}

/**
 * Format a percentage value
 * 
 * @param value - Number between 0 and 1 (or 0-100 if asPercentage is true)
 * @param asPercentage - If true, treats input as already a percentage (0-100)
 * @returns Formatted percentage string
 */
export function formatPercentage(value: number | null | undefined, asPercentage: boolean = false): string {
  if (value === null || value === undefined) return 'N/A';
  
  const percentage = asPercentage ? value : value * 100;
  return `${Math.round(percentage)}%`;
}

/**
 * Format hours with unit
 * 
 * @param hours - Number of hours
 * @returns Formatted hours string
 */
export function formatHours(hours: number | null | undefined): string {
  if (hours === null || hours === undefined) return 'N/A';
  return `${hours}h`;
}

/**
 * Format productivity score with color
 * 
 * @param score - Productivity score (0-100)
 * @returns Object with formatted score and color class
 */
export function formatProductivityScore(score: number | null | undefined): { text: string; color: string } {
  if (score === null || score === undefined) {
    return { text: 'N/A', color: 'text-neutral-500' };
  }
  
  const text = `${Math.round(score)}%`;
  let color = 'text-neutral-600';
  
  if (score >= 80) {
    color = 'text-green-600';
  } else if (score >= 60) {
    color = 'text-blue-600';
  } else if (score >= 40) {
    color = 'text-yellow-600';
  } else {
    color = 'text-red-600';
  }
  
  return { text, color };
}

// ============== Enum Formatters ==============

/**
 * Format task status to display label
 * 
 * @param status - TaskStatus enum value
 * @returns Display label
 */
export function formatTaskStatus(status: TaskStatus): string {
  return TASK_STATUS_LABELS[status] || status;
}

/**
 * Format task priority to display label
 * 
 * @param priority - TaskPriority enum value
 * @returns Display label
 */
export function formatTaskPriority(priority: TaskPriority): string {
  return TASK_PRIORITY_LABELS[priority] || priority;
}

/**
 * Format task category to display label
 * 
 * @param category - TaskCategory enum value
 * @returns Display label
 */
export function formatTaskCategory(category: TaskCategory): string {
  return TASK_CATEGORY_LABELS[category] || category;
}

/**
 * Format user role to display label
 * 
 * @param role - UserRole enum value
 * @returns Display label
 */
export function formatUserRole(role: UserRole): string {
  return USER_ROLE_LABELS[role] || role;
}

/**
 * Format notification type to display label
 * 
 * @param type - NotificationType enum value
 * @returns Display label
 */
export function formatNotificationType(type: NotificationType): string {
  return NOTIFICATION_TYPE_LABELS[type] || type;
}

// ============== User Formatters ==============

/**
 * Format user full name
 * 
 * @param firstName - User's first name
 * @param lastName - User's last name
 * @returns Full name
 */
export function formatUserName(firstName: string, lastName: string): string {
  return `${firstName} ${lastName}`.trim();
}

/**
 * Get user initials for avatar
 * 
 * @param firstName - User's first name
 * @param lastName - User's last name
 * @returns Initials (e.g., "JD")
 */
export function getUserInitials(firstName: string, lastName: string): string {
  const firstInitial = firstName?.charAt(0)?.toUpperCase() || '';
  const lastInitial = lastName?.charAt(0)?.toUpperCase() || '';
  return `${firstInitial}${lastInitial}`;
}

// ============== Sentiment Formatters ==============

/**
 * Format sentiment score with icon and color
 * 
 * @param score - Sentiment score (0.0 to 1.0)
 * @returns Object with icon, color, and label
 */
export function formatSentiment(score: number | null | undefined): {
  icon: string;
  color: string;
  label: string;
} {
  if (score === null || score === undefined) {
    return {
      icon: SENTIMENT_CONFIG.ICONS.NEUTRAL,
      color: SENTIMENT_CONFIG.COLORS.NEUTRAL,
      label: 'Unknown',
    };
  }
  
  if (score >= SENTIMENT_CONFIG.POSITIVE_THRESHOLD) {
    return {
      icon: SENTIMENT_CONFIG.ICONS.POSITIVE,
      color: SENTIMENT_CONFIG.COLORS.POSITIVE,
      label: 'Positive',
    };
  } else if (score <= SENTIMENT_CONFIG.NEGATIVE_THRESHOLD) {
    return {
      icon: SENTIMENT_CONFIG.ICONS.NEGATIVE,
      color: SENTIMENT_CONFIG.COLORS.NEGATIVE,
      label: 'Negative',
    };
  } else {
    return {
      icon: SENTIMENT_CONFIG.ICONS.NEUTRAL,
      color: SENTIMENT_CONFIG.COLORS.NEUTRAL,
      label: 'Neutral',
    };
  }
}

// ============== Tag Formatters ==============

/**
 * Parse tags string to array
 * 
 * @param tags - Comma-separated tags string
 * @returns Array of tag strings
 */
export function parseTags(tags: string | null | undefined): string[] {
  if (!tags) return [];
  return tags.split(',').map(tag => tag.trim()).filter(Boolean);
}

/**
 * Format tags array to string
 * 
 * @param tags - Array of tag strings
 * @returns Comma-separated tags string
 */
export function formatTags(tags: string[]): string {
  return tags.join(', ');
}

// ============== File Size Formatter ==============

/**
 * Format file size in bytes to human-readable format
 * 
 * @param bytes - File size in bytes
 * @returns Formatted file size string
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

// ============== Truncate Text ==============

/**
 * Truncate text to specified length with ellipsis
 * 
 * @param text - Text to truncate
 * @param maxLength - Maximum length
 * @returns Truncated text
 */
export function truncateText(text: string | null | undefined, maxLength: number): string {
  if (!text) return '';
  if (text.length <= maxLength) return text;
  return `${text.substring(0, maxLength)}...`;
}

// ============== AI Score Formatters ==============

/**
 * Format AI priority score
 * 
 * @param score - AI priority score (0.0 to 1.0)
 * @returns Formatted percentage string
 */
export function formatAiPriorityScore(score: number | null | undefined): string {
  return formatPercentage(score);
}

/**
 * Format predicted completion probability
 * 
 * @param probability - Completion probability (0.0 to 1.0)
 * @returns Object with formatted text and color
 */
export function formatCompletionProbability(probability: number | null | undefined): {
  text: string;
  color: string;
} {
  if (probability === null || probability === undefined) {
    return { text: 'N/A', color: 'text-neutral-500' };
  }
  
  const percentage = Math.round(probability * 100);
  const text = `${percentage}%`;
  
  let color = 'text-neutral-600';
  if (percentage >= 75) {
    color = 'text-green-600';
  } else if (percentage >= 50) {
    color = 'text-blue-600';
  } else if (percentage >= 25) {
    color = 'text-yellow-600';
  } else {
    color = 'text-red-600';
  }
  
  return { text, color };
}
