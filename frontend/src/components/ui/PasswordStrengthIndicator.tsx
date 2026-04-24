/**
 * Password Strength Indicator Component
 * 
 * Displays a visual indicator of password strength based on various criteria:
 * - Length (minimum 8 characters)
 * - Contains uppercase letters
 * - Contains lowercase letters
 * - Contains numbers
 * - Contains special characters
 * 
 * Strength levels:
 * - Weak: 0-2 criteria met
 * - Fair: 3 criteria met
 * - Good: 4 criteria met
 * - Strong: 5 criteria met
 */

import { useMemo } from 'react';

interface PasswordStrengthIndicatorProps {
  password: string;
}

export interface PasswordStrength {
  score: number; // 0-5
  label: 'Weak' | 'Fair' | 'Good' | 'Strong' | '';
  color: string;
  percentage: number;
}

/**
 * Calculate password strength based on various criteria
 */
export function calculatePasswordStrength(password: string): PasswordStrength {
  if (!password) {
    return { score: 0, label: '', color: 'bg-neutral-200', percentage: 0 };
  }

  let score = 0;

  // Criteria 1: Length >= 8
  if (password.length >= 8) score++;

  // Criteria 2: Length >= 12 (bonus)
  if (password.length >= 12) score++;

  // Criteria 3: Contains uppercase
  if (/[A-Z]/.test(password)) score++;

  // Criteria 4: Contains lowercase
  if (/[a-z]/.test(password)) score++;

  // Criteria 5: Contains numbers
  if (/[0-9]/.test(password)) score++;

  // Criteria 6: Contains special characters
  if (/[^A-Za-z0-9]/.test(password)) score++;

  // Determine label and color based on score
  let label: PasswordStrength['label'] = 'Weak';
  let color = 'bg-danger-500';
  let percentage = 25;

  if (score >= 5) {
    label = 'Strong';
    color = 'bg-success-500';
    percentage = 100;
  } else if (score >= 4) {
    label = 'Good';
    color = 'bg-success-400';
    percentage = 75;
  } else if (score >= 3) {
    label = 'Fair';
    color = 'bg-warning-500';
    percentage = 50;
  }

  return { score, label, color, percentage };
}

export function PasswordStrengthIndicator({ password }: PasswordStrengthIndicatorProps) {
  const strength = useMemo(() => calculatePasswordStrength(password), [password]);

  if (!password) {
    return null;
  }

  return (
    <div className="mt-2">
      {/* Progress bar */}
      <div className="h-2 w-full bg-neutral-200 rounded-full overflow-hidden">
        <div
          className={`h-full transition-all duration-300 ${strength.color}`}
          style={{ width: `${strength.percentage}%` }}
          role="progressbar"
          aria-valuenow={strength.percentage}
          aria-valuemin={0}
          aria-valuemax={100}
          aria-label={`Password strength: ${strength.label}`}
        />
      </div>

      {/* Strength label */}
      <div className="mt-1 flex items-center justify-between">
        <span className="text-xs text-neutral-600">
          Password strength: <span className="font-medium">{strength.label}</span>
        </span>
        
        {/* Criteria hints */}
        <span className="text-xs text-neutral-500">
          {strength.score < 5 && 'Use 8+ chars, uppercase, lowercase, numbers, symbols'}
        </span>
      </div>
    </div>
  );
}
