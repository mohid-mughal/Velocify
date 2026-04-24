import React from 'react';
import { MainLayout } from './MainLayout';
import { Button } from '../ui/Button';
import { Badge } from '../ui/Badge';

/**
 * LayoutExample component
 * 
 * Demonstrates how to use the MainLayout component with various content.
 * This is a reference implementation showing best practices.
 */
export const LayoutExample: React.FC = () => {
  return (
    <MainLayout>
      {/* Page header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-neutral-900 mb-2">
          Layout Example
        </h1>
        <p className="text-neutral-600">
          This page demonstrates the MainLayout component with Header, Sidebar, and Footer.
        </p>
      </div>

      {/* Content sections */}
      <div className="space-y-6">
        {/* Card example */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            Layout Features
          </h2>
          <ul className="space-y-2 text-neutral-700">
            <li className="flex items-start gap-2">
              <Badge variant="success" size="sm">✓</Badge>
              <span>Responsive header with navigation and user menu</span>
            </li>
            <li className="flex items-start gap-2">
              <Badge variant="success" size="sm">✓</Badge>
              <span>Collapsible sidebar with role-based navigation</span>
            </li>
            <li className="flex items-start gap-2">
              <Badge variant="success" size="sm">✓</Badge>
              <span>Notification bell with unread count</span>
            </li>
            <li className="flex items-start gap-2">
              <Badge variant="success" size="sm">✓</Badge>
              <span>Footer with quick links and support</span>
            </li>
            <li className="flex items-start gap-2">
              <Badge variant="success" size="sm">✓</Badge>
              <span>Mobile-friendly with slide-in drawer</span>
            </li>
          </ul>
        </div>

        {/* Action buttons */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            Quick Actions
          </h2>
          <div className="flex flex-wrap gap-3">
            <Button variant="primary">Create Task</Button>
            <Button variant="secondary">View Dashboard</Button>
            <Button variant="danger">Clear Filters</Button>
          </div>
        </div>

        {/* Grid example */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div className="bg-white rounded-lg shadow-md p-6">
            <h3 className="text-lg font-semibold text-neutral-900 mb-2">
              Stat Card 1
            </h3>
            <p className="text-3xl font-bold text-primary-600">42</p>
            <p className="text-sm text-neutral-600 mt-1">Active Tasks</p>
          </div>
          <div className="bg-white rounded-lg shadow-md p-6">
            <h3 className="text-lg font-semibold text-neutral-900 mb-2">
              Stat Card 2
            </h3>
            <p className="text-3xl font-bold text-success-600">18</p>
            <p className="text-sm text-neutral-600 mt-1">Completed Today</p>
          </div>
          <div className="bg-white rounded-lg shadow-md p-6">
            <h3 className="text-lg font-semibold text-neutral-900 mb-2">
              Stat Card 3
            </h3>
            <p className="text-3xl font-bold text-warning-600">5</p>
            <p className="text-sm text-neutral-600 mt-1">Overdue</p>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

/**
 * Example: Layout without sidebar
 */
export const LayoutExampleNoSidebar: React.FC = () => {
  return (
    <MainLayout showSidebar={false}>
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold text-neutral-900 mb-4">
          Centered Content
        </h1>
        <p className="text-neutral-600">
          This layout has no sidebar, useful for login, registration, or focused content pages.
        </p>
      </div>
    </MainLayout>
  );
};

/**
 * Example: Layout without footer
 */
export const LayoutExampleNoFooter: React.FC = () => {
  return (
    <MainLayout showFooter={false}>
      <h1 className="text-3xl font-bold text-neutral-900 mb-4">
        No Footer Layout
      </h1>
      <p className="text-neutral-600">
        This layout has no footer, useful for full-height content or dashboards.
      </p>
    </MainLayout>
  );
};
