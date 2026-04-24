/**
 * User Management Table Component
 * 
 * Requirements: 27.1, 27.2
 * Task: 26.3
 * 
 * Displays users with role assignment functionality.
 * SuperAdmin only can change roles.
 */

import { useState } from 'react';
import { useUsers, useUpdateUserRole } from '../../hooks/useUsers';
import { useUserRole } from '../../store/authStore';
import type { UserDto, UserRole } from '../../api/types';

export default function UserManagementTable() {
  const userRole = useUserRole();
  const isSuperAdmin = userRole === 'SuperAdmin';
  const { data: users, isLoading } = useUsers();
  const updateRoleMutation = useUpdateUserRole();
  
  const [editingUserId, setEditingUserId] = useState<string | null>(null);
  const [selectedRole, setSelectedRole] = useState<UserRole>('Member');

  const handleChangeRole = (user: UserDto) => {
    setEditingUserId(user.id);
    setSelectedRole(user.role);
  };

  const handleSaveRole = async (userId: string) => {
    try {
      await updateRoleMutation.mutateAsync({
        id: userId,
        role: { role: selectedRole },
      });
      setEditingUserId(null);
    } catch (error) {
      console.error('Failed to update role:', error);
    }
  };

  const handleCancelEdit = () => {
    setEditingUserId(null);
  };

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="space-y-4">
          {[1, 2, 3, 4, 5].map(i => (
            <div key={i} className="h-16 bg-gray-200 animate-pulse rounded"></div>
          ))}
        </div>
      </div>
    );
  }

  if (!users || users.items.length === 0) {
    return (
      <div className="p-6 text-center text-gray-500">No users found</div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              User
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Email
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Role
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Productivity
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Status
            </th>
            {isSuperAdmin && (
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            )}
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {users.items.map(user => (
            <tr key={user.id} className="hover:bg-gray-50">
              <td className="px-6 py-4 whitespace-nowrap">
                <div className="flex items-center">
                  <div className="flex-shrink-0 h-10 w-10 bg-blue-500 rounded-full flex items-center justify-center text-white font-semibold">
                    {user.firstName[0]}{user.lastName[0]}
                  </div>
                  <div className="ml-4">
                    <div className="text-sm font-medium text-gray-900">
                      {user.firstName} {user.lastName}
                    </div>
                  </div>
                </div>
              </td>
              <td className="px-6 py-4 whitespace-nowrap">
                <div className="text-sm text-gray-900">{user.email}</div>
              </td>
              <td className="px-6 py-4 whitespace-nowrap">
                {editingUserId === user.id ? (
                  <select
                    value={selectedRole}
                    onChange={(e) => setSelectedRole(e.target.value as UserRole)}
                    className="px-2 py-1 text-xs border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="Member">Member</option>
                    <option value="Admin">Admin</option>
                    <option value="SuperAdmin">SuperAdmin</option>
                  </select>
                ) : (
                  <span
                    className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                      user.role === 'SuperAdmin'
                        ? 'bg-purple-100 text-purple-800'
                        : user.role === 'Admin'
                        ? 'bg-blue-100 text-blue-800'
                        : 'bg-gray-100 text-gray-800'
                    }`}
                  >
                    {user.role}
                  </span>
                )}
              </td>
              <td className="px-6 py-4 whitespace-nowrap">
                <div className="text-sm text-gray-900">{user.productivityScore.toFixed(1)}</div>
              </td>
              <td className="px-6 py-4 whitespace-nowrap">
                <span
                  className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                    user.isActive
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}
                >
                  {user.isActive ? 'Active' : 'Inactive'}
                </span>
              </td>
              {isSuperAdmin && (
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  {editingUserId === user.id ? (
                    <div className="flex gap-2">
                      <button
                        className="text-green-600 hover:text-green-900"
                        onClick={() => handleSaveRole(user.id)}
                        disabled={updateRoleMutation.isPending}
                      >
                        {updateRoleMutation.isPending ? 'Saving...' : 'Save'}
                      </button>
                      <button
                        className="text-gray-600 hover:text-gray-900"
                        onClick={handleCancelEdit}
                        disabled={updateRoleMutation.isPending}
                      >
                        Cancel
                      </button>
                    </div>
                  ) : (
                    <button
                      className="text-blue-600 hover:text-blue-900"
                      onClick={() => handleChangeRole(user)}
                    >
                      Change Role
                    </button>
                  )}
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
