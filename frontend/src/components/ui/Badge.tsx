import React from 'react';
import { clsx } from 'clsx';

export type BadgeVariant = 'default' | 'primary' | 'secondary' | 'success' | 'warning' | 'danger';
export type BadgeSize = 'sm' | 'md' | 'lg';

export interface BadgeProps {
  children: React.ReactNode;
  variant?: BadgeVariant;
  size?: BadgeSize;
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({
  children,
  variant = 'default',
  size = 'md',
  className,
}) => {
  const baseStyles = 'inline-flex items-center justify-center font-medium rounded-full';

  const variantStyles = {
    default: 'bg-neutral-100 text-neutral-800',
    primary: 'bg-primary-100 text-primary-800',
    secondary: 'bg-secondary-100 text-secondary-800',
    success: 'bg-success-100 text-success-800',
    warning: 'bg-warning-100 text-warning-800',
    danger: 'bg-danger-100 text-danger-800',
  };

  const sizeStyles = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-2.5 py-1 text-sm',
    lg: 'px-3 py-1.5 text-base',
  };

  return (
    <span
      className={clsx(
        baseStyles,
        variantStyles[variant],
        sizeStyles[size],
        className
      )}
    >
      {children}
    </span>
  );
};

// Priority Badge Component
export interface PriorityBadgeProps {
  priority: 'Critical' | 'High' | 'Medium' | 'Low';
  size?: BadgeSize;
  className?: string;
}

export const PriorityBadge: React.FC<PriorityBadgeProps> = ({
  priority,
  size = 'md',
  className,
}) => {
  const variantMap: Record<string, BadgeVariant> = {
    Critical: 'danger',
    High: 'warning',
    Medium: 'primary',
    Low: 'default',
  };

  return (
    <Badge variant={variantMap[priority]} size={size} className={className}>
      {priority}
    </Badge>
  );
};

// Status Badge Component
export interface StatusBadgeProps {
  status: 'Pending' | 'InProgress' | 'Completed' | 'Cancelled' | 'Blocked';
  size?: BadgeSize;
  className?: string;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({
  status,
  size = 'md',
  className,
}) => {
  const variantMap: Record<string, BadgeVariant> = {
    Pending: 'default',
    InProgress: 'primary',
    Completed: 'success',
    Cancelled: 'secondary',
    Blocked: 'danger',
  };

  const labelMap: Record<string, string> = {
    InProgress: 'In Progress',
    Pending: 'Pending',
    Completed: 'Completed',
    Cancelled: 'Cancelled',
    Blocked: 'Blocked',
  };

  return (
    <Badge variant={variantMap[status]} size={size} className={className}>
      {labelMap[status]}
    </Badge>
  );
};
