/**
 * Dashboard Hooks
 * 
 * Requirements: 7.1-7.7
 * 
 * Custom hooks for dashboard operations using TanStack Query:
 * - useDashboardSummary: Query for dashboard summary statistics
 * - useVelocity: Query for task velocity data
 * - useWorkload: Query for workload distribution (Admin only)
 * - useOverdue: Query for overdue tasks
 */

import { useQuery } from '@tanstack/react-query';
import { dashboardKeys } from '../api/queryKeys';
import { dashboardService } from '../api/dashboard.service';
import type {
  DashboardSummaryDto,
  VelocityDataPoint,
  WorkloadDistributionDto,
  TaskDto,
} from '../api/types';

/**
 * Hook to fetch dashboard summary statistics
 * 
 * Requirement 7.1: Dashboard displays task counts by status
 * Requirement 7.2: Dashboard shows overdue and due today counts
 * 
 * @returns Query result with DashboardSummaryDto
 */
export function useDashboardSummary() {
  return useQuery({
    queryKey: dashboardKeys.summary(),
    queryFn: () => dashboardService.getSummary(),
  });
}

/**
 * Hook to fetch task velocity data
 * 
 * Requirement 7.3: Dashboard shows task completion velocity chart
 * Requirement 22.1: Line chart showing task completion velocity (last 30 days)
 * 
 * @param days - Number of days to include (default: 30)
 * @returns Query result with array of VelocityDataPoint
 */
export function useVelocity(days?: number) {
  return useQuery({
    queryKey: dashboardKeys.velocity(days),
    queryFn: () => dashboardService.getVelocity(days),
  });
}

/**
 * Hook to fetch workload distribution across team members
 * 
 * Requirement 7.4: Admin users can view workload distribution
 * Requirement 22.1: Admin users see workload distribution donut chart
 * 
 * Note: This endpoint requires Admin or SuperAdmin role
 * 
 * @returns Query result with array of WorkloadDistributionDto
 */
export function useWorkload() {
  return useQuery({
    queryKey: dashboardKeys.workload(),
    queryFn: () => dashboardService.getWorkload(),
  });
}

/**
 * Hook to fetch overdue tasks
 * 
 * Requirement 7.5: Dashboard shows overdue tasks alert
 * Requirement 22.1: Overdue tasks alert section
 * 
 * @returns Query result with array of overdue TaskDto
 */
export function useOverdue() {
  return useQuery({
    queryKey: dashboardKeys.overdue(),
    queryFn: () => dashboardService.getOverdue(),
  });
}

// Re-export types for convenience
export type { DashboardSummaryDto, VelocityDataPoint, WorkloadDistributionDto, TaskDto };
