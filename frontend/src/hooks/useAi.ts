/**
 * AI Hooks
 * 
 * Requirements: 8.1-8.7, 9.1-9.6, 11.1-11.6, 12.1-12.7
 * 
 * Custom hooks for AI operations using TanStack Query:
 * - useAiParse: Mutation for natural language task parsing
 * - useAiDecompose: Mutation for task decomposition
 * - useSemanticSearch: Query for semantic search
 * - useWorkloadSuggestions: Query for workload suggestions (Admin only)
 * - useDigest: Query for daily digest
 * - useNormalizeImport: Mutation for CSV import normalization
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { aiKeys, taskKeys } from '../api/queryKeys';
import { aiService } from '../api/ai.service';
import type {
  CreateTaskRequest,
  SubtaskSuggestion,
  WorkloadSuggestion,
  TaskDto,
  TaskImportRow,
} from '../api/types';

/**
 * Hook to parse natural language input into task data
 * 
 * Requirement 8.1: Users can create tasks using natural language
 * Requirement 8.2: AI extracts title, description, priority, category, assignee, due date
 * Requirement 24.1: Natural Language mode for task creation
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useAiParse() {
  return useMutation({
    mutationFn: (input: string) => aiService.parseTask(input),
  });
}

/**
 * Hook to decompose a task into subtasks using AI
 * 
 * Requirement 9.1: AI can decompose tasks into subtasks
 * Requirement 9.2: Subtask generation is capped at 8 items
 * Requirement 23.1: AI decomposition button with modal
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useAiDecompose() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (taskId: string) => aiService.decomposeTask(taskId),
    onSuccess: (_, taskId) => {
      // Invalidate subtasks for this task since new ones may be created
      queryClient.invalidateQueries({ queryKey: taskKeys.subtasks(taskId) });
      
      // Invalidate task lists since subtasks are new tasks
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
    },
  });
}

/**
 * Hook to search tasks using semantic or keyword search
 * 
 * Requirement 12.1: Users can search tasks using natural language
 * Requirement 12.2: Semantic search uses AI embeddings
 * Requirement 22.1: Search input with semantic toggle
 * 
 * Note: This query is disabled by default to avoid unnecessary API calls.
 * Enable it when the user submits a search query.
 * 
 * @param query - Search query string
 * @param semantic - Whether to use semantic search (default: false)
 * @param enabled - Whether the query should execute (default: false)
 * @returns Query result with array of TaskDto
 */
export function useSemanticSearch(
  query: string,
  semantic: boolean = false,
  enabled: boolean = false
) {
  return useQuery({
    queryKey: aiKeys.search(query),
    queryFn: () => aiService.searchTasks(query, semantic),
    enabled: enabled && !!query.trim(),
    // Don't cache search results for too long since task data changes
    staleTime: 1000 * 60 * 2, // 2 minutes
  });
}

/**
 * Hook to get AI-powered workload balancing suggestions
 * 
 * Requirement 11.1: AI suggests task reassignments for workload balancing
 * Requirement 11.2: Suggestions consider task count, productivity scores, due dates
 * Requirement 27.2: Workload balancing panel with AI suggestions
 * 
 * Note: This endpoint requires Admin or SuperAdmin role
 * 
 * @param enabled - Whether the query should execute (default: true)
 * @returns Query result with array of WorkloadSuggestion
 */
export function useWorkloadSuggestions(enabled: boolean = true) {
  return useQuery({
    queryKey: aiKeys.workloadSuggestions(),
    queryFn: () => aiService.getWorkloadSuggestions(),
    enabled,
    // Refetch every 5 minutes since workload changes
    staleTime: 1000 * 60 * 5,
  });
}

/**
 * Hook to get the current user's daily digest
 * 
 * Requirement 10.1: AI generates daily digest for each user
 * Requirement 10.2: Digest includes tasks due today and overdue tasks
 * Requirement 22.1: AI digest card displaying today's digest
 * 
 * @param enabled - Whether the query should execute (default: true)
 * @returns Query result with digest content as string
 */
export function useDigest(enabled: boolean = true) {
  return useQuery({
    queryKey: aiKeys.digest(),
    queryFn: () => aiService.getMyDigest(),
    enabled,
    // Cache for 1 hour since digest is generated daily
    staleTime: 1000 * 60 * 60,
  });
}

/**
 * Hook to normalize CSV import data using AI
 * 
 * Requirement 13.1: AI normalizes imported CSV data
 * Requirement 13.2: AI maps non-standard columns to schema fields
 * Requirement 13.3: AI normalizes enum values
 * 
 * @returns Mutation object with mutate, isPending, isError, error, etc.
 */
export function useNormalizeImport() {
  return useMutation({
    mutationFn: (csvData: string) => aiService.normalizeImport(csvData),
  });
}

// Re-export types for convenience
export type {
  CreateTaskRequest,
  SubtaskSuggestion,
  WorkloadSuggestion,
  TaskDto,
  TaskImportRow,
};
