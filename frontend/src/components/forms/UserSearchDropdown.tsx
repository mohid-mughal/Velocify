import React, { useState, useRef, useEffect, useMemo } from 'react';
import { clsx } from 'clsx';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role?: string;
}

export interface UserSearchDropdownProps {
  name: string;
  label?: string;
  users: User[];
  value: string | null;
  onChange: (userId: string | null) => void;
  error?: string;
  helperText?: string;
  placeholder?: string;
  disabled?: boolean;
  fullWidth?: boolean;
  allowClear?: boolean;
}

/**
 * UserSearchDropdown component provides a searchable dropdown for selecting users
 * Displays user name and email with avatar placeholder
 * 
 * @example
 * ```tsx
 * <UserSearchDropdown
 *   name="assignedTo"
 *   label="Assign To"
 *   users={teamMembers}
 *   value={assignedUserId}
 *   onChange={setAssignedUserId}
 *   placeholder="Search users..."
 *   allowClear
 * />
 * ```
 */
export const UserSearchDropdown: React.FC<UserSearchDropdownProps> = ({
  name,
  label,
  users,
  value,
  onChange,
  error,
  helperText,
  placeholder = 'Search users...',
  disabled = false,
  fullWidth = false,
  allowClear = true,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownId = `usersearch-${name}`;
  const errorId = error ? `${dropdownId}-error` : undefined;
  const helperId = helperText ? `${dropdownId}-helper` : undefined;

  const selectedUser = users.find((u) => u.id === value);

  // Filter users based on search query
  const filteredUsers = useMemo(() => {
    if (!searchQuery.trim()) return users;
    
    const query = searchQuery.toLowerCase();
    return users.filter(
      (user) =>
        user.firstName.toLowerCase().includes(query) ||
        user.lastName.toLowerCase().includes(query) ||
        user.email.toLowerCase().includes(query)
    );
  }, [users, searchQuery]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
        setSearchQuery('');
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
      return () => document.removeEventListener('mousedown', handleClickOutside);
    }
  }, [isOpen]);

  // Focus input when dropdown opens
  useEffect(() => {
    if (isOpen && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isOpen]);

  const handleSelectUser = (user: User) => {
    onChange(user.id);
    setIsOpen(false);
    setSearchQuery('');
  };

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onChange(null);
  };

  const getInitials = (user: User) => {
    return `${user.firstName[0]}${user.lastName[0]}`.toUpperCase();
  };

  return (
    <div className={clsx('flex flex-col gap-1', fullWidth && 'w-full')} ref={containerRef}>
      {label && (
        <label htmlFor={dropdownId} className="text-sm font-medium text-neutral-700">
          {label}
        </label>
      )}

      <div className="relative">
        {!isOpen && selectedUser ? (
          <button
            type="button"
            onClick={() => !disabled && setIsOpen(true)}
            disabled={disabled}
            className={clsx(
              'w-full px-3 py-2 border rounded-md text-base transition-colors text-left',
              'focus:outline-none focus:ring-2 focus:ring-offset-1',
              'disabled:bg-neutral-100 disabled:cursor-not-allowed',
              error
                ? 'border-danger-500 focus:ring-danger-500'
                : 'border-neutral-300 focus:ring-primary-500',
              'bg-white'
            )}
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 rounded-full bg-primary-600 text-white flex items-center justify-center text-sm font-medium">
                  {getInitials(selectedUser)}
                </div>
                <div className="flex flex-col">
                  <span className="text-sm font-medium">
                    {selectedUser.firstName} {selectedUser.lastName}
                  </span>
                  <span className="text-xs text-neutral-500">{selectedUser.email}</span>
                </div>
              </div>
              {allowClear && (
                <button
                  type="button"
                  onClick={handleClear}
                  disabled={disabled}
                  className="p-1 hover:bg-neutral-100 rounded focus:outline-none focus:ring-2 focus:ring-primary-500"
                  aria-label="Clear selection"
                >
                  <svg
                    className="w-4 h-4 text-neutral-500"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              )}
            </div>
          </button>
        ) : (
          <div
            className={clsx(
              'w-full px-3 py-2 border rounded-md transition-colors',
              'focus-within:ring-2 focus-within:ring-offset-1',
              error
                ? 'border-danger-500 focus-within:ring-danger-500'
                : 'border-neutral-300 focus-within:ring-primary-500',
              'bg-white'
            )}
          >
            <input
              ref={inputRef}
              type="text"
              id={dropdownId}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onFocus={() => setIsOpen(true)}
              disabled={disabled}
              placeholder={placeholder}
              aria-expanded={isOpen}
              aria-haspopup="listbox"
              aria-describedby={clsx(errorId, helperId).trim() || undefined}
              className="w-full outline-none bg-transparent text-base disabled:cursor-not-allowed"
            />
          </div>
        )}

        {isOpen && (
          <div
            className="absolute z-10 w-full mt-1 bg-white border border-neutral-300 rounded-md shadow-lg max-h-60 overflow-auto"
            role="listbox"
          >
            {filteredUsers.length === 0 ? (
              <div className="px-3 py-2 text-sm text-neutral-500">
                {searchQuery ? 'No users found' : 'No users available'}
              </div>
            ) : (
              filteredUsers.map((user) => (
                <button
                  key={user.id}
                  type="button"
                  onClick={() => handleSelectUser(user)}
                  role="option"
                  aria-selected={user.id === value}
                  className={clsx(
                    'w-full px-3 py-2 text-left transition-colors',
                    'hover:bg-neutral-100 focus:bg-neutral-100 focus:outline-none',
                    user.id === value && 'bg-primary-50'
                  )}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-8 h-8 rounded-full bg-primary-600 text-white flex items-center justify-center text-sm font-medium flex-shrink-0">
                      {getInitials(user)}
                    </div>
                    <div className="flex flex-col min-w-0">
                      <span className="text-sm font-medium truncate">
                        {user.firstName} {user.lastName}
                      </span>
                      <span className="text-xs text-neutral-500 truncate">{user.email}</span>
                    </div>
                  </div>
                </button>
              ))
            )}
          </div>
        )}
      </div>

      {error && (
        <span id={errorId} className="text-sm text-danger-600" role="alert">
          {error}
        </span>
      )}
      {helperText && !error && (
        <span id={helperId} className="text-sm text-neutral-500">
          {helperText}
        </span>
      )}
    </div>
  );
};

UserSearchDropdown.displayName = 'UserSearchDropdown';
