import { BackendConnectionTest } from '../components/BackendConnectionTest';

/**
 * Connection Test Page
 * 
 * A dedicated page for testing the frontend-backend connection.
 * Useful for debugging deployment and configuration issues.
 */

export default function ConnectionTestPage() {
  return (
    <div className="min-h-screen bg-neutral-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-3xl mx-auto">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-neutral-900">
            Velocify Connection Test
          </h1>
          <p className="mt-2 text-neutral-600">
            Test the connection between frontend and backend
          </p>
        </div>

        <BackendConnectionTest />

        <div className="mt-8 text-center">
          <a
            href="/login"
            className="text-primary-600 hover:text-primary-500 font-medium"
          >
            ← Back to Login
          </a>
        </div>
      </div>
    </div>
  );
}
