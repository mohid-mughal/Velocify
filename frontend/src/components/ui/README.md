# UI Components

This directory contains reusable UI components for the Velocify platform. All components are built with React 18, TypeScript, and Tailwind CSS, following accessibility best practices.

## Components

### Button
A versatile button component with multiple variants and sizes.

**Variants:** `primary`, `secondary`, `danger`  
**Sizes:** `sm`, `md`, `lg`

```tsx
import { Button } from '@/components/ui';

<Button variant="primary" size="md" onClick={handleClick}>
  Click Me
</Button>

<Button variant="danger" isLoading>
  Deleting...
</Button>
```

### Input
A text input component with label, error display, and helper text support.

```tsx
import { Input } from '@/components/ui';

<Input
  label="Email"
  type="email"
  placeholder="Enter your email"
  error={errors.email}
  helperText="We'll never share your email"
  fullWidth
/>
```

### Select
A dropdown select component with label and error display.

```tsx
import { Select } from '@/components/ui';

<Select
  label="Priority"
  options={[
    { value: 'low', label: 'Low' },
    { value: 'medium', label: 'Medium' },
    { value: 'high', label: 'High' },
  ]}
  placeholder="Select priority"
  error={errors.priority}
/>
```

### DatePicker
A date input component with label and error display.

```tsx
import { DatePicker } from '@/components/ui';

<DatePicker
  label="Due Date"
  error={errors.dueDate}
  fullWidth
/>
```

### Modal
A modal dialog component with customizable size and close behavior.

**Sizes:** `sm`, `md`, `lg`, `xl`

```tsx
import { Modal } from '@/components/ui';

<Modal
  isOpen={isOpen}
  onClose={handleClose}
  title="Confirm Action"
  size="md"
>
  <p>Are you sure you want to proceed?</p>
  <div className="flex gap-2 mt-4">
    <Button onClick={handleConfirm}>Confirm</Button>
    <Button variant="secondary" onClick={handleClose}>Cancel</Button>
  </div>
</Modal>
```

### Toast
Toast notification components for displaying temporary messages.

**Types:** `success`, `error`, `warning`, `info`

```tsx
import { ToastContainer } from '@/components/ui';

// In your app root
<ToastContainer
  toasts={toasts}
  onClose={handleCloseToast}
  position="top-right"
/>

// To show a toast, add to your toasts array:
const newToast = {
  id: Date.now().toString(),
  type: 'success',
  message: 'Task created successfully!',
  duration: 5000,
};
```

### Spinner
Loading spinner component with size and color variants.

**Sizes:** `sm`, `md`, `lg`, `xl`  
**Colors:** `primary`, `secondary`, `white`

```tsx
import { Spinner, LoadingOverlay } from '@/components/ui';

// Standalone spinner
<Spinner size="lg" color="primary" />

// Loading overlay
<LoadingOverlay isLoading={isLoading} message="Loading tasks...">
  <TaskList />
</LoadingOverlay>
```

### Badge
Badge components for displaying status, priority, and other labels.

**Variants:** `default`, `primary`, `secondary`, `success`, `warning`, `danger`  
**Sizes:** `sm`, `md`, `lg`

```tsx
import { Badge, PriorityBadge, StatusBadge } from '@/components/ui';

// Generic badge
<Badge variant="primary" size="md">New</Badge>

// Priority badge
<PriorityBadge priority="High" />

// Status badge
<StatusBadge status="InProgress" />
```

### Avatar
Avatar component for displaying user profile pictures or initials.

**Sizes:** `xs`, `sm`, `md`, `lg`, `xl`

```tsx
import { Avatar, AvatarGroup } from '@/components/ui';

// Single avatar
<Avatar
  src="/path/to/image.jpg"
  name="John Doe"
  size="md"
/>

// Avatar with initials (when no image)
<Avatar name="Jane Smith" size="lg" />

// Avatar group
<AvatarGroup
  avatars={[
    { name: 'John Doe', src: '/john.jpg' },
    { name: 'Jane Smith' },
    { name: 'Bob Johnson' },
  ]}
  max={3}
  size="sm"
/>
```

## Accessibility

All components follow WCAG 2.1 Level AA guidelines:

- Proper ARIA attributes
- Keyboard navigation support
- Focus management
- Screen reader compatibility
- Color contrast compliance

## Styling

Components use Tailwind CSS with the custom design system defined in `tailwind.config.js`. The color palette includes:

- **Primary:** Blue tones for main actions
- **Secondary:** Purple tones for secondary actions
- **Success:** Green tones for positive feedback
- **Warning:** Yellow/orange tones for warnings
- **Danger:** Red tones for destructive actions
- **Neutral:** Gray tones for text and backgrounds

## Integration with React Hook Form

These components work seamlessly with React Hook Form:

```tsx
import { useForm } from 'react-hook-form';
import { Input, Button } from '@/components/ui';

const { register, handleSubmit, formState: { errors } } = useForm();

<form onSubmit={handleSubmit(onSubmit)}>
  <Input
    {...register('email', { required: 'Email is required' })}
    label="Email"
    error={errors.email?.message}
  />
  <Button type="submit">Submit</Button>
</form>
```
