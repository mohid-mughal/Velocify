import { useState } from 'react';
import { Button } from './ui/Button';
import axios from 'axios';
import axiosInstance from '../api/axios';

/**
 * Backend Connection Test Component
 * 
 * A simple component to test if the frontend can connect to the backend API.
 * Shows the configured API URL and allows testing the connection.
 */

interface ConnectionStatus {
  status: 'idle' | 'testing' | 'success' | 'error';
  message: string;
  details?: any;
}

export function BackendConnectionTest() {
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>({
    status: 'idle',
    message: 'Click the button to test the connection',
  });

  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/api/v1';

  const testConnection = async () => {
    setConnectionStatus({
      status: 'testing',
      message: 'Testing connection...',
    });

    try {
      // 1. Get the raw base URL, stripping any trailing slash
      const rawBaseUrl = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') || '';
      
      // 2. Ensure we hit /health directly, bypassing the /api/v1 axiosInstance
      const targetUrl = rawBaseUrl ? `${rawBaseUrl}/health` : '/health';

      // 3. Use raw axios instead of axiosInstance
      const response = await axios.get(targetUrl);
      
      setConnectionStatus({
        status: 'success',
        message: 'Successfully connected to backend!',
        details: response.data,
      });
    } catch (error: any) {
      setConnectionStatus({
        status: 'error',
        message: 'Failed to connect to backend',
        details: {
          error: error.message,
          code: error.code,
          response: error.response?.data,
          status: error.response?.status,
        },
      });
    }
  };

  const getStatusColor = () => {
    switch (connectionStatus.status) {
      case 'success':
        return 'text-success-600 bg-success-50 border-success-200';
      case 'error':
        return 'text-danger-600 bg-danger-50 border-danger-200';
      case 'testing':
        return 'text-primary-600 bg-primary-50 border-primary-200';
      default:
        return 'text-neutral-600 bg-neutral-50 border-neutral-200';
    }
  };

  const getStatusIcon = () => {
    switch (connectionStatus.status) {
      case 'success':
        return (
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
          </svg>
        );
      case 'error':
        return (
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
          </svg>
        );
      case 'testing':
        return (
          <svg className="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
          </svg>
        );
      default:
        return (
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
          </svg>
        );
    }
  };

  return (
    <div className="p-6 bg-white rounded-lg shadow-sm border border-neutral-200">
      <h3 className="text-lg font-semibold text-neutral-900 mb-4">
        Backend Connection Test
      </h3>

      <div className="space-y-4">
        {/* API URL Info */}
        <div className="p-3 bg-neutral-50 rounded border border-neutral-200">
          <p className="text-sm font-medium text-neutral-700 mb-1">
            Configured API URL:
          </p>
          <p className="text-sm text-neutral-600 font-mono break-all">
            {apiBaseUrl}
          </p>
        </div>

        {/* Test Button */}
        <Button
          onClick={testConnection}
          variant="primary"
          disabled={connectionStatus.status === 'testing'}
          isLoading={connectionStatus.status === 'testing'}
        >
          {connectionStatus.status === 'testing' ? 'Testing...' : 'Test Connection'}
        </Button>

        {/* Status Display */}
        {connectionStatus.status !== 'idle' && (
          <div className={`p-4 rounded border ${getStatusColor()}`}>
            <div className="flex items-start">
              <div className="flex-shrink-0">
                {getStatusIcon()}
              </div>
              <div className="ml-3 flex-1">
                <p className="text-sm font-medium">
                  {connectionStatus.message}
                </p>
                {connectionStatus.details && (
                  <div className="mt-2">
                    <details className="text-xs">
                      <summary className="cursor-pointer font-medium">
                        View Details
                      </summary>
                      <pre className="mt-2 p-2 bg-white bg-opacity-50 rounded overflow-auto max-h-48">
                        {JSON.stringify(connectionStatus.details, null, 2)}
                      </pre>
                    </details>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Help Text */}
        <div className="text-xs text-neutral-500 space-y-1">
          <p>
            <strong>For local development:</strong> Make sure your backend is running on the configured URL.
          </p>
          <p>
            <strong>For production:</strong> Ensure VITE_API_BASE_URL is set correctly in Vercel environment variables.
          </p>
          <p className="mt-2">
            <strong>Common issues:</strong>
          </p>
          <ul className="list-disc list-inside ml-2 space-y-1">
            <li>Backend not running</li>
            <li>CORS not configured properly</li>
            <li>Wrong API URL in environment variables</li>
            <li>Network/firewall blocking the connection</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
