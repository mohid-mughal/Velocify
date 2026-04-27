import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { Button } from '../components/ui/Button';
import { FormField } from '../components/forms/FormField';
import { PasswordStrengthIndicator } from '../components/ui/PasswordStrengthIndicator';
import { useRegister } from '../hooks/useAuth';

/**
 * Register Page
 * 
 * Requirements: 1.1, 1.2
 * 
 * Features:
 * - Full name (first name, last name), email, password, confirm password fields
 * - Password strength indicator
 * - Form validation with Zod
 * - On success, redirect to login page
 * - Error handling with user-friendly messages
 */

// Zod validation schema for registration form
const registerSchema = z
  .object({
    firstName: z
      .string()
      .min(1, 'First name is required')
      .max(50, 'First name must be less than 50 characters'),
    lastName: z
      .string()
      .min(1, 'Last name is required')
      .max(50, 'Last name must be less than 50 characters'),
    email: z
      .string()
      .min(1, 'Email is required')
      .email('Please enter a valid email address'),
    password: z
      .string()
      .min(1, 'Password is required')
      .min(8, 'Password must be at least 8 characters')
      .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
      .regex(/[0-9]/, 'Password must contain at least one number'),
    confirmPassword: z.string().min(1, 'Please confirm your password'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const navigate = useNavigate();
  const registerMutation = useRegister();

  // React Hook Form setup with Zod validation
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    mode: 'onSubmit', // Only validate on submit, not on blur
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
  });

  // Watch password field for strength indicator
  const password = watch('password');

  // Handle form submission
  const onSubmit = async (data: RegisterFormData) => {
    try {
      await registerMutation.mutateAsync({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
      });
      // On success, redirect to login page
      // Note: The useRegister hook automatically logs the user in and updates the auth store
      // So we redirect to dashboard instead
      navigate('/dashboard');
    } catch (error) {
      // Error is handled by the mutation and displayed below
      console.error('Registration failed:', error);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-neutral-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        {/* Header */}
        <div>
          <h1 className="text-center text-3xl font-bold text-neutral-900">
            Velocify
          </h1>
          <h2 className="mt-6 text-center text-2xl font-semibold text-neutral-700">
            Create your account
          </h2>
          <p className="mt-2 text-center text-sm text-neutral-600">
            Already have an account?{' '}
            <Link
              to="/login"
              className="font-medium text-primary-600 hover:text-primary-500 transition-colors"
            >
              Sign in
            </Link>
          </p>
        </div>

        {/* Registration Form */}
        <form className="mt-8 space-y-6" onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-4">
            {/* First Name Field */}
            <FormField
              name="firstName"
              label="First name"
              type="text"
              autoComplete="given-name"
              placeholder="John"
              error={errors.firstName?.message}
              {...register('firstName')}
            />

            {/* Last Name Field */}
            <FormField
              name="lastName"
              label="Last name"
              type="text"
              autoComplete="family-name"
              placeholder="Doe"
              error={errors.lastName?.message}
              {...register('lastName')}
            />

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
            <div>
              <FormField
                name="password"
                label="Password"
                type="password"
                autoComplete="new-password"
                placeholder="Create a strong password"
                error={errors.password?.message}
                {...register('password')}
              />
              {/* Password Strength Indicator */}
              <PasswordStrengthIndicator password={password} />
            </div>

            {/* Confirm Password Field */}
            <FormField
              name="confirmPassword"
              label="Confirm password"
              type="password"
              autoComplete="new-password"
              placeholder="Re-enter your password"
              error={errors.confirmPassword?.message}
              {...register('confirmPassword')}
            />
          </div>

          {/* Error Message */}
          {registerMutation.isError && (
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
                    Registration failed
                  </h3>
                  <div className="mt-2 text-sm text-danger-700">
                    <p>
                      {registerMutation.error instanceof Error
                        ? registerMutation.error.message
                        : 'Unable to create account. Please try again.'}
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
              isLoading={isSubmitting || registerMutation.isPending}
              disabled={isSubmitting || registerMutation.isPending}
            >
              {isSubmitting || registerMutation.isPending
                ? 'Creating account...'
                : 'Create account'}
            </Button>
          </div>
        </form>


      </div>
    </div>
  );
}
