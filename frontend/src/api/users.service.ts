/**
 * Users API Service
 * 
 * Requirements: 2.1-2.6, 26.1-26.5, 27.1-27.5
 * 
 * Handles all user-related API calls:
 * - Current user operations
 * - User management (Admin/SuperAdmin only)
 * - User productivity data
 */

import axiosInstance from './axios';
import type {
  UserDto,
  ProductivityDto,
  PagedResult,
  UserFilters,
  UpdateCurrentUserRequest,
  UpdateUserRoleRequest,
} from './types';

/**
 * Get the current authenticated user
 * 
 * Requirement 2.1: Users can view their profile
 * Requirement 26.1: Display user info (name, email, role, account creation date)
 * 
 * @returns Current UserDto
 */
export async function getCurrentUser(): Promise<UserDto> {
  const response = await axiosInstance.get<UserDto>('/users/me');
  return response.data;
}

/**
 * Update the current user's profile
 * 
 * Requirement 2.2: Users can update their profile
 * Requirement 26.1: Edit profile form
 * 
 * @param data - Update data (firstName, lastName)
 * @returns Updated UserDto
 */
export async function updateCurrentUser(data: UpdateCurrentUserRequest): Promise<UserDto> {
  const response = await axiosInstance.put<UserDto>('/users/me', data);
  return response.data;
}

/**
 * Get paginated list of users (Admin/SuperAdmin only)
 * 
 * Requirement 2.3: Admins can view all users
 * Requirement 27.1: User management table
 * 
 * Note: This endpoint requires Admin or SuperAdmin role
 * 
 * @param filters - Optional filters for page, pageSize
 * @returns PagedResult of UserDto
 */
export async function getUsers(filters?: UserFilters): Promise<PagedResult<UserDto>> {
  const response = await axiosInstance.get<PagedResult<UserDto>>('/users', {
    params: filters,
  });
  return response.data;
}

/**
 * Get a user by ID (Admin/SuperAdmin only)
 * 
 * Requirement 2.4: Admins can view user details
 * 
 * Note: This endpoint requires Admin or SuperAdmin role
 * 
 * @param id - User ID
 * @returns UserDto
 */
export async function getUserById(id: string): Promise<UserDto> {
  const response = await axiosInstance.get<UserDto>(`/users/${id}`);
  return response.data;
}

/**
 * Update a user's role (SuperAdmin only)
 * 
 * Requirement 2.5: SuperAdmins can assign user roles
 * Requirement 27.1: User management table with role assignment
 * 
 * Note: This endpoint requires SuperAdmin role
 * 
 * @param id - User ID
 * @param role - New role to assign
 * @returns Updated UserDto
 */
export async function updateUserRole(id: string, role: UpdateUserRoleRequest): Promise<UserDto> {
  const response = await axiosInstance.put<UserDto>(`/users/${id}/role`, role);
  return response.data;
}

/**
 * Soft delete a user (SuperAdmin only)
 * 
 * Requirement 2.6: SuperAdmins can deactivate users
 * Requirement 27.1: User management table
 * 
 * Note: This endpoint requires SuperAdmin role
 * 
 * @param id - User ID
 */
export async function deleteUser(id: string): Promise<void> {
  await axiosInstance.delete(`/users/${id}`);
}

/**
 * Get a user's productivity data
 * 
 * Requirement 26.1: Productivity score gauge chart
 * Requirement 26.2: Personal velocity chart
 * 
 * @param id - User ID
 * @returns ProductivityDto with current score and history
 */
export async function getUserProductivity(id: string): Promise<ProductivityDto> {
  const response = await axiosInstance.get<ProductivityDto>(`/users/${id}/productivity`);
  return response.data;
}

/**
 * Users service object with all user-related methods
 */
export const usersService = {
  getCurrentUser,
  updateCurrentUser,
  getUsers,
  getUserById,
  updateUserRole,
  deleteUser,
  getUserProductivity,
};

export default usersService;
