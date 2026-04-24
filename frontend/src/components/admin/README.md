# Admin Components

This directory contains reusable admin-specific components extracted from the AdminPage.

## Components

### UserManagementTable

**Requirements:** 27.1, 27.2  
**Task:** 26.3

Displays a table of all users with their information and allows SuperAdmins to change user roles.

**Features:**
- Displays user avatar, name, email, role, productivity score, and status
- SuperAdmins can click "Change Role" to edit a user's role inline
- Role changes are saved via the `useUpdateUserRole` hook
- Automatically refreshes the user list after role changes
- Shows loading states and handles empty states

**Usage:**
```tsx
import { UserManagementTable } from '../components/admin';

<UserManagementTable />
```

---

### WorkloadBalancingPanel

**Requirements:** 27.3  
**Task:** 26.3

Shows AI-powered workload balancing suggestions with Accept buttons that reassign tasks.

**Features:**
- Fetches workload suggestions from the AI service
- Displays task ID, reason for suggestion, and suggested assignee
- Accept button triggers task reassignment via `useUpdateTask` hook
- Automatically refreshes suggestions after accepting
- Shows loading states and empty states when no suggestions available

**Usage:**
```tsx
import { WorkloadBalancingPanel } from '../components/admin';

<WorkloadBalancingPanel />
```

---

### SystemMetrics

**Requirements:** 27.3  
**Task:** 26.3

Displays system-wide metrics in card format.

**Features:**
- Total Tasks card: Shows total task count with breakdown by status
- Active Users card: Shows count of active users vs total users
- AI Suggestions card: Shows count of current workload balancing suggestions
- Each card has an icon and color-coded design
- Shows loading states with skeleton loaders

**Usage:**
```tsx
import { SystemMetrics } from '../components/admin';

<SystemMetrics />
```

---

### AiAdoptionCharts

**Requirements:** 27.4  
**Task:** 26.3

Shows AI feature adoption metrics with Recharts visualizations.

**Features:**
- Feature Usage Distribution: Bar chart showing usage count per AI feature
- Feature Distribution: Pie chart showing percentage breakdown
- AI Interactions Over Time: Bar chart showing daily interaction counts
- Summary stats cards: Total interactions, most used feature, avg daily usage, growth rate
- Currently displays mock data (see note below)

**Note:** This component currently uses mock data for demonstration. In production, connect to an analytics endpoint that queries the `AiInteractionLog` table for real metrics.

**Usage:**
```tsx
import { AiAdoptionCharts } from '../components/admin';

<AiAdoptionCharts />
```

---

## Implementation Details

### Data Fetching

All components use TanStack Query hooks for data fetching:
- `useUsers()` - Fetches user list
- `useUpdateUserRole()` - Updates user role
- `useWorkloadSuggestions()` - Fetches AI workload suggestions
- `useUpdateTask()` - Updates task assignment
- `useDashboardSummary()` - Fetches dashboard metrics

### State Management

- Local component state for UI interactions (e.g., editing mode in UserManagementTable)
- TanStack Query for server state caching and invalidation
- Zustand auth store for user role checks

### Role-Based Access

- UserManagementTable: Only SuperAdmins can change roles
- WorkloadBalancingPanel: Available to Admins and SuperAdmins
- SystemMetrics: Available to Admins and SuperAdmins
- AiAdoptionCharts: Available to Admins and SuperAdmins

The parent AdminPage component handles the overall access control check.

---

## Future Enhancements

1. **AiAdoptionCharts**: Replace mock data with real API endpoint
2. **UserManagementTable**: Add user deletion functionality
3. **WorkloadBalancingPanel**: Add ability to reject suggestions
4. **SystemMetrics**: Add more detailed drill-down views
5. **All components**: Add unit tests with React Testing Library
