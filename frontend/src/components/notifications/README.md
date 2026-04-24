# Notification Components

This directory contains all notification-related components for the Velocify platform.

## Components

### NotificationsPanel
**File:** `NotificationsPanel.tsx`

The main notification panel component that displays all user notifications in a slide-in drawer.

**Features:**
- Slide-in drawer from right side
- Displays all notifications ordered by creation time
- Mark individual notifications as read
- Mark all notifications as read
- Real-time updates via SignalR
- Unread count badge
- Empty state when no notifications
- Loading state with spinner
- Error handling

**Props:**
```typescript
interface NotificationsPanelProps {
  isOpen: boolean;      // Controls drawer visibility
  onClose: () => void;  // Callback when drawer is closed
}
```

**Usage:**
```tsx
import { NotificationsPanel } from '@/components/notifications';

function MyComponent() {
  const [isOpen, setIsOpen] = useState(false);
  
  return (
    <>
      <button onClick={() => setIsOpen(true)}>Open Notifications</button>
      <NotificationsPanel 
        isOpen={isOpen} 
        onClose={() => setIsOpen(false)} 
      />
    </>
  );
}
```

### NotificationItem
**File:** `NotificationItem.tsx`

Individual notification item component with icon, message, timestamp, and read/unread indicator.

**Features:**
- Type-specific icons (Assigned, StatusChanged, DueSoon, Overdue, AiSuggestion)
- Type-specific colors
- Formatted timestamp
- Read/unread visual states
- Mark as read button for unread notifications
- Unread indicator dot
- Hover effects

**Props:**
```typescript
interface NotificationItemProps {
  id: string;                           // Notification ID
  type: NotificationType;               // Notification type
  message: string;                      // Notification message
  createdAt: string;                    // ISO timestamp
  isRead: boolean;                      // Read status
  onMarkAsRead: (id: string) => void;   // Mark as read callback
  isMarkingAsRead?: boolean;            // Loading state
}
```

**Notification Types:**
- `Assigned` - Task assigned to user (blue)
- `StatusChanged` - Task status changed (green)
- `DueSoon` - Task due soon (orange)
- `Overdue` - Task overdue (red)
- `AiSuggestion` - AI-generated suggestion (purple)

**Usage:**
```tsx
import { NotificationItem } from '@/components/notifications';

function MyComponent() {
  const handleMarkAsRead = (id: string) => {
    // Mark notification as read
  };
  
  return (
    <NotificationItem
      id="123"
      type="Assigned"
      message="You have been assigned to Task ABC"
      createdAt="2024-01-15T10:30:00Z"
      isRead={false}
      onMarkAsRead={handleMarkAsRead}
    />
  );
}
```

### NotificationBell
**File:** `NotificationBell.tsx`

Notification bell icon button with unread count badge.

**Features:**
- Bell icon with SVG
- Unread count badge (shows "9+" for counts > 9)
- Badge only visible when count > 0
- Hover and focus states
- Accessible with aria-label

**Props:**
```typescript
interface NotificationBellProps {
  unreadCount: number;    // Number of unread notifications
  onClick: () => void;    // Click callback
  className?: string;     // Optional CSS classes
}
```

**Usage:**
```tsx
import { NotificationBell } from '@/components/notifications';

function Header() {
  const unreadCount = 5;
  const [showPanel, setShowPanel] = useState(false);
  
  return (
    <NotificationBell
      unreadCount={unreadCount}
      onClick={() => setShowPanel(true)}
    />
  );
}
```

## Integration Example

Complete example showing how all notification components work together:

```tsx
import React, { useState } from 'react';
import { 
  NotificationsPanel, 
  NotificationBell 
} from '@/components/notifications';
import { useNotificationStore } from '@/store/notificationStore';

function AppHeader() {
  const [showNotifications, setShowNotifications] = useState(false);
  const unreadCount = useNotificationStore((state) => state.unreadCount);
  
  return (
    <header>
      <nav>
        {/* Other header content */}
        
        <NotificationBell
          unreadCount={unreadCount}
          onClick={() => setShowNotifications(true)}
        />
      </nav>
      
      <NotificationsPanel
        isOpen={showNotifications}
        onClose={() => setShowNotifications(false)}
      />
    </header>
  );
}
```

## State Management

Notifications use Zustand for state management:

```typescript
// Store location: src/store/notificationStore.ts

interface NotificationStore {
  notifications: Notification[];
  unreadCount: number;
  setNotifications: (notifications: Notification[]) => void;
  addNotification: (notification: Notification) => void;
  markAsRead: (id: string) => void;
  markAllAsRead: () => void;
  incrementUnread: () => void;
}
```

