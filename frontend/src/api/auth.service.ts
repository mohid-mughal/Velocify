/**
 * Authentication API Service
 * 
 * Requirements: 1.1-1.8
 * 
 * Handles all authentication-related API calls:
 * - User registration
 * - User login
 * - Token refresh
 * - User logout
 * - Session revocation (Admin only)
 */

import axiosInstance from './axios';
import type { AuthResponseDto, RegisterRequest } from './types';

/**
 * Register a new user
 * 
 * Requirement 1.1: Users can register with first name, last name, email, and password
 * 
 * @param data - Registration data (firstName, lastName, email, password)
 * @returns AuthResponseDto with access token, refresh token, and user info
 */
export async function register(data: RegisterRequest): Promise<AuthResponseDto> {
  const response = await axiosInstance.post<AuthResponseDto>('/auth/register', data);
  return response.data;
}

/**
 * Login with email and password
 * 
 * Requirement 1.2: Users can log in with email and password
 * 
 * @param email - User's email address
 * @param password - User's password
 * @returns AuthResponseDto with access token, refresh token, and user info
 */
export async function login(email: string, password: string): Promise<AuthResponseDto> {
  const response = await axiosInstance.post<AuthResponseDto>('/auth/login', { email, password });
  return response.data;
}

/**
 * Refresh the access token using the refresh token
 * 
 * Requirement 1.4: Refresh tokens are stored as httpOnly cookies
 * Requirement 1.5: Access tokens expire after 15 minutes
 * 
 * Note: The refresh token is sent automatically via httpOnly cookie (withCredentials: true)
 * 
 * @returns AuthResponseDto with new access token, refresh token, and user info
 */
export async function refreshToken(): Promise<AuthResponseDto> {
  const response = await axiosInstance.post<AuthResponseDto>('/auth/refresh');
  return response.data;
}

/**
 * Logout the current user
 * 
 * Requirement 1.6: Users can log out to revoke their session
 * 
 * This will revoke the current refresh token and clear the session
 */
export async function logout(): Promise<void> {
  await axiosInstance.post('/auth/logout');
}

/**
 * Revoke all sessions for the current user
 * 
 * Requirement 1.7: Admin users can revoke all sessions for a user
 * 
 * This will revoke all refresh tokens for the current user,
 * forcing them to log in again on all devices.
 * 
 * Note: This endpoint requires Admin or SuperAdmin role
 */
export async function revokeAllSessions(): Promise<void> {
  await axiosInstance.post('/auth/revoke-all-sessions');
}

/**
 * Auth service object with all authentication methods
 */
export const authService = {
  register,
  login,
  refreshToken,
  logout,
  revokeAllSessions,
};

export default authService;
