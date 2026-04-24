/**
 * Users Hooks
 * 
 * Requirements: 26.1-26.5, 27.1-27.5
 * 
 * Custom hooks for user operations using TanStack Query:
 * - useCurrentUser: Query for current authenticated user
 * - useUpdateCurrentUser: Mutation for updating current user profile
 * - useUsers: Query for user list (Admin only)
 * - useUserById: Query for specific user (Admin only)
 * - useUpdateUserRole: Mutation for updating user role (SuperAdmin only)
 * - useDeleteUser: Mutation for deleting user (SuperAdmin only)
 * - useUserProductivity: Query for user productivity data
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userKeys } from '../api/queryKeys';
import { usersService } from '../api/users.service';
import { useAuthStore } from '../store/authStore';
import type {
  UserDto,
  ProductivityDto,
  PagedResult,
  UserFilters,
  UpdateCurrentUserRequest,
  UpdateUserRoleRequest,
} from '../api/types';

/**
 * Hook to fetch the current authenticated user
 * 
 * Requirement 26.1: Display user info (name, email, role, account creation date)
 * 
 * @returns Query result with UserDto
 */
export function useCurrentUser() {
  return useQuery({
    queryKey: userKeys.me(),
    queryFn: () => usersService.getCurrentUser(),
  });
}

/**
 * Hook to update the current user's profile
 * 
 * Requirement 26.4: Edit profile form
 * 
 * @returns Mutation object for updating current user
 */
export function useUpdateCurrentUser() {
  const queryClient = useQueryClient();
  const setUser = useAuthStore((state) => state.setUser);

  return useMutation({
    mutationFn: (data: UpdateCurrentUserRequest) => usersService.updateCurrentUser(data),
    onSuccess: (updatedUser) => {
      // Update auth store with new user data
      setUser(updatedUser);
      
      // Invalidate current user query
      queryClient.invalidateQueries({ queryKey: userKeys.me() });
    },
  });
}

/**
 * Hook to fetch paginated list of users (Admin/SuperAdmin only)
 * 
 * Requirement 27.1: User management table
 * 
 * @param filters - Optional filters for pagination
 * @returns Query result with PagedResult<UserDto>
 */
export function useUsers(filters?: UserFilters) {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: () => usersService.getUsers(filters),
  });
}

/**
 * Hook to fetch a specific user by ID (Admin/SuperAdmin only)
 * 
 * Requirement 27.1: User management
 * 
 * @param id - User ID
 * @returns Query result with UserDto
 */
export function useUserById(id: string) {
  return useQuery({
    queryKey: userKeys.detail(id),
    queryFn: () => usersService.getUserById(id),
    enabled: Boolean(id),
  });
}

/**
 * Hook to update a user's role (SuperAdmin only)
 * 
 * Requirement 27.2: SuperAdmins can assign user roles
 * 
 * @returns Mutation object for updating user role
 */
export function useUpdateUserRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, role }: { id: string; role: UpdateUserRoleRequest }) =>
      usersService.updateUserRole(id, role),
    onSuccess: (_, variables) => {
      // Invalidate user queries
      queryClient.invalidateQueries({ queryKey: userKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: userKeys.lists() });
    },
  });
}

/**
 * Hook to delete a user (SuperAdmin only)
 * 
 * Requirement 27.1: User management
 * 
 * @returns Mutation object for deleting user
 */
export function useDeleteUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => usersService.deleteUser(id),
    onSuccess: () => {
      // Invalidate user list queries
      queryClient.invalidateQueries({ queryKey: userKeys.lists() });
    },
  });
}

/**
 * Hook to fetch user productivity data
 * 
 * Requirement 26.2: Productivity score gauge chart
 * Requirement 26.3: Personal velocity chart (tasks completed per week, last 12 weeks)
 * 
 * @param userId - User ID
 * @returns Query result with ProductivityDto
 */
export function useUserProductivity(userId: string) {
  return useQuery({
    queryKey: userKeys.productivity(userId),
    queryFn: () => usersService.getUserProductivity(userId),
    enabled: Boolean(userId),
  });
}

// Re-export types for convenience
export type { UserDto, ProductivityDto, PagedResult, UserFilters };
