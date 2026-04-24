/**
 * StatCard Component
 * 
 * Requirements: 7.1
 * Task: 22.2
 * 
 * Displays a single statistic card with title, count, icon, and loading state
 */

interface StatCardProps {
  title: string;
  count: number;
  color: string;
  icon: string;
  loading?: boolean;
}

export function StatCard({ title, count, color, icon, loading }: StatCardProps) {
  return (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex items-center justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-gray-600 mb-1">{title}</p>
          {loading ? (
            <div className="h-8 w-16 bg-gray-200 animate-pulse rounded"></div>
          ) : (
            <p className="text-3xl font-bold text-gray-900">{count}</p>
          )}
        </div>
        <div className={`${color} rounded-full p-3 text-white text-2xl`}>
          {icon}
        </div>
      </div>
    </div>
  );
}
