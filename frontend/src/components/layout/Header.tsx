import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { clsx } from 'clsx';
import { Avatar } from '../ui/Avatar';
import { Badge } from '../ui/Badge';
import { useAuthStore } from '../../store/authStore';
import { useUnreadCount } from '../../store/notificationStore';
import { NotificationsPanel } from '../notifications/NotificationsPanel';
import { NotificationBell } from '../notifications/NotificationBell';

export interface HeaderProps {
  onMenuClick?: () => void;
  className?: string;
}

export const Header: React.FC<HeaderProps> = ({ onMenuClick, className }) => {
  const { user, logout } = useAuthStore();
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);

  // Get unread count from notification store
  const unreadCount = useUnreadCount();

  const handleLogout = () => {
    logout();
    setShowUserMenu(false);
  };

  return (
    <header
      className={clsx(
        'bg-white border-b border-neutral-200 sticky top-0 z-40',
        className
      )}
    >
      <div className="flex items-center justify-between h-16 px-4 sm:px-6 lg:px-8">
        {/* Left section: Menu button (mobile) + Logo */}
        <div className="flex items-center gap-4">
          {/* Mobile menu button */}
          <button
            onClick={onMenuClick}
            className="lg:hidden p-2 rounded-md text-neutral-600 hover:bg-neutral-100 focus:outline-none focus:ring-2 focus:ring-primary-500"
            aria-label="Open menu"
          >
            <svg
              className="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4 6h16M4 12h16M4 18h16"
              />
            </svg>
          </button>

          {/* Logo */}
          <Link to="/" className="flex items-center gap-2">
            <div className="w-8 h-8 bg-gradient-to-br from-primary-500 to-secondary-500 rounded-lg flex items-center justify-center">
              <span className="text-white font-bold text-lg">V</span>
            </div>
            <span className="text-xl font-bold text-neutral-900 hidden sm:block">
              Velocify
            </span>
          </Link>
        </div>

        {/* Center section: Navigation (desktop) */}
        <nav className="hidden lg:flex items-center gap-1">
          <Link
            to="/dashboard"
            className="px-4 py-2 text-sm font-medium text-neutral-700 hover:text-primary-600 hover:bg-primary-50 rounded-md transition-colors"
          >
            Dashboard
          </Link>
          <Link
            to="/tasks"
            className="px-4 py-2 text-sm font-medium text-neutral-700 hover:text-primary-600 hover:bg-primary-50 rounded-md transition-colors"
          >
            Tasks
          </Link>
          {user?.role !== 'Member' && (
            <Link
              to="/admin"
              className="px-4 py-2 text-sm font-medium text-neutral-700 hover:text-primary-600 hover:bg-primary-50 rounded-md transition-colors"
            >
              Admin
            </Link>
          )}
        </nav>

        {/* Right section: Notifications + User menu */}
        <div className="flex items-center gap-3">
          {/* Notification bell */}
          <NotificationBell
            unreadCount={unreadCount}
            onClick={() => setShowNotifications(!showNotifications)}
          />

          {/* User menu */}
          <div className="relative">
            <button
              onClick={() => setShowUserMenu(!showUserMenu)}
              className="flex items-center gap-2 p-1 rounded-md hover:bg-neutral-100 focus:outline-none focus:ring-2 focus:ring-primary-500"
              aria-label="User menu"
            >
              <Avatar
                name={user ? `${user.firstName} ${user.lastName}` : 'User'}
                size="sm"
              />
              <span className="hidden sm:block text-sm font-medium text-neutral-700">
                {user?.firstName}
              </span>
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
                  d="M19 9l-7 7-7-7"
                />
              </svg>
            </button>

            {/* User dropdown menu */}
            {showUserMenu && (
              <div className="absolute right-0 mt-2 w-56 bg-white rounded-lg shadow-lg border border-neutral-200 py-2 animate-fade-in">
                <div className="px-4 py-3 border-b border-neutral-200">
                  <p className="text-sm font-medium text-neutral-900">
                    {user?.firstName} {user?.lastName}
                  </p>
                  <p className="text-xs text-neutral-500 mt-0.5">
                    {user?.email}
                  </p>
                  <div className="mt-2">
                    <Badge variant="primary" size="sm">
                      {user?.role}
                    </Badge>
                  </div>
                </div>
                <div className="py-1">
                  <Link
                    to="/profile"
                    className="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-100"
                    onClick={() => setShowUserMenu(false)}
                  >
                    Profile
                  </Link>
                  <Link
                    to="/settings"
                    className="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-100"
                    onClick={() => setShowUserMenu(false)}
                  >
                    Settings
                  </Link>
                </div>
                <div className="border-t border-neutral-200 py-1">
                  <button
                    onClick={handleLogout}
                    className="block w-full text-left px-4 py-2 text-sm text-danger-600 hover:bg-danger-50"
                  >
                    Sign out
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Notifications Panel */}
      <NotificationsPanel isOpen={showNotifications} onClose={() => setShowNotifications(false)} />
    </header>
  );
};
