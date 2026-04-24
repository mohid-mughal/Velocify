/**
 * TaskFilters Component
 * 
 * Requirements: 22.4
 * Task: 23.2
 * 
 * Filter panel for task list with status, priority, category filters and clear button
 */

import { Select } from '../ui/Select';
import { Button } from '../ui/Button';
import type { TaskFilters as TaskFiltersType, TaskStatus, TaskPriority, TaskCategory } from '../../api/types';

interface TaskFiltersProps {
  filters: TaskFiltersType;
  onFilterChange: (key: keyof TaskFiltersType, value: any) => void;
  onClearFilters: () => void;
}

export function TaskFilters({ filters, onFilterChange, onClearFilters }: TaskFiltersProps) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      {/* Status Filter */}
      <Select
        label="Status"
        value={filters.status?.[0] || ''}
        onChange={(e) => onFilterChange('status', e.target.value ? [e.target.value as TaskStatus] : undefined)}
        options={[
          { value: '', label: 'All Statuses' },
          { value: 'Pending', label: 'Pending' },
          { value: 'InProgress', label: 'In Progress' },
          { value: 'Completed', label: 'Completed' },
          { value: 'Blocked', label: 'Blocked' },
          { value: 'Cancelled', label: 'Cancelled' },
        ]}
        fullWidth
      />

      {/* Priority Filter */}
      <Select
        label="Priority"
        value={filters.priority?.[0] || ''}
        onChange={(e) => onFilterChange('priority', e.target.value ? [e.target.value as TaskPriority] : undefined)}
        options={[
          { value: '', label: 'All Priorities' },
          { value: 'Critical', label: 'Critical' },
          { value: 'High', label: 'High' },
          { value: 'Medium', label: 'Medium' },
          { value: 'Low', label: 'Low' },
        ]}
        fullWidth
      />

      {/* Category Filter */}
      <Select
        label="Category"
        value={filters.category?.[0] || ''}
        onChange={(e) => onFilterChange('category', e.target.value ? [e.target.value as TaskCategory] : undefined)}
        options={[
          { value: '', label: 'All Categories' },
          { value: 'Development', label: 'Development' },
          { value: 'Design', label: 'Design' },
          { value: 'Marketing', label: 'Marketing' },
          { value: 'Operations', label: 'Operations' },
          { value: 'Research', label: 'Research' },
          { value: 'Other', label: 'Other' },
        ]}
        fullWidth
      />

      {/* Clear Filters Button */}
      <div className="flex items-end">
        <Button
          variant="secondary"
          onClick={onClearFilters}
          fullWidth
        >
          Clear Filters
        </Button>
      </div>
    </div>
  );
}
