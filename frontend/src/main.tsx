/**
 * Main Entry Point
 * 
 * Task: 28.3
 * Requirements: All frontend requirements
 * 
 * Sets up the React application with all necessary providers:
 * - QueryClientProvider for TanStack Query
 * - React Query Devtools for development
 * - StrictMode for development checks
 */

import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import App from './App';
import { queryClient } from './api/queryClient';
import './index.css';

/**
 * Main Application Bootstrap
 * 
 * Responsibilities:
 * - Create React root and render application
 * - Setup QueryClientProvider for server state management
 * - Enable React Query Devtools in development
 * - Enable StrictMode for development checks
 * 
 * Requirements:
 * - 21.2: TanStack Query for server state management
 * - 21.4: Global error handling via QueryClient
 * - All frontend requirements
 */

// Get root element
const rootElement = document.getElementById('root');

if (!rootElement) {
  throw new Error('Failed to find the root element');
}

// Create React root
const root = createRoot(rootElement);

// Render application
root.render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
      {/* React Query Devtools - only visible in development */}
      <ReactQueryDevtools 
        initialIsOpen={false}
        position="bottom-right"
      />
    </QueryClientProvider>
  </StrictMode>
);
