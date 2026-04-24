import React from 'react';
import { clsx } from 'clsx';

export type AvatarSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';

export interface AvatarProps {
  src?: string;
  alt?: string;
  name?: string;
  size?: AvatarSize;
  className?: string;
}

const getInitials = (name: string): string => {
  const parts = name.trim().split(' ');
  if (parts.length === 1) {
    return parts[0].charAt(0).toUpperCase();
  }
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
};

const getColorFromName = (name: string): string => {
  const colors = [
    'bg-primary-500',
    'bg-secondary-500',
    'bg-success-500',
    'bg-warning-500',
    'bg-danger-500',
    'bg-neutral-500',
  ];
  
  const hash = name.split('').reduce((acc, char) => {
    return char.charCodeAt(0) + ((acc << 5) - acc);
  }, 0);
  
  return colors[Math.abs(hash) % colors.length];
};

export const Avatar: React.FC<AvatarProps> = ({
  src,
  alt,
  name = '',
  size = 'md',
  className,
}) => {
  const [imageError, setImageError] = React.useState(false);

  const sizeStyles = {
    xs: 'w-6 h-6 text-xs',
    sm: 'w-8 h-8 text-sm',
    md: 'w-10 h-10 text-base',
    lg: 'w-12 h-12 text-lg',
    xl: 'w-16 h-16 text-xl',
  };

  const showImage = src && !imageError;
  const initials = name ? getInitials(name) : '?';
  const bgColor = name ? getColorFromName(name) : 'bg-neutral-400';

  return (
    <div
      className={clsx(
        'inline-flex items-center justify-center rounded-full overflow-hidden flex-shrink-0',
        sizeStyles[size],
        !showImage && bgColor,
        !showImage && 'text-white font-semibold',
        className
      )}
      title={alt || name}
    >
      {showImage ? (
        <img
          src={src}
          alt={alt || name}
          className="w-full h-full object-cover"
          onError={() => setImageError(true)}
        />
      ) : (
        <span>{initials}</span>
      )}
    </div>
  );
};

export interface AvatarGroupProps {
  avatars: Array<{
    src?: string;
    alt?: string;
    name?: string;
  }>;
  max?: number;
  size?: AvatarSize;
  className?: string;
}

export const AvatarGroup: React.FC<AvatarGroupProps> = ({
  avatars,
  max = 3,
  size = 'md',
  className,
}) => {
  const displayAvatars = avatars.slice(0, max);
  const remaining = avatars.length - max;

  return (
    <div className={clsx('flex -space-x-2', className)}>
      {displayAvatars.map((avatar, index) => (
        <Avatar
          key={index}
          {...avatar}
          size={size}
          className="ring-2 ring-white"
        />
      ))}
      {remaining > 0 && (
        <div
          className={clsx(
            'inline-flex items-center justify-center rounded-full bg-neutral-200 text-neutral-700 font-semibold ring-2 ring-white',
            size === 'xs' && 'w-6 h-6 text-xs',
            size === 'sm' && 'w-8 h-8 text-sm',
            size === 'md' && 'w-10 h-10 text-base',
            size === 'lg' && 'w-12 h-12 text-lg',
            size === 'xl' && 'w-16 h-16 text-xl'
          )}
          title={`${remaining} more`}
        >
          +{remaining}
        </div>
      )}
    </div>
  );
};
