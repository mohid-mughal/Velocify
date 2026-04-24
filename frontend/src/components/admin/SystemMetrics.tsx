/**
 * System Metrics Component
 * 
 * Requirements: 27.3
 * Task: 26.3
 * 
 * Display system-wide metrics cards (total tasks, active users, AI feature usage).
 */

import { useDashboardSummary } from '../../hooks/useDashboard';
import { useUsers } from '../../hooks/useUsers';
import { useWorkloadSuggestions } from '../../hooks/useAi';

export default function SystemMetrics() {
  const { data: summary, isLoading: summaryLoading } = useDashboardSummary();
  const { data: users, isLoading: usersLoading } = useUsers();
  const { data: workloadSuggestions, isLoading: suggestionsLoading } = useWorkloadSuggestions(true);

  // Calculate system metrics
  const totalTasks = summary
    ? summary.pendingCount + summary.inProgressCount + summary.completedCount + summary.blockedCount
    : 0;
  const activeUsers = users?.items.filter(u => u.isActive).length || 0;
  const aiSuggestions = workloadSuggestions?.length || 0;

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
      {/* Total Tasks Card */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center justify-between">
          <div className="flex-1">
            <p className="text-sm font-medium text-gray-600 mb-1">Total Tasks</p>
            {summaryLoading ? (
              <div className="h-8 w-16 bg-gray-200 animate-pulse rounded"></div>
            ) : (
              <p className="text-3xl font-bold text-gray-900">{totalTasks}</p>
            )}
            {summary && (
              <div className="mt-2 text-xs text-gray-500">
                <span className="text-yellow-600">{summary.pendingCount} pending</span>
                {' • '}
                <span className="text-blue-600">{summary.inProgressCount} in progress</span>
              </div>
            )}
          </div>
          <div className="bg-blue-500 rounded-full p-3 text-white text-2xl">📊</div>
        </div>
      </div>

      {/* Active Users Card */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center justify-between">
          <div className="flex-1">
            <p className="text-sm font-medium text-gray-600 mb-1">Active Users</p>
            {usersLoading ? (
              <div className="h-8 w-16 bg-gray-200 animate-pulse rounded"></div>
            ) : (
              <p className="text-3xl font-bold text-gray-900">{activeUsers}</p>
            )}
            {users && (
              <div className="mt-2 text-xs text-gray-500">
                {users.items.length} total users
              </div>
            )}
          </div>
          <div className="bg-green-500 rounded-full p-3 text-white text-2xl">👥</div>
        </div>
      </div>

      {/* AI Suggestions Card */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center justify-between">
          <div className="flex-1">
            <p className="text-sm font-medium text-gray-600 mb-1">AI Suggestions</p>
            {suggestionsLoading ? (
              <div className="h-8 w-16 bg-gray-200 animate-pulse rounded"></div>
            ) : (
              <p className="text-3xl font-bold text-gray-900">{aiSuggestions}</p>
            )}
            <div className="mt-2 text-xs text-gray-500">
              Workload balancing recommendations
            </div>
          </div>
          <div className="bg-purple-500 rounded-full p-3 text-white text-2xl">🤖</div>
        </div>
      </div>
    </div>
  );
}
