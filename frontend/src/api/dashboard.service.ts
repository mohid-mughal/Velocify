/**
 * Dashboard API Service
 * 
 * Requirements: 7.1-7.7
 * 
 * Handles all dashboard-related API calls:
 * - Dashboard summary statistics
 * - Task velocity data
 * - Workload distribution (Admin only)
 * - Overdue tasks
 */

import axiosInstance from './axios';
import type { DashboardSummaryDto, VelocityDataPoint, WorkloadDistributionDto, TaskDto } from './types';

/**
 * Get dashboard summary statistics
 * 
 * Requirement 7.1: Dashboard displays task counts by status
 * Requirement 7.2: Dashboard shows overdue and due today counts
 * 
 * @returns DashboardSummaryDto with task counts
 */
export async function getSummary(): Promise<DashboardSummaryDto> {
  const response = await axiosInstance.get<DashboardSummaryDto>('/dashboard/summary');
  return response.data;
}

/**
 * Get task velocity data for the specified number of days
 * 
 * Requirement 7.3: Dashboard shows task completion velocity chart
 * Requirement 22.1: Line chart showing task completion velocity (last 30 days)
 * 
 * @param days - Number of days to include (default: 30)
 * @returns List of VelocityDataPoint
 */
export async function getVelocity(days?: number): Promise<VelocityDataPoint[]> {
  const response = await axiosInstance.get<VelocityDataPoint[]>('/dashboard/velocity', {
    params: { days },
  });
  return response.data;
}

/**
 * Get workload distribution across team members
 * 
 * Requirement 7.4: Admin users can view workload distribution
 * Requirement 22.1: Admin users see workload distribution donut chart
 * 
 * Note: This endpoint requires Admin or SuperAdmin role
 * 
 * @returns List of WorkloadDistributionDto
 */
export async function getWorkload(): Promise<WorkloadDistributionDto[]> {
  const response = await axiosInstance.get<WorkloadDistributionDto[]>('/dashboard/workload');
  return response.data;
}

/**
 * Get list of overdue tasks
 * 
 * Requirement 7.5: Dashboard shows overdue tasks alert
 * Requirement 22.1: Overdue tasks alert section
 * 
 * @returns List of overdue TaskDto
 */
export async function getOverdue(): Promise<TaskDto[]> {
  const response = await axiosInstance.get<TaskDto[]>('/dashboard/overdue');
  return response.data;
}

/**
 * Dashboard service object with all dashboard-related methods
 */
export const dashboardService = {
  getSummary,
  getVelocity,
  getWorkload,
  getOverdue,
};

export default dashboardService;
