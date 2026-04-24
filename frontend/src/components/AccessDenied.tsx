import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

/**
 * AccessDenied Component
 * 
 * Requirements: 20.2, 20.5
 * 
 * Displays when a user attempts to access a route they don't have
 * permission for based on their role.
 * 
 * Features:
 * - Shows clear message about insufficient permissions
 * - Displays user's current role
 * - Provides navigation back to dashboard
 * - Provides logout option
 */

export default function AccessDenied() {
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();

  const handleGoToDashboard = () => {
    navigate('/dashboard');
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div style={styles.container}>
      <div style={styles.content}>
        <div style={styles.iconContainer}>
          <svg
            style={styles.icon}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
            />
          </svg>
        </div>

        <h1 style={styles.title}>Access Denied</h1>
        
        <p style={styles.message}>
          You don't have permission to access this page.
        </p>

        {user && (
          <p style={styles.roleInfo}>
            Your current role: <strong>{user.role}</strong>
          </p>
        )}

        <div style={styles.buttonContainer}>
          <button
            onClick={handleGoToDashboard}
            style={styles.primaryButton}
          >
            Go to Dashboard
          </button>
          
          <button
            onClick={handleLogout}
            style={styles.secondaryButton}
          >
            Logout
          </button>
        </div>
      </div>
    </div>
  );
}

// Inline styles for simplicity - can be replaced with Tailwind classes
const styles: { [key: string]: React.CSSProperties } = {
  container: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: '100vh',
    backgroundColor: '#f9fafb',
    padding: '1rem',
  },
  content: {
    textAlign: 'center',
    maxWidth: '28rem',
    width: '100%',
  },
  iconContainer: {
    display: 'flex',
    justifyContent: 'center',
    marginBottom: '1.5rem',
  },
  icon: {
    width: '4rem',
    height: '4rem',
    color: '#ef4444',
  },
  title: {
    fontSize: '2rem',
    fontWeight: 'bold',
    color: '#111827',
    marginBottom: '1rem',
  },
  message: {
    fontSize: '1.125rem',
    color: '#6b7280',
    marginBottom: '0.5rem',
  },
  roleInfo: {
    fontSize: '0.875rem',
    color: '#9ca3af',
    marginBottom: '2rem',
  },
  buttonContainer: {
    display: 'flex',
    flexDirection: 'column',
    gap: '0.75rem',
  },
  primaryButton: {
    width: '100%',
    padding: '0.75rem 1.5rem',
    backgroundColor: '#3b82f6',
    color: 'white',
    border: 'none',
    borderRadius: '0.5rem',
    fontSize: '1rem',
    fontWeight: '500',
    cursor: 'pointer',
    transition: 'background-color 0.2s',
  },
  secondaryButton: {
    width: '100%',
    padding: '0.75rem 1.5rem',
    backgroundColor: 'white',
    color: '#374151',
    border: '1px solid #d1d5db',
    borderRadius: '0.5rem',
    fontSize: '1rem',
    fontWeight: '500',
    cursor: 'pointer',
    transition: 'background-color 0.2s',
  },
};
