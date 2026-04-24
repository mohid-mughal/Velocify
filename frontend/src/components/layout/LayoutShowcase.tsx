import React from 'react';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import { Footer } from './Footer';
import { MainLayout } from './MainLayout';

/**
 * LayoutShowcase component
 * 
 * Visual demonstration of all layout components.
 * Useful for development, testing, and documentation.
 */
export const LayoutShowcase: React.FC = () => {
  const [showSidebar, setShowSidebar] = React.useState(true);
  const [showFooter, setShowFooter] = React.useState(true);

  return (
    <div className="min-h-screen bg-neutral-100 p-8">
      <div className="max-w-7xl mx-auto space-y-8">
        {/* Title */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h1 className="text-3xl font-bold text-neutral-900 mb-2">
            Layout Components Showcase
          </h1>
          <p className="text-neutral-600">
            Visual demonstration of all layout components for the Velocify platform.
          </p>
        </div>

        {/* Controls */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            Layout Controls
          </h2>
          <div className="flex flex-wrap gap-4">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={showSidebar}
                onChange={(e) => setShowSidebar(e.target.checked)}
                className="w-4 h-4 text-primary-600 rounded focus:ring-2 focus:ring-primary-500"
              />
              <span className="text-sm text-neutral-700">Show Sidebar</span>
            </label>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={showFooter}
                onChange={(e) => setShowFooter(e.target.checked)}
                className="w-4 h-4 text-primary-600 rounded focus:ring-2 focus:ring-primary-500"
              />
              <span className="text-sm text-neutral-700">Show Footer</span>
            </label>
          </div>
        </div>

        {/* Header Component */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            Header Component
          </h2>
          <p className="text-sm text-neutral-600 mb-4">
            Navigation bar with logo, menu, notifications, and user menu.
          </p>
          <div className="border border-neutral-200 rounded-lg overflow-hidden">
            <Header />
          </div>
        </div>

        {/* Sidebar Component */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            Sidebar Component
          </h2>
          <p className="text-sm text-neutral-600 mb-4">
            Navigation menu with role-based access control.
          </p>
          <div className="border border-neutral-200 rounded-lg overflow-hidden h-96 relative">
            <Sidebar isOpen={true} />
          </div>
        </div>

        {/* Footer Component */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            Footer Component
          </h2>
          <p className="text-sm text-neutral-600 mb-4">
            Footer with brand, quick links, and support information.
          </p>
          <div className="border border-neutral-200 rounded-lg overflow-hidden">
            <Footer />
          </div>
        </div>

        {/* MainLayout Component */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            MainLayout Component
          </h2>
          <p className="text-sm text-neutral-600 mb-4">
            Complete layout combining Header, Sidebar, content area, and Footer.
          </p>
          <div className="border border-neutral-200 rounded-lg overflow-hidden h-[600px]">
            <div className="h-full overflow-auto">
              <MainLayout showSidebar={showSidebar} showFooter={showFooter}>
                <div className="space-y-4">
                  <h3 className="text-2xl font-bold text-neutral-900">
                    Page Content
                  </h3>
                  <p className="text-neutral-600">
                    This is the main content area. It can contain any page content.
                  </p>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="bg-primary-50 rounded-lg p-4">
                      <h4 className="font-semibold text-primary-900 mb-2">
                        Feature 1
                      </h4>
                      <p className="text-sm text-primary-700">
                        Responsive design with mobile support
                      </p>
                    </div>
                    <div className="bg-secondary-50 rounded-lg p-4">
                      <h4 className="font-semibold text-secondary-900 mb-2">
                        Feature 2
                      </h4>
                      <p className="text-sm text-secondary-700">
                        Role-based navigation filtering
                      </p>
                    </div>
                    <div className="bg-success-50 rounded-lg p-4">
                      <h4 className="font-semibold text-success-900 mb-2">
                        Feature 3
                      </h4>
                      <p className="text-sm text-success-700">
                        Notification system integration
                      </p>
                    </div>
                    <div className="bg-warning-50 rounded-lg p-4">
                      <h4 className="font-semibold text-warning-900 mb-2">
                        Feature 4
                      </h4>
                      <p className="text-sm text-warning-700">
                        AI Assistant quick access
                      </p>
                    </div>
                  </div>
                </div>
              </MainLayout>
            </div>
          </div>
        </div>

        {/* Component Props */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            Component Props
          </h2>
          <div className="space-y-4">
            <div>
              <h3 className="text-lg font-semibold text-neutral-800 mb-2">
                Header
              </h3>
              <ul className="text-sm text-neutral-600 space-y-1 ml-4">
                <li>• onMenuClick?: () =&gt; void</li>
                <li>• className?: string</li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-neutral-800 mb-2">
                Sidebar
              </h3>
              <ul className="text-sm text-neutral-600 space-y-1 ml-4">
                <li>• isOpen?: boolean</li>
                <li>• onClose?: () =&gt; void</li>
                <li>• className?: string</li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-neutral-800 mb-2">
                Footer
              </h3>
              <ul className="text-sm text-neutral-600 space-y-1 ml-4">
                <li>• className?: string</li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-neutral-800 mb-2">
                MainLayout
              </h3>
              <ul className="text-sm text-neutral-600 space-y-1 ml-4">
                <li>• children: React.ReactNode</li>
                <li>• className?: string</li>
                <li>• showSidebar?: boolean (default: true)</li>
                <li>• showFooter?: boolean (default: true)</li>
              </ul>
            </div>
          </div>
        </div>

        {/* Usage Examples */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-neutral-900 mb-4">
            Usage Examples
          </h2>
          <div className="space-y-4">
            <div>
              <h3 className="text-sm font-semibold text-neutral-800 mb-2">
                Basic Layout
              </h3>
              <pre className="bg-neutral-900 text-neutral-100 p-4 rounded-lg text-xs overflow-x-auto">
{`import { MainLayout } from '@/components/layout';

function DashboardPage() {
  return (
    <MainLayout>
      <h1>Dashboard</h1>
      {/* Page content */}
    </MainLayout>
  );
}`}
              </pre>
            </div>
            <div>
              <h3 className="text-sm font-semibold text-neutral-800 mb-2">
                Layout Without Sidebar
              </h3>
              <pre className="bg-neutral-900 text-neutral-100 p-4 rounded-lg text-xs overflow-x-auto">
{`import { MainLayout } from '@/components/layout';

function LoginPage() {
  return (
    <MainLayout showSidebar={false} showFooter={false}>
      <div className="max-w-md mx-auto">
        {/* Login form */}
      </div>
    </MainLayout>
  );
}`}
              </pre>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
