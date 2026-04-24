import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

// User type matching backend UserDto
export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'SuperAdmin' | 'Admin' | 'Member';
  productivityScore: number;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

// Auth state interface
interface AuthState {
  // State
  user: User | null;
  accessToken: string | null;
  role: 'SuperAdmin' | 'Admin' | 'Member' | null;
  isAuthenticated: boolean;

  // Actions
  login: (accessToken: string, user: User) => void;
  logout: () => void;
  setUser: (user: User) => void;
  setToken: (accessToken: string) => void;
}

/**
 * Zustand auth store for managing authentication state
 * 
 * Security Requirements (21.5, 21.6):
 * - Access tokens are stored in memory only (not persisted)
 * - User information is persisted to sessionStorage for page refresh
 * - Tokens are never stored in localStorage or sessionStorage
 * 
 * State Management (21.1):
 * - user: Current authenticated user information
 * - accessToken: JWT access token (memory only, 15-minute TTL)
 * - role: User's role for authorization checks
 * - isAuthenticated: Computed from presence of user and token
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      // Initial state
      user: null,
      accessToken: null,
      role: null,
      isAuthenticated: false,

      // Login action: Store access token in memory and user info
      login: (accessToken: string, user: User) => {
        set({
          accessToken,
          user,
          role: user.role,
          isAuthenticated: true,
        });
      },

      // Logout action: Clear all auth state
      logout: () => {
        set({
          accessToken: null,
          user: null,
          role: null,
          isAuthenticated: false,
        });
      },

      // Update user information (e.g., after profile update)
      setUser: (user: User) => {
        set({
          user,
          role: user.role,
        });
      },

      // Update access token (e.g., after token refresh)
      setToken: (accessToken: string) => {
        set({
          accessToken,
          isAuthenticated: true,
        });
      },
    }),
    {
      name: 'auth-storage', // sessionStorage key
      storage: createJSONStorage(() => sessionStorage),
      // Partition state: only persist user info, NOT tokens
      partialize: (state) => ({
        user: state.user,
        role: state.role,
        // Explicitly exclude accessToken from persistence
        // isAuthenticated will be recomputed on hydration
      }),
      // On hydration, recompute isAuthenticated based on persisted user
      onRehydrateStorage: () => (state) => {
        if (state) {
          // User info was restored from sessionStorage, but no token
          // Set isAuthenticated to false - user must refresh token or re-login
          state.isAuthenticated = false;
          state.accessToken = null;
        }
      },
    }
  )
);

// Selector hooks for common use cases
export const useUser = () => useAuthStore((state) => state.user);
export const useIsAuthenticated = () => useAuthStore((state) => state.isAuthenticated);
export const useUserRole = () => useAuthStore((state) => state.role);
export const useAccessToken = () => useAuthStore((state) => state.accessToken);
