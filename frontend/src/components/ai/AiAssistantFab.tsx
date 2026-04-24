/**
 * AiAssistantFab Component
 * 
 * Requirements: 27.2
 * 
 * Floating action button (FAB) with AI icon that's accessible from all pages.
 * Opens the AI Assistant Drawer when clicked.
 */

import React from 'react';
import { clsx } from 'clsx';

export interface AiAssistantFabProps {
  onClick: () => void;
  className?: string;
}

/**
 * AiAssistantFab Component
 * 
 * Requirement 27.2: Floating action button (AI icon) accessible from all pages
 */
export const AiAssistantFab: React.FC<AiAssistantFabProps> = ({ onClick, className }) => {
  return (
    <button
      onClick={onClick}
      className={clsx(
        'fixed bottom-6 right-6 z-30',
        'w-14 h-14 rounded-full shadow-lg',
        'bg-gradient-to-br from-primary-500 to-secondary-500',
        'hover:from-primary-600 hover:to-secondary-600',
        'focus:outline-none focus:ring-4 focus:ring-primary-300',
        'transition-all duration-200 hover:scale-110',
        'flex items-center justify-center',
        'group',
        className
      )}
      aria-label="Open AI Assistant"
      title="AI Assistant"
    >
      {/* AI Icon */}
      <svg
        className="w-7 h-7 text-white group-hover:scale-110 transition-transform"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth={2}
          d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"
        />
      </svg>

      {/* Pulse animation */}
      <span className="absolute inset-0 rounded-full bg-primary-400 opacity-75 animate-ping" />
    </button>
  );
};

export default AiAssistantFab;
