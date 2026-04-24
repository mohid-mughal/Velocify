/**
 * VelocityChart Component
 * 
 * Requirements: 7.2
 * Task: 22.2
 * 
 * Displays task completion velocity as a line chart using Recharts
 */

import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';

interface VelocityDataPoint {
  date: string;
  completed: number;
}

interface VelocityChartProps {
  data: VelocityDataPoint[];
  loading?: boolean;
  color?: string;
}

export function VelocityChart({ data, loading, color = '#3b82f6' }: VelocityChartProps) {
  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Task Completion Velocity (Last 30 Days)
        </h3>
        <div className="h-64 flex items-center justify-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
        </div>
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Task Completion Velocity (Last 30 Days)
        </h3>
        <div className="h-64 flex items-center justify-center text-gray-500">
          No velocity data available
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">
        Task Completion Velocity (Last 30 Days)
      </h3>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="date" />
          <YAxis />
          <Tooltip />
          <Legend />
          <Line
            type="monotone"
            dataKey="completed"
            stroke={color}
            strokeWidth={2}
            name="Completed Tasks"
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
