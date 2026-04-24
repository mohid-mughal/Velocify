/**
 * Admin Page
 * 
 * Requirements: 27.1-27.5
 * Task: 26.2, 26.3
 * 
 * Admin dashboard displaying:
 * - User management table with role assignment (SuperAdmin only)
 * - Workload balancing panel with AI suggestions and Accept buttons
 * - System metrics cards (total tasks, active users, AI feature usage)
 * - AI adoption metrics charts
 * 
 * Access: Admin and SuperAdmin roles only
 */

import { useUserRole } from '../store/authStore';
import AccessDenied from '../components/AccessDenied';
import {
  UserManagementTable,
  WorkloadBalancingPanel,
  SystemMetrics,
  AiAdoptionCharts,
} from '../components/admin';

export default function AdminPage() {
  const userRole = useUserRole();
  const isAdmin = userRole === 'Admin' || userRole === 'SuperAdmin';
  const isSuperAdmin = userRole === 'SuperAdmin';

  // Requirement 27.5: Non-Admin users see access denied
  if (!isAdmin) {
    return <AccessDenied />;
  }

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Admin Dashboard</h1>
          <p className="text-gray-600 mt-2">Manage users, workload, and system metrics</p>
        </div>

        {/* System Metrics Cards - Requirement 27.3 */}
        <div className="mb-8">
          <SystemMetrics />
        </div>

        {/* Workload Balancing Panel - Requirement 27.3 */}
        <div className="bg-white rounded-lg shadow mb-8">
          <div className="p-6 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">Workload Balancing</h2>
            <p className="text-sm text-gray-600 mt-1">AI-powered task reassignment suggestions</p>
          </div>
          <div className="p-6">
            <WorkloadBalancingPanel />
          </div>
        </div>

        {/* User Management Table - Requirement 27.1 */}
        <div className="bg-white rounded-lg shadow mb-8">
          <div className="p-6 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">User Management</h2>
            <p className="text-sm text-gray-600 mt-1">
              {isSuperAdmin ? 'Manage user roles and access' : 'View team members'}
            </p>
          </div>
          <UserManagementTable />
        </div>

        {/* AI Adoption Metrics - Requirement 27.4 */}
        <div className="bg-white rounded-lg shadow p-6">
          <AiAdoptionCharts />
        </div>
      </div>
    </div>
  );
}
