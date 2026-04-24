import React from 'react';
import { Link } from 'react-router-dom';
import { clsx } from 'clsx';

export interface FooterProps {
  className?: string;
}

export const Footer: React.FC<FooterProps> = ({ className }) => {
  const currentYear = new Date().getFullYear();

  return (
    <footer
      className={clsx(
        'bg-white border-t border-neutral-200 mt-auto',
        className
      )}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {/* Brand section */}
          <div className="space-y-3">
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 bg-gradient-to-br from-primary-500 to-secondary-500 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-lg">V</span>
              </div>
              <span className="text-lg font-bold text-neutral-900">
                Velocify
              </span>
            </div>
            <p className="text-sm text-neutral-600">
              AI-augmented task management platform for modern teams.
            </p>
          </div>

          {/* Quick links */}
          <div>
            <h3 className="text-sm font-semibold text-neutral-900 mb-3">
              Quick Links
            </h3>
            <ul className="space-y-2">
              <li>
                <Link
                  to="/dashboard"
                  className="text-sm text-neutral-600 hover:text-primary-600 transition-colors"
                >
                  Dashboard
                </Link>
              </li>
              <li>
                <Link
                  to="/tasks"
                  className="text-sm text-neutral-600 hover:text-primary-600 transition-colors"
                >
                  Tasks
                </Link>
              </li>
              <li>
                <Link
                  to="/profile"
                  className="text-sm text-neutral-600 hover:text-primary-600 transition-colors"
                >
                  Profile
                </Link>
              </li>
            </ul>
          </div>

          {/* Support section */}
          <div>
            <h3 className="text-sm font-semibold text-neutral-900 mb-3">
              Support
            </h3>
            <ul className="space-y-2">
              <li>
                <a
                  href="/docs"
                  className="text-sm text-neutral-600 hover:text-primary-600 transition-colors"
                >
                  Documentation
                </a>
              </li>
              <li>
                <a
                  href="/help"
                  className="text-sm text-neutral-600 hover:text-primary-600 transition-colors"
                >
                  Help Center
                </a>
              </li>
              <li>
                <a
                  href="/contact"
                  className="text-sm text-neutral-600 hover:text-primary-600 transition-colors"
                >
                  Contact Us
                </a>
              </li>
            </ul>
          </div>
        </div>

        {/* Bottom section */}
        <div className="mt-8 pt-6 border-t border-neutral-200">
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4">
            <p className="text-sm text-neutral-600">
              © {currentYear} Velocify. All rights reserved.
            </p>
            <div className="flex items-center gap-6">
              <a
                href="/privacy"
                className="text-sm text-neutral-600 hover:text-primary-600 transition-colors"
              >
                Privacy Policy
              </a>
              <a
                href="/terms"
                className="text-sm text-neutral-600 hover:text-primary-600 transition-colors"
              >
                Terms of Service
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
};
