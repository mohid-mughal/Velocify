/**
 * Password Strength Indicator Example
 * 
 * This component demonstrates the PasswordStrengthIndicator in action
 * with various password examples showing different strength levels.
 * 
 * Usage: Import and render this component to see the password strength indicator demo
 */

import { useState } from 'react';
import { PasswordStrengthIndicator } from './PasswordStrengthIndicator';
import { Input } from './Input';

export function PasswordStrengthExample() {
  const [password, setPassword] = useState('');

  const examples = [
    { label: 'Weak', value: 'abc', description: 'Too short, no variety' },
    { label: 'Weak', value: 'abcdefgh', description: 'Only lowercase' },
    { label: 'Fair', value: 'Abcdef123', description: 'Mixed case + numbers' },
    { label: 'Good', value: 'Abcdef123456', description: 'Longer + mixed case + numbers' },
    { label: 'Strong', value: 'Abcdef123456!@#', description: 'All criteria met' },
  ];

  return (
    <div className="max-w-2xl mx-auto p-8 space-y-8">
      <div>
        <h2 className="text-2xl font-bold text-neutral-900 mb-2">
          Password Strength Indicator Demo
        </h2>
        <p className="text-neutral-600">
          Type a password to see the strength indicator in action
        </p>
      </div>

      {/* Interactive Demo */}
      <div className="bg-white p-6 rounded-lg border border-neutral-200 shadow-sm">
        <h3 className="text-lg font-semibold text-neutral-900 mb-4">
          Try it yourself
        </h3>
        <div>
          <label htmlFor="demo-password" className="block text-sm font-medium text-neutral-700 mb-2">
            Password
          </label>
          <Input
            id="demo-password"
            type="text"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Type a password..."
          />
          <PasswordStrengthIndicator password={password} />
        </div>
      </div>

      {/* Examples */}
      <div className="bg-white p-6 rounded-lg border border-neutral-200 shadow-sm">
        <h3 className="text-lg font-semibold text-neutral-900 mb-4">
          Example Passwords
        </h3>
        <div className="space-y-4">
          {examples.map((example, index) => (
            <div key={index} className="border-b border-neutral-100 pb-4 last:border-0">
              <div className="flex items-center justify-between mb-2">
                <div>
                  <span className="font-mono text-sm text-neutral-900">{example.value}</span>
                  <span className="ml-3 text-sm text-neutral-600">{example.description}</span>
                </div>
                <button
                  onClick={() => setPassword(example.value)}
                  className="text-sm text-primary-600 hover:text-primary-700 font-medium"
                >
                  Try it
                </button>
              </div>
              <PasswordStrengthIndicator password={example.value} />
            </div>
          ))}
        </div>
      </div>

      {/* Criteria Explanation */}
      <div className="bg-neutral-50 p-6 rounded-lg border border-neutral-200">
        <h3 className="text-lg font-semibold text-neutral-900 mb-4">
          Strength Criteria
        </h3>
        <ul className="space-y-2 text-sm text-neutral-700">
          <li className="flex items-start">
            <span className="font-medium mr-2">✓</span>
            <span>Length ≥ 8 characters</span>
          </li>
          <li className="flex items-start">
            <span className="font-medium mr-2">✓</span>
            <span>Length ≥ 12 characters (bonus point)</span>
          </li>
          <li className="flex items-start">
            <span className="font-medium mr-2">✓</span>
            <span>Contains uppercase letters (A-Z)</span>
          </li>
          <li className="flex items-start">
            <span className="font-medium mr-2">✓</span>
            <span>Contains lowercase letters (a-z)</span>
          </li>
          <li className="flex items-start">
            <span className="font-medium mr-2">✓</span>
            <span>Contains numbers (0-9)</span>
          </li>
          <li className="flex items-start">
            <span className="font-medium mr-2">✓</span>
            <span>Contains special characters (!@#$%^&*)</span>
          </li>
        </ul>
        <div className="mt-4 pt-4 border-t border-neutral-200">
          <p className="text-sm text-neutral-600">
            <strong>Scoring:</strong> Weak (0-2), Fair (3), Good (4), Strong (5-6)
          </p>
        </div>
      </div>
    </div>
  );
}

export default PasswordStrengthExample;