## Real-Time Updates

Notifications are updated in real-time via SignalR:

```typescript
// SignalR events handled:
- 'TaskAssigned': When a task is assigned to the user
- 'StatusChanged': When a task status changes
- 'CommentAdded': When a comment is added to a task
- 'AiSuggestionReady': When an AI suggestion is available

// On event received:
1. Add notification to store
2. Increment unread count
3. Show toast notification
4. Update UI automatically
```

## API Integration

Notifications use TanStack Query for data fetching:

```typescript
// Query keys
const notificationKeys = {
  lists: () => ['notifications'] as const,
  list: (filters: NotificationFilters) => ['notifications', filters] as const,
};

// Queries
useQuery({
  queryKey: notificationKeys.lists(),
  queryFn: () => notificationsService.getNotifications(),
});

// Mutations
useMutation({
  mutationFn: (id: string) => notificationsService.markAsRead(id),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: notificationKeys.lists() });
  },
});
```

## Styling

All notification components use Tailwind CSS with the following design tokens:

**Colors:**
- Primary (blue): Task assignments
- Success (green): Status changes
- Warning (orange): Due soon alerts
- Danger (red): Overdue alerts
- Secondary (purple): AI suggestions

**Spacing:**
- Padding: 4px base unit (p-4, p-6)
- Gap: 3 units (gap-3)
- Border radius: rounded-md, rounded-full

**Transitions:**
- Duration: 300ms
- Easing: ease-in-out
- Properties: transform, opacity, colors

## Accessibility

All notification components follow accessibility best practices:

- **Semantic HTML**: Proper use of `<button>`, `<div role="dialog">`, etc.
- **ARIA attributes**: `aria-label`, `aria-modal`, `aria-labelledby`
- **Keyboard navigation**: Focus management, tab order
- **Screen reader support**: Descriptive labels and announcements
- **Color contrast**: WCAG AA compliant color combinations

## Performance Considerations

1. **Lazy loading**: NotificationsPanel only fetches data when opened
2. **Optimistic updates**: UI updates immediately before API confirmation
3. **Query invalidation**: Automatic refetch after mutations
4. **Memoization**: Components use React.memo where appropriate
5. **Virtual scrolling**: Consider implementing for large notification lists

## Testing

### Unit Tests
```typescript
// NotificationItem.test.tsx
describe('NotificationItem', () => {
  it('renders notification message', () => {});
  it('shows unread indicator for unread notifications', () => {});
  it('calls onMarkAsRead when button clicked', () => {});
  it('displays correct icon for notification type', () => {});
  it('formats timestamp correctly', () => {});
});

// NotificationBell.test.tsx
describe('NotificationBell', () => {
  it('shows badge when unread count > 0', () => {});
  it('hides badge when unread count = 0', () => {});
  it('displays "9+" for counts > 9', () => {});
  it('calls onClick when clicked', () => {});
});
```

### Integration Tests
```typescript
describe('Notification Flow', () => {
  it('opens panel when bell clicked', () => {});
  it('marks notification as read', () => {});
  it('marks all notifications as read', () => {});
  it('updates unread count after marking as read', () => {});
  it('receives real-time notifications via SignalR', () => {});
});
```

## Requirements Mapping

- **Requirement 28.1**: Display all notifications ordered by creation time ✅
- **Requirement 28.2**: Mark notification as read and decrement unread count ✅
- **Requirement 28.3**: Mark all as read and reset unread count to zero ✅
- **Requirement 28.4**: Increment unread count badge when new notification arrives ✅
- **Requirement 28.5**: Implement notifications panel as slide-in drawer ✅

## Future Enhancements

1. **Notification filtering**: Filter by type, read/unread status
2. **Notification grouping**: Group related notifications
3. **Notification actions**: Quick actions from notification (e.g., "View Task")
4. **Notification preferences**: User settings for notification types
5. **Push notifications**: Browser push notifications when app is closed
6. **Notification sound**: Optional sound for new notifications
7. **Notification history**: Archive and search old notifications
8. **Batch operations**: Select multiple notifications for bulk actions

## Related Components

- **Header**: Uses NotificationBell
- **MainLayout**: Manages NotificationsPanel state
- **Toast**: Shows temporary notification alerts
- **Badge**: Used for unread count display

## Related Files

- `src/api/notifications.service.ts`: API service for notifications
- `src/store/notificationStore.ts`: Zustand store for notification state
- `src/hooks/useSignalR.ts`: SignalR hook for real-time updates
- `src/api/queryKeys.ts`: TanStack Query keys for notifications
