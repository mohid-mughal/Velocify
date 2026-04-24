/**
 * User Profile Page
 * 
 * Requirements: 26.1-26.5
 * Task: 26.1
 * 
 * User profile page displaying:
 * - User information (name, email, role, account creation date)
 * - Productivity score gauge chart (Recharts RadialBarChart)
 * - Personal velocity chart (tasks completed per week, last 12 weeks)
 * - Edit profile form
 * - Logout button
 */

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { format } from 'date-fns';
import {
  RadialBarChart,
  RadialBar,
  Legend,
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend as LineChartLegend,
} from 'recharts';
import { useCurrentUser, useUpdateCurrentUser, useUserProductivity } from '../hooks/useUsers';
import { useLogout } from '../hooks/useAuth';
import { useNavigate } from 'react-router-dom';

// Profile edit form schema
const profileSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100, 'First name too long'),
  lastName: z.string().min(1, 'Last name is required').max(100, 'Last name too long'),
});

type ProfileFormData = z.infer<typeof profileSchema>;

/**
 * UserProfilePage component
 * 
 * Requirement 26.1: Display user info (name, email, role, account creation date)
 * Requirement 26.2: Productivity score gauge chart (Recharts RadialBarChart)
 * Requirement 26.3: Personal velocity chart (tasks completed per week, last 12 weeks)
 * Requirement 26.4: Edit profile form
 * Requirement 26.5: Logout button
 */
