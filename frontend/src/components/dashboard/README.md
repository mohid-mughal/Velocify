# Dashboard Components

**Requirements:** 7.1-7.7  
**Task:** 22.2

## Overview

This directory contains reusable dashboard components extracted from the DashboardPage. Each component is self-contained with proper TypeScript interfaces, loading states, and empty state handling.

## Components

### StatCard
**File:** `StatCard.tsx`  
**Requirement:** 7.1  
**Purpose:** Displays a single statistic card with title, count, icon, and loading state

**Props:**
- `title: string` - Card title (e.g., "Pending", "In Progress")
- `count: number` - Numeric value to display
- `color: string` - Tailwind CSS background color class (e.g., "bg-blue-500")
- `icon: string` - Emoji or icon to display
- `loading?: boolean` - Shows skeleton loader when true

### VelocityChart
**File:** `VelocityChart.tsx`  
**Requirement:** 7.2  
**Purpose:** Displays task completion velocity as a line chart using Recharts

**Props:**
- `data: VelocityDataPoint[]` - Array of {date: string, completed: number}
- `loading?: boolean` - Shows loading spinner when true
- `color?: string` - Line color (default: '#3b82f6')

**Features:**
- Responsive container
- Loading state with spinner
- Empty state message
- Formatted date labels

### PriorityChart
**File:** `PriorityChart.tsx`  
**Requirement:** 7.5  
**Purpose:** Displays tasks by priority as a bar chart using Recharts

**Props:**
- `data: PriorityDataPoint[]` - Array of {name: string, count: number}
- `loading?: boolean` - Shows loading spinner when true
- `colors?: Record<string, string>` - Priority color mapping

**Features:**
- Color-coded bars by priority (Critical=red, High=orange, Medium=blue, Low=green)
- Loading state with spinner
- Empty state message ("No overdue tasks - great job!")
- Responsive container

### WorkloadChart
**File:** `WorkloadChart.tsx`  
**Requirement:** 7.3  
**Purpose:** Displays team workload distribution as a pie chart using Recharts

**Props:**
- `data: WorkloadDataPoint[]` - Array of {name: string, value: number}
- `loading?: boolean` - Shows loading spinner when true
- `colors?: string[]` - Array of colors for pie slices

**Features:**
- Percentage labels on pie slices
- Legend with user names
- Loading state with spinner
- Empty state message
- Responsive container (height: 400px)

### DigestCard
**File:** `DigestCard.tsx`  
**Requirement:** 7.4  
**Purpose:** Displays AI-generated daily digest with task summary

**Props:**
- `dueTodayCount: number` - Number of tasks due today
- `overdueCount: number` - Number of overdue tasks
- `loading?: boolean` - Shows skeleton loader when true

**Features:**
- Gradient background (indigo to purple)
- Robot emoji icon
- Personalized message
- Loading state with skeleton

### OverdueAlert
**File:** `OverdueAlert.tsx`  
**Requirement:** 7.5  
**Purpose:** Displays alert for overdue tasks with task list

**Props:**
- `tasks: OverdueTask[]` - Array of {id: string, title: string, dueDate: string | null}
- `loading?: boolean` - Shows skeleton loader when true

**Features:**
- Red alert styling
- Shows up to 5 tasks with due dates
- "And X more..." message for additional tasks
- Automatically hides when no overdue tasks
- Loading state with skeleton

## Usage Example

```tsx
import {
  StatCard,
  VelocityChart,
  PriorityChart,
  WorkloadChart,
  DigestCard,
  OverdueAlert
} from '../components/dashboard';

function DashboardPage() {
  const { data: summary, isLoading: summaryLoading } = useDashboardSummary();
  const { data: velocity, isLoading: velocityLoading } = useVelocity(30);
  const { data: overdueTasks, isLoading: overdueLoading } = useOverdue();

  return (
    <div>
      {/* Stat Cards */}
      <StatCard
        title="Pending"
        count={summary?.pendingCount || 0}
        color="bg-blue-500"
        icon="📋"
        loading={summaryLoading}
      />

      {/* Overdue Alert */}
      <OverdueAlert tasks={overdueTasks || []} loading={overdueLoading} />

      {/* AI Digest */}
      <DigestCard
        dueTodayCount={summary?.dueTodayCount || 0}
        overdueCount={summary?.overdueCount || 0}
        loading={summaryLoading}
      />

      {/* Charts */}
      <VelocityChart data={velocityChartData} loading={velocityLoading} />
      <PriorityChart data={priorityData} loading={overdueLoading} />
      <WorkloadChart data={workloadChartData} loading={workloadLoading} />
    </div>
  );
}
```

## Design Decisions

1. **Self-contained components**: Each component includes its own title, container styling, and layout
2. **Consistent loading states**: All components show appropriate loading indicators
3. **Empty state handling**: Components gracefully handle empty data with helpful messages
4. **TypeScript interfaces**: All props are properly typed for type safety
5. **Recharts integration**: Chart components use Recharts for consistent visualization
6. **Tailwind CSS**: All styling uses Tailwind utility classes for consistency
7. **Accessibility**: Components use semantic HTML and proper ARIA attributes

## Testing

All components have been verified with TypeScript diagnostics and show no errors. The components are ready for integration testing with the backend API.
