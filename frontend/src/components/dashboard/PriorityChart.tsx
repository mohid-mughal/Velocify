/**
 * PriorityChart Component
 * 
 * Requirements: 7.5
 * Task: 22.2
 * 
 * Displays tasks by priority as a bar chart using Recharts
 */

import { BarChart, Bar, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';

interface PriorityDataPoint {
  name: string;
  count: number;
}

interface PriorityChartProps {
  data: PriorityDataPoint[];
  loading?: boolean;
  colors?: Record<string, string>;
}

const DEFAULT_PRIORITY_COLORS = {
  Critical: '#ef4444',
  High: '#f59e0b',
  Medium: '#3b82f6',
  Low: '#10b981',
};

export function PriorityChart({ data, loading, colors = DEFAULT_PRIORITY_COLORS }: PriorityChartProps) {
  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Overdue Tasks by Priority
        </h3>
        <div className="h-64 flex items-center justify-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
        </div>
      </div>
    );
  }

  if (!data || data.length === 0 || !data.some(d => d.count > 0)) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Overdue Tasks by Priority
        </h3>
        <div className="h-64 flex items-center justify-center text-gray-500">
          No overdue tasks - great job!
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">
        Overdue Tasks by Priority
      </h3>
      <ResponsiveContainer width="100%" height={300}>
        <BarChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" />
          <YAxis />
          <Tooltip />
          <Legend />
          <Bar dataKey="count" name="Task Count">
            {data.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={colors[entry.name] || '#3b82f6'} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
