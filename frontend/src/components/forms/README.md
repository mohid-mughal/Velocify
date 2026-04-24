# Form Components

This directory contains reusable form components designed for the Velocify platform. All components are built with React 18, TypeScript, Tailwind CSS, and are designed to work seamlessly with React Hook Form.

## Components

### FormField

A wrapper component that combines an Input with label and error display. Simplifies form field creation with consistent styling.

**Props:**
- `name` (string, required): Field name for form integration
- `label` (string, required): Label text displayed above the input
- `error` (string, optional): Error message to display
- `required` (boolean, optional): Whether the field is required
- All standard HTML input props

**Usage:**
```tsx
<FormField
  name="title"
  label="Task Title"
  error={errors.title?.message}
  required
  {...register('title')}
/>
```

### MultiSelect

A multi-selection dropdown component that displays selected items as removable badges.

**Props:**
- `name` (string, required): Field name
- `label` (string, optional): Label text
- `options` (MultiSelectOption[], required): Array of options
- `value` (string[], required): Array of selected values
- `onChange` ((value: string[]) => void, required): Change handler
- `error` (string, optional): Error message
- `helperText` (string, optional): Helper text
- `placeholder` (string, optional): Placeholder text
- `disabled` (boolean, optional): Disable the component
- `fullWidth` (boolean, optional): Take full width

**Usage:**
```tsx
<Controller
  name="categories"
  control={control}
  render={({ field }) => (
    <MultiSelect
      name="categories"
      label="Categories"
      options={categoryOptions}
      value={field.value}
      onChange={field.onChange}
      placeholder="Select categories..."
      fullWidth
    />
  )}
/>
```

### TagInput

A chip-based input component for entering multiple tags. Tags are added by pressing Enter or comma.

**Props:**
- `name` (string, required): Field name
- `label` (string, optional): Label text
- `value` (string[], required): Array of tags
- `onChange` ((value: string[]) => void, required): Change handler
- `error` (string, optional): Error message
- `helperText` (string, optional): Helper text
- `placeholder` (string, optional): Placeholder text
- `disabled` (boolean, optional): Disable the component
- `fullWidth` (boolean, optional): Take full width
- `maxTags` (number, optional): Maximum number of tags allowed
- `allowDuplicates` (boolean, optional): Allow duplicate tags (default: false)

**Usage:**
```tsx
<Controller
  name="tags"
  control={control}
  render={({ field }) => (
    <TagInput
      name="tags"
      label="Tags"
      value={field.value}
      onChange={field.onChange}
      placeholder="Type and press Enter..."
      maxTags={10}
      fullWidth
    />
  )}
/>
```

**Keyboard Shortcuts:**
- `Enter` or `,` (comma): Add current input as a tag
- `Backspace` on empty input: Remove the last tag

### UserSearchDropdown

A searchable dropdown for selecting users. Displays user avatars, names, and emails.

**Props:**
- `name` (string, required): Field name
- `label` (string, optional): Label text
- `users` (User[], required): Array of user objects
- `value` (string | null, required): Selected user ID
- `onChange` ((userId: string | null) => void, required): Change handler
- `error` (string, optional): Error message
- `helperText` (string, optional): Helper text
- `placeholder` (string, optional): Placeholder text
- `disabled` (boolean, optional): Disable the component
- `fullWidth` (boolean, optional): Take full width
- `allowClear` (boolean, optional): Show clear button (default: true)

**User Interface:**
```typescript
interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role?: string;
}
```

**Usage:**
```tsx
<Controller
  name="assignedTo"
  control={control}
  render={({ field }) => (
    <UserSearchDropdown
      name="assignedTo"
      label="Assign To"
      users={teamMembers}
      value={field.value}
      onChange={field.onChange}
      placeholder="Search users..."
      allowClear
      fullWidth
    />
  )}
/>
```

## Integration with React Hook Form

All components are designed to work with React Hook Form:

1. **FormField**: Use with `register()` directly
2. **MultiSelect, TagInput, UserSearchDropdown**: Use with `Controller` component

Example:
```tsx
import { useForm, Controller } from 'react-hook-form';
import { FormField, MultiSelect, TagInput, UserSearchDropdown } from '@/components/forms';

const MyForm = () => {
  const { register, control, handleSubmit, formState: { errors } } = useForm();

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <FormField
        name="title"
        label="Title"
        error={errors.title?.message}
        {...register('title', { required: 'Title is required' })}
      />

      <Controller
        name="categories"
        control={control}
        render={({ field }) => (
          <MultiSelect
            name="categories"
            label="Categories"
            options={options}
            value={field.value}
            onChange={field.onChange}
          />
        )}
      />
    </form>
  );
};
```

## Accessibility

All components follow accessibility best practices:

- Proper ARIA attributes (`aria-label`, `aria-describedby`, `aria-invalid`, etc.)
- Keyboard navigation support
- Focus management
- Screen reader friendly
- Error announcements with `role="alert"`

## Styling

Components use Tailwind CSS with the design system defined in `tailwind.config.js`:

- Primary color: `primary-*` (blue)
- Danger color: `danger-*` (red)
- Neutral colors: `neutral-*` (gray)
- Consistent spacing and sizing
- Focus rings for accessibility

## Example

See `FormComponentsExample.tsx` for a complete working example demonstrating all components with React Hook Form integration.

## Requirements Mapping

These components satisfy the following requirements:

- **Requirement 24.1-24.5**: Natural Language Task Form (form components with React Hook Form)
- **Requirement 22**: Task List Interface (form components for filtering)
- **Requirement 23**: Task Detail and Editing Interface (form components for task editing)

## Task Completion

This implementation completes **Task 20.3: Create form components** from the Velocify platform spec.

### Sub-tasks Completed:
- ✅ FormField component (wraps input with label and error)
- ✅ MultiSelect component
- ✅ TagInput component (multi-input chip field)
- ✅ UserSearchDropdown component

All components are:
- Fully typed with TypeScript
- Integrated with React Hook Form
- Styled with Tailwind CSS
- Accessible (WCAG compliant patterns)
- Documented with examples
