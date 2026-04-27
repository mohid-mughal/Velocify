import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { Button } from '../components/ui/Button';
import { FormField } from '../components/forms/FormField';
import { useLogin } from '../hooks/useAuth';

/**
 * Login Page
 * 
 * Requirements: 1.1-1.8
 * 
 * Features:
 * - Email/password form with React Hook Form
 * - Zod validation for form fields
 * - Link to register page
 * - On success, store tokens in auth store and redirect to dashboard
 * - Error handling with user-friendly messages
 */

// Zod validation schema for login form
const loginSchema = z.object({
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Please enter a valid email address'),
  password: z
    .string()
    .min(1, 'Password is required')
    .min(6, 'Password must be at least 6 characters'),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const navigate = useNavigate();
  const loginMutation = useLogin();

  // React Hook Form setup with Zod validation
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: 'onSubmit', // Only validate on submit, not on blur
    defaultValues: {
      email: '',
      password: '',
    },
  });

  // Handle form submission
  const onSubmit = async (data: LoginFormData) => {
    try {
      await loginMutation.mutateAsync({
        email: data.email,
        password: data.password,
      });
      // On success, redirect to dashboard
      // The useLogin hook already updates the auth store
      navigate('/dashboard');
    } catch (error) {
      // Error is handled by the mutation and displayed below
      console.error('Login failed:', error);
    }
  };

  // Redirect if already authenticated
  useEffect(() => {
    // This could be enhanced to check auth state and redirect
    // For now, we rely on the PrivateRoute component
  }, []);

  return (
    <div className="min-h-screen flex items-center justify-center bg-neutral-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        {/* Header */}
        <div>
          <h1 className="text-center text-3xl font-bold text-neutral-900">
            Velocify
          </h1>
          <h2 className="mt-6 text-center text-2xl font-semibold text-neutral-700">
            Sign in to your account
          </h2>
          <p className="mt-2 text-center text-sm text-neutral-600">
            Or{' '}
            <Link
              to="/register"
              className="font-medium text-primary-600 hover:text-primary-500 transition-colors"
            >
              create a new account
            </Link>
          </p>
        </div>

        {/* Login Form */}
        <form className="mt-8 space-y-6" onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-4">
            {/* Email Field */}
            <FormField
              name="email"
              label="Email address"
              type="email"
              autoComplete="email"
              placeholder="you@example.com"
              error={errors.email?.message}
              {...register('email')}
            />

            {/* Password Field */}
            <FormField
              name="password"
              label="Password"
              type="password"
              autoComplete="current-password"
              placeholder="Enter your password"
              error={errors.password?.message}
              {...register('password')}
            />
          </div>

          {/* Error Message */}
          {loginMutation.isError && (
            <div
              className="rounded-md bg-danger-50 p-4"
              role="alert"
              aria-live="polite"
            >
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg
                    className="h-5 w-5 text-danger-400"
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                    aria-hidden="true"
                  >
                    <path
                      fillRule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                      clipRule="evenodd"
                    />
                  </svg>
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-danger-800">
                    Login failed
                  </h3>
                  <div className="mt-2 text-sm text-danger-700">
                    <p>
                      {loginMutation.error instanceof Error
                        ? loginMutation.error.message
                        : 'Invalid email or password. Please try again.'}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Submit Button */}
          <div>
            <Button
              type="submit"
              variant="primary"
              fullWidth
              isLoading={isSubmitting || loginMutation.isPending}
              disabled={isSubmitting || loginMutation.isPending}
            >
              {isSubmitting || loginMutation.isPending
                ? 'Signing in...'
                : 'Sign in'}
            </Button>
          </div>
        </form>

        {/* Additional Links */}
        <div className="text-center space-y-2">
          <p className="text-sm text-neutral-600">
            Don't have an account?{' '}
            <Link
              to="/register"
              className="font-medium text-primary-600 hover:text-primary-500 transition-colors"
            >
              Sign up now
            </Link>
          </p>
          <p className="text-xs text-neutral-500">
            <Link
              to="/test-connection"
              className="font-medium text-neutral-600 hover:text-neutral-700 transition-colors"
            >
              Test backend connection
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
