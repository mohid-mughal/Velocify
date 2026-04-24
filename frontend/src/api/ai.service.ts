/**
 * AI API Service
 * 
 * Requirements: 8.1-8.7, 9.1-9.6, 10.1-10.7, 11.1-11.6, 12.1-12.7, 13.1-13.7
 * 
 * Handles all AI-related API calls:
 * - Natural language task parsing
 * - Task decomposition
 * - Semantic search
 * - Workload suggestions (Admin only)
 * - Import normalization
 * - Daily digest
 */

import axiosInstance from './axios';
import type {
  CreateTaskRequest,
  SubtaskSuggestion,
  WorkloadSuggestion,
  TaskDto,
  TaskImportRow,
  SemanticSearchRequest,
  NormalizeImportRequest,
} from './types';

/**
 * Parse natural language input into task data
 * 
 * Requirement 8.1: Users can create tasks using natural language
 * Requirement 8.2: AI extracts title, description, priority, category, assignee, due date
 * Requirement 24.1: Natural Language mode for task creation
 * 
 * @param input - Natural language task description
 * @returns CreateTaskRequest with parsed fields
 */
export async function parseTask(input: string): Promise<CreateTaskRequest> {
  const response = await axiosInstance.post<CreateTaskRequest>('/ai/parse-task', { input });
  return response.data;
}

/**
 * Decompose a task into subtasks using AI
 * 
 * Requirement 9.1: AI can decompose tasks into subtasks
 * Requirement 9.2: Subtask generation is capped at 8 items
 * Requirement 23.1: AI decomposition button with modal
 * 
 * @param taskId - Task ID to decompose
 * @returns List of SubtaskSuggestion
 */
export async function decomposeTask(taskId: string): Promise<SubtaskSuggestion[]> {
  const response = await axiosInstance.post<SubtaskSuggestion[]>(`/ai/decompose/${taskId}`);
  return response.data;
}

/**
 * Search tasks using semantic or keyword search
 * 
 * Requirement 12.1: Users can search tasks using natural language
 * Requirement 12.2: Semantic search uses AI embeddings
 * Requirement 22.1: Search input with semantic toggle
 * 
 * @param query - Search query
 * @param semantic - Whether to use semantic search (default: false)
 * @returns List of matching TaskDto
 */
export async function searchTasks(query: string, semantic: boolean = false): Promise<TaskDto[]> {
  const response = await axiosInstance.post<TaskDto[]>('/ai/search', {
    query,
    useSemanticSearch: semantic,
  } as SemanticSearchRequest);
  return response.data;
}

/**
 * Get AI-powered workload balancing suggestions
 * 
 * Requirement 11.1: AI suggests task reassignments for workload balancing
 * Requirement 11.2: Suggestions consider task count, productivity scores, due dates
 * Requirement 27.2: Workload balancing panel with AI suggestions
 * 
 * Note: This endpoint requires Admin or SuperAdmin role
 * 
 * @returns List of WorkloadSuggestion
 */
export async function getWorkloadSuggestions(): Promise<WorkloadSuggestion[]> {
  const response = await axiosInstance.get<WorkloadSuggestion[]>('/ai/workload-suggestions');
  return response.data;
}

/**
 * Normalize CSV import data using AI
 * 
 * Requirement 13.1: AI normalizes imported CSV data
 * Requirement 13.2: AI maps non-standard columns to schema fields
 * Requirement 13.3: AI normalizes enum values
 * 
 * @param csvData - Raw CSV data as string
 * @returns List of normalized TaskImportRow
 */
export async function normalizeImport(csvData: string): Promise<TaskImportRow[]> {
  const response = await axiosInstance.post<TaskImportRow[]>('/ai/import-normalize', {
    csvData,
  } as NormalizeImportRequest);
  return response.data;
}

/**
 * Get the current user's daily digest
 * 
 * Requirement 10.1: AI generates daily digest for each user
 * Requirement 10.2: Digest includes tasks due today and overdue tasks
 * Requirement 22.1: AI digest card displaying today's digest
 * 
 * @returns Daily digest content as string
 */
export async function getMyDigest(): Promise<string> {
  const response = await axiosInstance.get<string>('/ai/digest/me');
  return response.data;
}

/**
 * AI service object with all AI-related methods
 */
export const aiService = {
  parseTask,
  decomposeTask,
  searchTasks,
  getWorkloadSuggestions,
  normalizeImport,
  getMyDigest,
};

export default aiService;
