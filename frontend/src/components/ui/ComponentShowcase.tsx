/**
 * Component Showcase
 * 
 * This file demonstrates all UI components in action.
 * Use this as a reference for component usage and styling.
 */

import React, { useState } from 'react';
import {
  Button,
  Input,
  Select,
  DatePicker,
  Modal,
  ToastContainer,
  Spinner,
  LoadingOverlay,
  Badge,
  PriorityBadge,
  StatusBadge,
  Avatar,
  AvatarGroup,
} from './index';
import type { ToastProps } from './Toast';

export const ComponentShowcase: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [toasts, setToasts] = useState<Array<Omit<ToastProps, 'onClose'>>>([]);
  const [isLoading, setIsLoading] = useState(false);

  const handleAddToast = (type: 'success' | 'error' | 'warning' | 'info') => {
    const newToast = {
      id: Date.now().toString(),
      type,
      message: `This is a ${type} toast notification!`,
      duration: 5000,
    };
    setToasts((prev) => [...prev, newToast]);
  };

  const handleCloseToast = (id: string) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  };

  return (
    <div className="p-8 space-y-12 max-w-6xl mx-auto">
      <h1 className="text-4xl font-bold text-neutral-900">UI Component Showcase</h1>

      {/* Buttons */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Buttons</h2>
        <div className="flex flex-wrap gap-4">
          <Button variant="primary" size="sm">Primary Small</Button>
          <Button variant="primary" size="md">Primary Medium</Button>
          <Button variant="primary" size="lg">Primary Large</Button>
        </div>
        <div className="flex flex-wrap gap-4">
          <Button variant="secondary">Secondary</Button>
          <Button variant="danger">Danger</Button>
          <Button isLoading>Loading...</Button>
          <Button disabled>Disabled</Button>
        </div>
      </section>

      {/* Inputs */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Inputs</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input label="Email" type="email" placeholder="Enter your email" />
          <Input label="Password" type="password" placeholder="Enter password" />
          <Input label="With Error" error="This field is required" />
          <Input label="With Helper" helperText="This is helper text" />
        </div>
      </section>

      {/* Select */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Select</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Select
            label="Priority"
            options={[
              { value: 'low', label: 'Low' },
              { value: 'medium', label: 'Medium' },
              { value: 'high', label: 'High' },
              { value: 'critical', label: 'Critical' },
            ]}
            placeholder="Select priority"
          />
          <Select
            label="Status"
            options={[
              { value: 'pending', label: 'Pending' },
              { value: 'in-progress', label: 'In Progress' },
              { value: 'completed', label: 'Completed' },
            ]}
            error="Please select a status"
          />
        </div>
      </section>

      {/* DatePicker */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Date Picker</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <DatePicker label="Due Date" />
          <DatePicker label="Start Date" helperText="Select a start date" />
        </div>
      </section>

      {/* Modal */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Modal</h2>
        <Button onClick={() => setIsModalOpen(true)}>Open Modal</Button>
        <Modal
          isOpen={isModalOpen}
          onClose={() => setIsModalOpen(false)}
          title="Example Modal"
          size="md"
        >
          <p className="text-neutral-700 mb-4">
            This is an example modal dialog. You can put any content here.
          </p>
          <div className="flex gap-2">
            <Button onClick={() => setIsModalOpen(false)}>Confirm</Button>
            <Button variant="secondary" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
          </div>
        </Modal>
      </section>

      {/* Toasts */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Toasts</h2>
        <div className="flex flex-wrap gap-4">
          <Button onClick={() => handleAddToast('success')}>Success Toast</Button>
          <Button onClick={() => handleAddToast('error')}>Error Toast</Button>
          <Button onClick={() => handleAddToast('warning')}>Warning Toast</Button>
          <Button onClick={() => handleAddToast('info')}>Info Toast</Button>
        </div>
        <ToastContainer toasts={toasts} onClose={handleCloseToast} position="top-right" />
      </section>

      {/* Spinners */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Spinners</h2>
        <div className="flex flex-wrap gap-8 items-center">
          <Spinner size="sm" />
          <Spinner size="md" />
          <Spinner size="lg" />
          <Spinner size="xl" />
          <Spinner color="secondary" />
        </div>
        <div className="mt-4">
          <Button onClick={() => setIsLoading(!isLoading)}>
            Toggle Loading Overlay
          </Button>
          <div className="mt-4 h-40 border border-neutral-300 rounded-lg">
            <LoadingOverlay isLoading={isLoading} message="Loading content...">
              <div className="p-4">
                <p>This content is behind a loading overlay</p>
              </div>
            </LoadingOverlay>
          </div>
        </div>
      </section>

      {/* Badges */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Badges</h2>
        <div className="space-y-2">
          <div className="flex flex-wrap gap-2">
            <Badge variant="default">Default</Badge>
            <Badge variant="primary">Primary</Badge>
            <Badge variant="secondary">Secondary</Badge>
            <Badge variant="success">Success</Badge>
            <Badge variant="warning">Warning</Badge>
            <Badge variant="danger">Danger</Badge>
          </div>
          <div className="flex flex-wrap gap-2">
            <PriorityBadge priority="Low" />
            <PriorityBadge priority="Medium" />
            <PriorityBadge priority="High" />
            <PriorityBadge priority="Critical" />
          </div>
          <div className="flex flex-wrap gap-2">
            <StatusBadge status="Pending" />
            <StatusBadge status="InProgress" />
            <StatusBadge status="Completed" />
            <StatusBadge status="Cancelled" />
            <StatusBadge status="Blocked" />
          </div>
        </div>
      </section>

      {/* Avatars */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold text-neutral-800">Avatars</h2>
        <div className="space-y-4">
          <div className="flex flex-wrap gap-4 items-center">
            <Avatar name="John Doe" size="xs" />
            <Avatar name="Jane Smith" size="sm" />
            <Avatar name="Bob Johnson" size="md" />
            <Avatar name="Alice Williams" size="lg" />
            <Avatar name="Charlie Brown" size="xl" />
          </div>
          <div>
            <AvatarGroup
              avatars={[
                { name: 'John Doe' },
                { name: 'Jane Smith' },
                { name: 'Bob Johnson' },
                { name: 'Alice Williams' },
                { name: 'Charlie Brown' },
              ]}
              max={3}
              size="md"
            />
          </div>
        </div>
      </section>
    </div>
  );
};
