/**
 * Task List Page
 * 
 * Requirements: 22.1-22.7
 * Task: 23.1, 23.2
 * 
 * Main task list page with:
 * - Filter panel (Status, Priority, Category, Assignee, Due Date range)
 * - Search input with 300ms debounce
 * - Semantic search toggle
 * - Task cards with all required information
 * - Pagination
 * - Admin users see bulk action toolbar
 */

import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTasks } from '../hooks/useTasks';
import { useDebounce } from '../hooks/useDebounce';
import { useUserRole } from '../store/authStore';
import { Button } from '../components/ui/Button';
import { TaskFilters, TaskList, BulkActionToolbar, SearchBar } from '../components/tasks';
import type { TaskFilters as TaskFiltersType } from '../api/types';
import { isPast, isToday } from 'date-fns';

export default function TaskListPage() {
  const navigate = useNavigate();
  const userRole = useUserRole();
  const isAdmin = userRole === 'Admin' || userRole === 'SuperAdmin';

  // Filter state
  const [filters, setFilters] = useState<TaskFiltersType>({
    page: 1,
    pageSize: 20,
  });

  // Search state
  const [searchTerm, setSearchTerm] = useState('');
  const [useSemanticSearch, setUseSemanticSearch] = useState(false);
  const debouncedSearch = useDebounce(searchTerm, 300);

  // Bulk action state (admin only)
  const [selectedTasks, setSelectedTasks] = useState<Set<string>>(new Set());

  // Update filters when debounced search changes
  const activeFilters = useMemo(() => ({
    ...filters,
    searchTerm: debouncedSearch || undefined,
  }), [filters, debouncedSearch]);

  // Fetch tasks with filters
  const { data, isLoading, error } = useTasks(activeFilters);

  // Handle filter changes
  const handleFilterChange = (key: keyof TaskFiltersType, value: any) => {
    setFilters(prev => ({
      ...prev,
      [key]: value,
      page: 1, // Reset to first page when filters change
    }));
  };

  // Handle page change
  const handlePageChange = (newPage: number) => {
    setFilters(prev => ({ ...prev, page: newPage }));
  };

  // Handle task selection (admin only)
  const handleTaskSelect = (taskId: string) => {
    setSelectedTasks(prev => {
      const newSet = new Set(prev);
      if (newSet.has(taskId)) {
        newSet.delete(taskId);
      } else {
        newSet.add(taskId);
      }
      return newSet;
    });
  };

  // Clear filters
  const handleClearFilters = () => {
    setFilters({ page: 1, pageSize: 20 });
    setSearchTerm('');
    setUseSemanticSearch(false);
  };

  // Get due date color
  const getDueDateColor = (dueDate: string | null) => {
    if (!dueDate) return 'text-neutral-500';
    const date = new Date(dueDate);
    if (isPast(date) && !isToday(date)) return 'text-danger-600';
    if (isToday(date)) return 'text-warning-600';
    return 'text-neutral-700';
  };

  // Handle task click
  const handleTaskClick = (taskId: string) => {
    navigate(`/tasks/${taskId}`);
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Tasks</h1>
            <p className="text-gray-600 mt-1">
              {data?.totalCount || 0} task{data?.totalCount !== 1 ? 's' : ''} found
            </p>
          </div>
          <Button onClick={() => navigate('/tasks/new')}>
            + New Task
          </Button>
        </div>

        {/* Search and Filters */}
        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          {/* Search Bar */}
          <div className="mb-4">
            <SearchBar
              searchTerm={searchTerm}
              onSearchChange={setSearchTerm}
              useSemanticSearch={useSemanticSearch}
              onSemanticToggle={setUseSemanticSearch}
            />
          </div>

          {/* Filter Panel */}
          <TaskFilters
            filters={filters}
            onFilterChange={handleFilterChange}
            onClearFilters={handleClearFilters}
          />
        </div>

        {/* Bulk Action Toolbar (Admin Only) */}
        {isAdmin && (
          <div className="mb-6">
            <BulkActionToolbar
              selectedCount={selectedTasks.size}
              onChangeStatus={() => {/* TODO: Implement */}}
              onReassign={() => {/* TODO: Implement */}}
              onDelete={() => {/* TODO: Implement */}}
            />
          </div>
        )}

        {/* Task List */}
        <TaskList
          tasks={data?.items || []}
          isLoading={isLoading}
          error={error}
          isAdmin={isAdmin}
          selectedTasks={selectedTasks}
          onTaskSelect={handleTaskSelect}
          onTaskClick={handleTaskClick}
          getDueDateColor={getDueDateColor}
        />

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between bg-white rounded-lg shadow-sm p-4 mt-6">
            <div className="text-sm text-neutral-600">
              Page {data.page} of {data.totalPages}
            </div>
            <div className="flex gap-2">
              <Button
                size="sm"
                variant="secondary"
                disabled={data.page === 1}
                onClick={() => handlePageChange(data.page - 1)}
              >
                Previous
              </Button>
              <Button
                size="sm"
                variant="secondary"
                disabled={data.page === data.totalPages}
                onClick={() => handlePageChange(data.page + 1)}
              >
                Next
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
