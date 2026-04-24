import React, { useState } from 'react';
import { clsx } from 'clsx';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import { Footer } from './Footer';
import { AiAssistantFab, AiAssistantDrawer } from '../ai';

export interface MainLayoutProps {
  children: React.ReactNode;
  className?: string;
  showSidebar?: boolean;
  showFooter?: boolean;
}

/**
 * MainLayout component
 * 
 * Combines Header, Sidebar, content area, and Footer into a cohesive layout.
 * 
 * Features:
 * - Responsive design with mobile-friendly sidebar toggle
 * - Sticky header for persistent navigation
 * - Collapsible sidebar on mobile devices
 * - Optional footer display
 * - Flexible content area with proper spacing
 * 
 * Layout structure:
 * - Header: Fixed at top with navigation, user menu, and notifications
 * - Sidebar: Fixed on desktop, slide-in drawer on mobile
 * - Content: Main scrollable area with proper padding
 * - Footer: Optional footer at bottom
 */
export const MainLayout: React.FC<MainLayoutProps> = ({
  children,
  className,
  showSidebar = true,
  showFooter = true,
}) => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [isAiAssistantOpen, setIsAiAssistantOpen] = useState(false);

  const handleMenuClick = () => {
    setIsSidebarOpen(true);
  };

  const handleSidebarClose = () => {
    setIsSidebarOpen(false);
  };

  const handleAiAssistantOpen = () => {
    setIsAiAssistantOpen(true);
  };

  const handleAiAssistantClose = () => {
    setIsAiAssistantOpen(false);
  };

  return (
    <div className="min-h-screen flex flex-col bg-neutral-50">
      {/* Header */}
      <Header onMenuClick={handleMenuClick} />

      {/* Main content area */}
      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar */}
        {showSidebar && (
          <Sidebar isOpen={isSidebarOpen} onClose={handleSidebarClose} />
        )}

        {/* Content */}
        <main
          className={clsx(
            'flex-1 overflow-y-auto',
            showSidebar && 'lg:ml-0',
            className
          )}
        >
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {children}
          </div>

          {/* Footer */}
          {showFooter && <Footer />}
        </main>
      </div>

      {/* AI Assistant FAB - accessible from all pages */}
      <AiAssistantFab onClick={handleAiAssistantOpen} />

      {/* AI Assistant Drawer */}
      <AiAssistantDrawer isOpen={isAiAssistantOpen} onClose={handleAiAssistantClose} />
    </div>
  );
};
