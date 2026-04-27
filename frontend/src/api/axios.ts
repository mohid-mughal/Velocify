import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../store/authStore';

const envBaseUrl = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '');

// Create axios instance with base URL pointing to backend API
const axiosInstance = axios.create({
  // Forcing the /api/v1 suffix onto whatever the environment variable is
  baseURL: envBaseUrl ? `${envBaseUrl}/api/v1` : '/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Enable sending cookies for refresh tokens
});

// Request interceptor: Automatically add Authorization header with access token
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Retrieve access token from auth store (stored in memory)
    // Security requirement 21.5: Access tokens stored in memory only
    const accessToken = useAuthStore.getState().accessToken;
    
    if (accessToken && config.headers) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor: Handle 401 errors and attempt token refresh
axiosInstance.interceptors.response.use(
  (response) => {
    // Pass through successful responses
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    
    // Token Refresh Flow:
    // 1. Check if the error is 401 Unauthorized
    // 2. Ensure we haven't already attempted to retry this request
    // 3. Attempt to refresh the access token using the refresh token
    // 4. If refresh succeeds, retry the original request with new token
    // 5. If refresh fails, clear auth state and redirect to login
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Call the refresh token endpoint
        // The refresh token is sent via httpOnly cookie (withCredentials: true)
        const response = await axios.post(
          `${axiosInstance.defaults.baseURL}/auth/refresh`,
          {},
          { withCredentials: true }
        );
        
        // Extract new access token from response
        const { accessToken } = response.data;
        
        // Store new access token in auth store (memory only)
        // Security requirement 21.5: Access tokens stored in memory
        useAuthStore.getState().setToken(accessToken);
        
        // Update the Authorization header for the original request
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }
        
        // Retry the original request with the new token
        return axiosInstance(originalRequest);
      } catch (refreshError) {
        // Token refresh failed - user needs to log in again
        // Requirement 21.3: Clear auth state and redirect to login on 401
        useAuthStore.getState().logout();
        
        // Redirect to login page
        window.location.href = '/login';
        
        return Promise.reject(refreshError);
      }
    }
    
    // For all other errors, reject the promise
    return Promise.reject(error);
  }
);

export default axiosInstance;
