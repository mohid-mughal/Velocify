/**
 * Authentication Hooks
 * 
 * Requirements: 1.1-1.8
 * 
 * Custom hooks for authentication operations:
 * - useAuth: Access auth store state
 * - useLogin: Mutation for login
 * - useRegister: Mutation for register
 * - useLogout: Mutation for logout
 */

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuthStore, type User } from '../store/authStore';
import { authService } from '../api/auth.service';
import { queryKeys } from '../api/queryKeys';
import type { LoginRequest, RegisterRequest, AuthResponseDto } from '../api/types';

/**
 * Hook to access auth store state
 * 
 * Provides read-only access to authentication state from the Zustand store.
 * Use this hook to check authentication status and get user info.
 * 
 * @returns Auth state: user, accessToken, role, isAuthenticated
 * 
 * Requirements: 1.1-1.8 (Auth state management)
 */
export function useAuth() {
  const user = useAuthStore((state) => state.user);
  const accessToken = useAuthStore((state) => state.accessToken);
  const role = useAuthStore((state) => state.role);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  return {
    user,
    accessToken,
    role,
    isAuthenticated,
  };
}

/**
 * Hook to handle user login
 * 
 * Performs login mutation and updates auth store on success.
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 * 
 * Requirements:
 * - 1.2: Users can log in with email and password
 * - 21.2: TanStack Query invalidates cache keys on mutations
 */
export function useLogin() {
  const loginStore = useAuthStore((state) => state.login);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (credentials: LoginRequest) => authService.login(credentials.email, credentials.password),
    onSuccess: (data: AuthResponseDto) => {
      // Update auth store with token and user info
      loginStore(data.accessToken, mapUserDtoToUser(data.user));
      
      // Invalidate relevant queries to refetch with new auth context
      queryClient.invalidateQueries({ queryKey: queryKeys.auth.session() });
      queryClient.invalidateQueries({ queryKey: queryKeys.users.me() });
    },
  });
}

/**
 * Hook to handle user registration
 * 
 * Performs registration mutation and updates auth store on success.
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 * 
 * Requirements:
 * - 1.1: Users can register with first name, last name, email, and password
 * - 21.2: TanStack Query invalidates cache keys on mutations
 */
export function useRegister() {
  const loginStore = useAuthStore((state) => state.login);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: RegisterRequest) => authService.register(data),
    onSuccess: (data: AuthResponseDto) => {
      // Update auth store with token and user info
      loginStore(data.accessToken, mapUserDtoToUser(data.user));
      
      // Invalidate relevant queries to refetch with new auth context
      queryClient.invalidateQueries({ queryKey: queryKeys.auth.session() });
      queryClient.invalidateQueries({ queryKey: queryKeys.users.me() });
    },
  });
}

/**
 * Hook to handle user logout
 * 
 * Performs logout mutation and clears auth store on success.
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 * 
 * Requirements:
 * - 1.6: Users can log out to revoke their session
 * - 21.2: TanStack Query invalidates cache keys on mutations
 */
export function useLogout() {
  const logoutStore = useAuthStore((state) => state.logout);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => authService.logout(),
    onSuccess: () => {
      // Clear auth store
      logoutStore();
      
      // Clear all cached queries since user is no longer authenticated
      queryClient.clear();
    },
  });
}

/**
 * Map UserDto from API to User type in auth store
 * 
 * Ensures type compatibility between API response and store state.
 */
function mapUserDtoToUser(dto: AuthResponseDto['user']): User {
  return {
    id: dto.id,
    firstName: dto.firstName,
    lastName: dto.lastName,
    email: dto.email,
    role: dto.role,
    productivityScore: dto.productivityScore,
    isActive: dto.isActive,
    createdAt: dto.createdAt,
    lastLoginAt: dto.lastLoginAt,
  };
}
