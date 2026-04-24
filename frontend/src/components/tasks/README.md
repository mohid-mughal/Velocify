# Task Components

**Requirements:** 22.1-22.7  
**Task:** 23.2

This directory contains reusable components for the task list interface, extracted from TaskListPage.tsx for better maintainability and reusability.

## Components

### TaskFilters
Filter panel with dropdowns for Status, Priority, and Category, plus a Clear Filters button.

**Props:**
- `filters: TaskFilters` - Current filter state
- `onFilterChange: (key, value) => void` - Handler for filter changes
- `onClearFilters: () => void` - Handler to reset all filters

**Requirements:** 22.4

### SearchBar
Search input with semantic search toggle checkbox.

**Props:**
- `searchTerm: string` - Current search term
- `onSearchChange: (value: string) => void` - Handler for search input changes
- `useSemanticSearch: boolean` - Semantic search toggle state
- `onSemanticToggle: (value: boolean) => void` - Handler for semantic toggle

**Requirements:** 22.5, 22.6

### TaskCard
Individual task card displaying title, badges, avatar, due date, and AI probability score.

**Props:**
- `task: TaskDto` - Task data
- `isAdmin: boolean` - Whether current user is admin
- `isSelected: boolean` - Whether task is selected for bulk actions
- `onSelect: () => void` - Handler for checkbox selection
- `onClick: () => void` - Handler for card click
- `getDueDateColor: (dueDate) => string` - Function to determine due date color

**Requirements:** 22.1, 22.2, 22.3

### TaskList
Container component that renders a list of TaskCard components with loading and empty states.

**Props:**
- `tasks: TaskDto[]` - Array of tasks to display
- `isLoading: boolean` - Loading state
- `error: Error | null` - Error state
- `isAdmin: boolean` - Whether current user is admin
- `selectedTasks: Set<string>` - Set of selected task IDs
- `onTaskSelect: (taskId: string) => void` - Handler for task selection
- `onTaskClick: (taskId: string) => void` - Handler for task click
- `getDueDateColor: (dueDate) => string` - Function to determine due date color

**Requirements:** 22.1

### BulkActionToolbar
Toolbar for bulk actions on selected tasks (Admin only). Shows selected count and action buttons.

**Props:**
- `selectedCount: number` - Number of selected tasks
- `onChangeStatus: () => void` - Handler for change status action
- `onReassign: () => void` - Handler for reassign action
- `onDelete: () => void` - Handler for delete action

**Requirements:** 22.7

## Usage

```tsx
import { 
  TaskFilters, 
  TaskList, 
  BulkActionToolbar, 
  SearchBar 
} from '../components/tasks';

// In your component
<SearchBar
  searchTerm={searchTerm}
  onSearchChange={setSearchTerm}
  useSemanticSearch={useSemanticSearch}
  onSemanticToggle={setUseSemanticSearch}
/>

<TaskFilters
  filters={filters}
  onFilterChange={handleFilterChange}
  onClearFilters={handleClearFilters}
/>

<BulkActionToolbar
  selectedCount={selectedTasks.size}
  onChangeStatus={handleChangeStatus}
  onReassign={handleReassign}
  onDelete={handleDelete}
/>

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
```

## Design Patterns

All components follow these patterns:
- **Controlled components**: State is managed by parent component
- **Single responsibility**: Each component has one clear purpose
- **TypeScript**: Fully typed with explicit interfaces
- **Tailwind CSS**: Consistent styling with design system
- **Accessibility**: Proper semantic HTML and ARIA attributes

## Future Enhancements

The BulkActionToolbar currently has placeholder handlers. Future tasks will implement:
- Bulk status change modal
- Bulk reassignment modal
- Bulk delete confirmation dialog
