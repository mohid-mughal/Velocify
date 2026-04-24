/**
 * Dashboard Page
 * 
 * Requirements: 7.1-7.7
 * Task: 22.1, 22.2
 * 
 * Main dashboard page displaying:
 * - Four stat cards (Pending, InProgress, Completed, Blocked counts)
 * - Line chart showing task completion velocity (last 30 days)
 * - Bar chart showing tasks by priority
 * - AI digest card displaying today's digest
 * - Overdue tasks alert section
 * - Admin users see workload distribution donut chart (Recharts PieChart)
 */

import { useDashboardSummary, useVelocity, useWorkload, useOverdue } from '../hooks/useDashboard';
import { useUserRole } from '../store/authStore';
import { StatCard, VelocityChart, PriorityChart, WorkloadChart, DigestCard, OverdueAlert } from '../components/dashboard';
import { format } from 'date-fns';

export default function DashboardPage() {
  const userRole = useUserRole();
  const isAdmin = userRole === 'Admin' || userRole === 'SuperAdmin';

  // Fetch dashboard data
  const { data: summary, isLoading: summaryLoading } = useDashboardSummary();
  const { data: velocity, isLoading: velocityLoading } = useVelocity(30);
  const { data: workload, isLoading: workloadLoading } = useWorkload();
  const { data: overdueTasks, isLoading: overdueLoading } = useOverdue();

  // Prepare priority chart data from overdue tasks
  const priorityData = overdueTasks ? [
    { name: 'Critical', count: overdueTasks.filter(t => t.priority === 'Critical').length },
    { name: 'High', count: overdueTasks.filter(t => t.priority === 'High').length },
    { name: 'Medium', count: overdueTasks.filter(t => t.priority === 'Medium').length },
    { name: 'Low', count: overdueTasks.filter(t => t.priority === 'Low').length },
  ] : [];

  // Format velocity data for chart
  const velocityChartData = velocity?.map(point => ({
    date: format(new Date(point.date), 'MMM dd'),
    completed: point.completedCount,
  })) || [];

  // Format workload data for pie chart
  const workloadChartData = workload?.map(item => ({
    name: `${item.user.firstName} ${item.user.lastName}`,
    value: item.totalTaskCount,
  })) || [];

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600 mt-2">Overview of your tasks and productivity</p>
        </div>

        {/* Stat Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <StatCard
            title="Pending"
            count={summary?.pendingCount || 0}
            color="bg-blue-500"
            icon="📋"
            loading={summaryLoading}
          />
          <StatCard
            title="In Progress"
            count={summary?.inProgressCount || 0}
            color="bg-purple-500"
            icon="⚡"
            loading={summaryLoading}
          />
          <StatCard
            title="Completed"
            count={summary?.completedCount || 0}
            color="bg-green-500"
            icon="✓"
            loading={summaryLoading}
          />
          <StatCard
            title="Blocked"
            count={summary?.blockedCount || 0}
            color="bg-red-500"
            icon="⚠"
            loading={summaryLoading}
          />
        </div>

        {/* Overdue Tasks Alert */}
        <OverdueAlert tasks={overdueTasks || []} loading={overdueLoading} />

        {/* AI Digest Card */}
        <DigestCard
          dueTodayCount={summary?.dueTodayCount || 0}
          overdueCount={summary?.overdueCount || 0}
          loading={summaryLoading}
        />

        {/* Charts Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
          {/* Velocity Line Chart */}
          <VelocityChart data={velocityChartData} loading={velocityLoading} />

          {/* Priority Bar Chart */}
          <PriorityChart data={priorityData} loading={overdueLoading} />
        </div>

        {/* Admin-only Workload Distribution Chart */}
        {isAdmin && <WorkloadChart data={workloadChartData} loading={workloadLoading} />}
      </div>
    </div>
  );
}
