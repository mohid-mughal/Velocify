/**
 * AI Adoption Charts Component
 * 
 * Requirements: 27.4
 * Task: 26.3
 * 
 * Show AI feature adoption metrics with Recharts visualizations.
 * 
 * Note: This component displays mock data for demonstration purposes.
 * In a production environment, this would fetch real AI interaction logs
 * from a dedicated analytics endpoint.
 */

import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';

// Mock data for AI feature usage
// In production, this would come from an API endpoint that queries AiInteractionLog table
const featureUsageData = [
  { feature: 'Task Creation', count: 145 },
  { feature: 'Decomposition', count: 89 },
  { feature: 'Search', count: 234 },
  { feature: 'Digest', count: 67 },
  { feature: 'Workload', count: 42 },
  { feature: 'Import', count: 28 },
];

// Mock data for AI usage over time (last 7 days)
const usageOverTimeData = [
  { day: 'Mon', interactions: 45 },
  { day: 'Tue', interactions: 52 },
  { day: 'Wed', interactions: 48 },
  { day: 'Thu', interactions: 61 },
  { day: 'Fri', interactions: 55 },
  { day: 'Sat', interactions: 23 },
  { day: 'Sun', interactions: 19 },
];

// Colors for the pie chart
const COLORS = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899'];

export default function AiAdoptionCharts() {
  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h2 className="text-xl font-semibold text-gray-900 mb-2">AI Feature Adoption</h2>
        <p className="text-sm text-gray-600">
          Track AI feature usage across the platform to understand adoption patterns
        </p>
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Feature Usage Bar Chart */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Feature Usage Distribution</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={featureUsageData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis 
                dataKey="feature" 
                angle={-45}
                textAnchor="end"
                height={80}
                tick={{ fontSize: 12 }}
              />
              <YAxis />
              <Tooltip />
              <Bar dataKey="count" fill="#3B82F6" />
            </BarChart>
          </ResponsiveContainer>
          <p className="text-xs text-gray-500 mt-2">
            Total AI interactions across all features
          </p>
        </div>

        {/* Feature Distribution Pie Chart */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Feature Distribution</h3>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={featureUsageData}
                cx="50%"
                cy="50%"
                labelLine={false}
                label={({ feature, percent }) => `${feature}: ${(percent * 100).toFixed(0)}%`}
                outerRadius={80}
                fill="#8884d8"
                dataKey="count"
              >
                {featureUsageData.map((_entry, index) => (
                  <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
          <p className="text-xs text-gray-500 mt-2">
            Percentage breakdown of AI feature usage
          </p>
        </div>

        {/* Usage Over Time Bar Chart */}
        <div className="bg-white rounded-lg shadow p-6 lg:col-span-2">
          <h3 className="text-lg font-medium text-gray-900 mb-4">AI Interactions Over Time</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={usageOverTimeData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="day" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="interactions" fill="#10B981" name="AI Interactions" />
            </BarChart>
          </ResponsiveContainer>
          <p className="text-xs text-gray-500 mt-2">
            Daily AI interaction count for the last 7 days
          </p>
        </div>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-blue-50 rounded-lg p-4">
          <p className="text-sm font-medium text-blue-900">Total Interactions</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">605</p>
        </div>
        <div className="bg-green-50 rounded-lg p-4">
          <p className="text-sm font-medium text-green-900">Most Used Feature</p>
          <p className="text-2xl font-bold text-green-600 mt-1">Search</p>
        </div>
        <div className="bg-purple-50 rounded-lg p-4">
          <p className="text-sm font-medium text-purple-900">Avg. Daily Usage</p>
          <p className="text-2xl font-bold text-purple-600 mt-1">86</p>
        </div>
        <div className="bg-orange-50 rounded-lg p-4">
          <p className="text-sm font-medium text-orange-900">Growth Rate</p>
          <p className="text-2xl font-bold text-orange-600 mt-1">+12%</p>
        </div>
      </div>

      {/* Note about data source */}
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <p className="text-sm text-yellow-800">
          <span className="font-semibold">Note:</span> This component currently displays mock data for demonstration.
          In production, connect to an analytics endpoint that queries the AiInteractionLog table for real metrics.
        </p>
      </div>
    </div>
  );
}