export default function UserProfilePage() {
  const navigate = useNavigate();
  const [isEditing, setIsEditing] = useState(false);

  // Fetch current user data
  const { data: user, isLoading: isLoadingUser } = useCurrentUser();
  
  // Fetch productivity data
  const { data: productivity, isLoading: isLoadingProductivity } = useUserProductivity(
    user?.id || ''
  );

  // Mutations
  const updateUserMutation = useUpdateCurrentUser();
  const logoutMutation = useLogout();

  // Form setup
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    values: user
      ? {
          firstName: user.firstName,
          lastName: user.lastName,
        }
      : undefined,
  });

  // Handle profile update
  const onSubmit = async (data: ProfileFormData) => {
    try {
      await updateUserMutation.mutateAsync(data);
      setIsEditing(false);
    } catch (error) {
      console.error('Failed to update profile:', error);
    }
  };

  // Handle logout
  const handleLogout = async () => {
    try {
      await logoutMutation.mutateAsync();
      navigate('/login');
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  // Cancel editing
  const handleCancelEdit = () => {
    reset();
    setIsEditing(false);
  };

  // Prepare productivity gauge data
  const productivityGaugeData = productivity
    ? [
        {
          name: 'Productivity',
          value: productivity.currentScore,
          fill: getProductivityColor(productivity.currentScore),
        },
      ]
    : [];

  // Prepare velocity chart data (last 12 weeks)
  const velocityChartData = productivity?.history
    .slice(-12)
    .map((point) => ({
      week: format(new Date(point.date), 'MMM dd'),
      score: point.score,
    })) || [];

  if (isLoadingUser) {
    return (
      <div className="min-h-screen bg-gray-50 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
          </div>
        </div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="bg-white rounded-lg shadow p-6">
            <p className="text-red-600">Failed to load user profile</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">My Profile</h1>
            <p className="text-gray-600 mt-2">View and manage your profile information</p>
          </div>
          <button
            onClick={handleLogout}
            disabled={logoutMutation.isPending}
            className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {logoutMutation.isPending ? 'Logging out...' : 'Logout'}
          </button>
        </div>

        {/* User Information Card */}
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-semibold text-gray-900">Profile Information</h2>
            {!isEditing && (
              <button
                onClick={() => setIsEditing(true)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Edit Profile
              </button>
            )}
          </div>

          {isEditing ? (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    First Name
                  </label>
                  <input
                    type="text"
                    {...register('firstName')}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                  {errors.firstName && (
                    <p className="text-red-600 text-sm mt-1">{errors.firstName.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Last Name
                  </label>
                  <input
                    type="text"
                    {...register('lastName')}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                  {errors.lastName && (
                    <p className="text-red-600 text-sm mt-1">{errors.lastName.message}</p>
                  )}
                </div>
              </div>

              <div className="flex gap-3 pt-4">
                <button
                  type="submit"
                  disabled={updateUserMutation.isPending}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {updateUserMutation.isPending ? 'Saving...' : 'Save Changes'}
                </button>
                <button
                  type="button"
                  onClick={handleCancelEdit}
                  className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
                >
                  Cancel
                </button>
              </div>

              {updateUserMutation.isError && (
                <p className="text-red-600 text-sm">
                  Failed to update profile. Please try again.
                </p>
              )}
            </form>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <p className="text-sm text-gray-600 mb-1">Full Name</p>
                <p className="text-lg font-medium text-gray-900">
                  {user.firstName} {user.lastName}
                </p>
              </div>

              <div>
                <p className="text-sm text-gray-600 mb-1">Email</p>
                <p className="text-lg font-medium text-gray-900">{user.email}</p>
              </div>

              <div>
                <p className="text-sm text-gray-600 mb-1">Role</p>
                <span
                  className={`inline-flex px-3 py-1 rounded-full text-sm font-medium ${getRoleBadgeColor(
                    user.role
                  )}`}
                >
                  {user.role}
                </span>
              </div>

              <div>
                <p className="text-sm text-gray-600 mb-1">Account Created</p>
                <p className="text-lg font-medium text-gray-900">
                  {format(new Date(user.createdAt), 'MMM dd, yyyy')}
                </p>
              </div>

              {user.lastLoginAt && (
                <div>
                  <p className="text-sm text-gray-600 mb-1">Last Login</p>
                  <p className="text-lg font-medium text-gray-900">
                    {format(new Date(user.lastLoginAt), 'MMM dd, yyyy HH:mm')}
                  </p>
                </div>
              )}

              <div>
                <p className="text-sm text-gray-600 mb-1">Account Status</p>
                <span
                  className={`inline-flex px-3 py-1 rounded-full text-sm font-medium ${
                    user.isActive
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}
                >
                  {user.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>
            </div>
          )}
        </div>

        {/* Productivity Metrics */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Productivity Score Gauge */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Productivity Score
            </h3>
            {isLoadingProductivity ? (
              <div className="h-64 flex items-center justify-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
              </div>
            ) : productivity ? (
              <div>
                <ResponsiveContainer width="100%" height={300}>
                  <RadialBarChart
                    cx="50%"
                    cy="50%"
                    innerRadius="60%"
                    outerRadius="90%"
                    barSize={20}
                    data={productivityGaugeData}
                    startAngle={180}
                    endAngle={0}
                  >
                    <RadialBar
                      background
                      dataKey="value"
                      cornerRadius={10}
                    />
                    <Legend
                      iconSize={10}
                      layout="vertical"
                      verticalAlign="middle"
                      align="right"
                    />
                  </RadialBarChart>
                </ResponsiveContainer>
                <div className="text-center mt-4">
                  <p className="text-4xl font-bold text-gray-900">
                    {productivity.currentScore.toFixed(1)}%
                  </p>
                  <p className="text-sm text-gray-600 mt-1">Current Score</p>
                </div>
              </div>
            ) : (
              <div className="h-64 flex items-center justify-center text-gray-500">
                No productivity data available
              </div>
            )}
          </div>

          {/* Personal Velocity Chart */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Personal Velocity (Last 12 Weeks)
            </h3>
            {isLoadingProductivity ? (
              <div className="h-64 flex items-center justify-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
              </div>
            ) : velocityChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={velocityChartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="week" />
                  <YAxis />
                  <Tooltip />
                  <LineChartLegend />
                  <Line
                    type="monotone"
                    dataKey="score"
                    stroke="#3b82f6"
                    strokeWidth={2}
                    name="Productivity Score"
                  />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-64 flex items-center justify-center text-gray-500">
                No velocity data available
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

/**
 * Get color for productivity score gauge
 */
function getProductivityColor(score: number): string {
  if (score >= 80) return '#10b981'; // green
  if (score >= 60) return '#3b82f6'; // blue
  if (score >= 40) return '#f59e0b'; // orange
  return '#ef4444'; // red
}

/**
 * Get badge color for user role
 */
function getRoleBadgeColor(role: string): string {
  switch (role) {
    case 'SuperAdmin':
      return 'bg-purple-100 text-purple-800';
    case 'Admin':
      return 'bg-blue-100 text-blue-800';
    case 'Member':
      return 'bg-gray-100 text-gray-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}
