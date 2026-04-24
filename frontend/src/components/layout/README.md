# Layout Components

This directory contains the main layout components for the Velocify platform.

## Components

### Header

The Header component provides the top navigation bar with:
- Logo and branding
- Mobile menu toggle button
- Desktop navigation links (Dashboard, Tasks, Admin)
- Notification bell with unread count badge
- User menu with profile, settings, and logout options

**Props:**
- `onMenuClick?: () => void` - Callback for mobile menu button click
- `className?: string` - Additional CSS classes

**Features:**
- Sticky positioning for persistent navigation
- Responsive design with mobile-friendly menu
- Dropdown menus for notifications and user actions
- Role-based navigation (Admin link only for Admin/SuperAdmin)

### Sidebar

The Sidebar component provides the main navigation menu with:
- Navigation links with icons
- Active route highlighting
- Role-based access control
- AI Assistant quick access button

**Props:**
- `isOpen?: boolean` - Controls sidebar visibility (mobile)
- `onClose?: () => void` - Callback for closing sidebar (mobile)
- `className?: string` - Additional CSS classes

**Features:**
- Fixed sidebar on desktop
- Slide-in drawer on mobile with overlay
- Active route highlighting
- Role-based menu filtering
- AI Assistant button at bottom

**Navigation Items:**
- Dashboard - Home page with analytics
- Tasks - All tasks list
- My Tasks - User's assigned tasks
- Create Task - New task form
- Admin - User management (Admin/SuperAdmin only)
- Profile - User profile page

### Footer

The Footer component provides the bottom section with:
- Brand information
- Quick links
- Support links
- Copyright notice

**Props:**
- `className?: string` - Additional CSS classes

**Features:**
- Responsive grid layout
- Quick access to important pages
- Support and legal links

### MainLayout

The MainLayout component combines all layout elements:
- Header at top
- Sidebar on left (optional)
- Main content area
- Footer at bottom (optional)

**Props:**
- `children: React.ReactNode` - Page content
- `className?: string` - Additional CSS classes for content area
- `showSidebar?: boolean` - Show/hide sidebar (default: true)
- `showFooter?: boolean` - Show/hide footer (default: true)

**Features:**
- Responsive layout with mobile support
- Sidebar toggle state management
- Flexible content area
- Proper spacing and overflow handling

## Usage

### Basic Layout

```tsx
import { MainLayout } from '@/components/layout';

function DashboardPage() {
  return (
    <MainLayout>
      <h1>Dashboard</h1>
      {/* Page content */}
    </MainLayout>
  );
}
```

### Layout Without Sidebar

```tsx
import { MainLayout } from '@/components/layout';

function LoginPage() {
  return (
    <MainLayout showSidebar={false} showFooter={false}>
      <div className="max-w-md mx-auto">
        {/* Login form */}
      </div>
    </MainLayout>
  );
}
```

### Custom Header Usage

```tsx
import { Header } from '@/components/layout';

function CustomPage() {
  return (
    <div>
      <Header onMenuClick={() => console.log('Menu clicked')} />
      {/* Custom content */}
    </div>
  );
}
```

## Styling

All layout components use Tailwind CSS with the project's custom theme:
- Primary colors for branding and active states
- Neutral colors for backgrounds and borders
- Responsive breakpoints (sm, md, lg)
- Custom animations (fade-in, slide-in)

## Accessibility

- Semantic HTML elements (header, nav, main, footer, aside)
- ARIA labels for icon buttons
- Keyboard navigation support
- Focus indicators on interactive elements
- Screen reader friendly

## Mobile Responsiveness

- Header: Hamburger menu on mobile, full navigation on desktop
- Sidebar: Slide-in drawer on mobile, fixed sidebar on desktop
- Footer: Stacked layout on mobile, grid layout on desktop
- Content: Responsive padding and max-width constraints

## Integration with Auth Store

The layout components integrate with the Zustand auth store:
- User information displayed in header
- Role-based navigation filtering
- Logout functionality
- User avatar and name display

## Future Enhancements

- Real-time notification updates via SignalR
- Notification panel component
- AI Assistant drawer component
- Breadcrumb navigation
- Search functionality in header
- Theme switcher (light/dark mode)
