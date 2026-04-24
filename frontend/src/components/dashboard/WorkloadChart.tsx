/**
 * WorkloadChart Component
 * 
 * Requirements: 7.3
 * Task: 22.2
 * 
 * Displays team workload distribution as a pie chart using Recharts
 */

import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from 'recharts';

interface WorkloadDataPoint {
  name: string;
  value: number;
}

interface WorkloadChartProps {
  data: WorkloadDataPoint[];
  loading?: boolean;
  colors?: string[];
}

const DEFAULT_WORKLOAD_COLORS = [
  '#3b82f6',
  '#8b5cf6',
  '#6366f1',
  '#ec4899',
  '#14b8a6',
  '#f97316',
];

export function WorkloadChart({ data, loading, colors = DEFAULT_WORKLOAD_COLORS }: WorkloadChartProps) {
  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Team Workload Distribution
        </h3>
        <div className="h-80 flex items-center justify-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
        </div>
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Team Workload Distribution
        </h3>
        <div className="h-80 flex items-center justify-center text-gray-500">
          No workload data available
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">
        Team Workload Distribution
      </h3>
      <ResponsiveContainer width="100%" height={400}>
        <PieChart>
          <Pie
            data={data}
            cx="50%"
            cy="50%"
            labelLine={false}
            label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
            outerRadius={120}
            fill="#8884d8"
            dataKey="value"
          >
            {data.map((_, index) => (
              <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
            ))}
          </Pie>
          <Tooltip />
          <Legend />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
}
