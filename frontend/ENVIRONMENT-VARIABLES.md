# Environment Variables for Velocify Frontend

## Overview

This document lists all required and optional environment variables for the Velocify frontend application.

## Important Notes

⚠️ **All environment variables for Vite must be prefixed with `VITE_`** to be exposed to the client-side code.

⚠️ **Never commit `.env.local` or `.env.production` files to Git** - they may contain sensitive information.

## Required Environment Variables

### 1. VITE_API_BASE_URL

**Description**: The base URL of the Velocify backend API

**Required**: Yes

**Type**: String (URL)

**Examples**:
- Local development: `http://localhost:5000`
- Production: `https://velocify-api.azurewebsites.net`
- Staging: `https://velocify-api-staging.azurewebsites.net`

**Usage**: Used by the API client to make HTTP requests to the backend

**Where to set**:
- Local: `.env.local` file
- Vercel: Project Settings → Environment Variables

---

### 2. VITE_SIGNALR_HUB_URL

**Description**: The URL of the SignalR hub for real-time notifications

**Required**: Yes

**Type**: String (URL)

**Examples**:
- Local development: `http://localhost:5000/hubs/task`
- Production: `https://velocify-api.azurewebsites.net/hubs/task`
- Staging: `https://velocify-api-staging.azurewebsites.net/hubs/task`

**Usage**: Used by the SignalR client to establish WebSocket connections for real-time features:
- Task assignment notifications
- Status change notifications
- Comment notifications
- AI suggestion notifications

**Where to set**:
- Local: `.env.local` file
- Vercel: Project Settings → Environment Variables

---

## Optional Environment Variables

### 3. VITE_DEBUG

**Description**: Enable debug mode for additional console logging

**Required**: No

**Type**: Boolean (string)

**Default**: `false`

**Examples**:
- Development: `true`
- Production: `false`

**Usage**: When set to `true`, enables verbose logging in the browser console for debugging purposes

**Where to set**:
- Local: `.env.local` file
- Vercel: Project Settings → Environment Variables (optional)

---

## Environment-Specific Configuration

### Local Development

Create a `.env.local` file in the `frontend/` directory:

```env
# Backend API
VITE_API_BASE_URL=http://localhost:5000

# SignalR Hub
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/task

# Debug mode
VITE_DEBUG=true
```

### Vercel Production

Set in Vercel Dashboard → Project Settings → Environment Variables:

| Variable Name | Value | Environment |
|--------------|-------|-------------|
| `VITE_API_BASE_URL` | `https://velocify-api.azurewebsites.net` | Production |
| `VITE_SIGNALR_HUB_URL` | `https://velocify-api.azurewebsites.net/hubs/task` | Production |
| `VITE_DEBUG` | `false` | Production |

### Vercel Preview (Optional)

Set in Vercel Dashboard → Project Settings → Environment Variables:

| Variable Name | Value | Environment |
|--------------|-------|-------------|
| `VITE_API_BASE_URL` | `https://velocify-api-staging.azurewebsites.net` | Preview |
| `VITE_SIGNALR_HUB_URL` | `https://velocify-api-staging.azurewebsites.net/hubs/task` | Preview |
| `VITE_DEBUG` | `true` | Preview |

---

## How to Access Environment Variables in Code

### In TypeScript/JavaScript

```typescript
// API base URL
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

// SignalR hub URL
const signalRHubUrl = import.meta.env.VITE_SIGNALR_HUB_URL;

// Debug mode
const isDebugMode = import.meta.env.VITE_DEBUG === 'true';
```

### Type Safety

Add type definitions in `src/vite-env.d.ts`:

```typescript
/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_SIGNALR_HUB_URL: string;
  readonly VITE_DEBUG?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
```

---

## Validation

### Runtime Validation

The application should validate required environment variables at startup:

```typescript
// src/config/env.ts
export function validateEnv() {
  const required = ['VITE_API_BASE_URL', 'VITE_SIGNALR_HUB_URL'];
  
  const missing = required.filter(key => !import.meta.env[key]);
  
  if (missing.length > 0) {
    throw new Error(
      `Missing required environment variables: ${missing.join(', ')}\n` +
      'Please check your .env.local file or Vercel environment variables.'
    );
  }
}
```

### Build-Time Validation

Vite will replace `import.meta.env.VITE_*` with the actual values at build time. If a variable is not set, it will be `undefined`.

---

## Troubleshooting

### Issue: Environment variables are undefined

**Possible causes**:
1. Variable name doesn't start with `VITE_`
2. `.env.local` file is not in the `frontend/` directory
3. Vercel environment variables are not set
4. Need to restart dev server after changing `.env.local`

**Solution**:
- Ensure all variables start with `VITE_`
- Restart the dev server: `npm run dev`
- Check Vercel environment variables in dashboard
- Redeploy after changing Vercel environment variables

### Issue: API requests fail with CORS errors

**Possible causes**:
1. `VITE_API_BASE_URL` is incorrect
2. Backend CORS is not configured to allow frontend domain

**Solution**:
- Verify `VITE_API_BASE_URL` matches your backend URL
- Configure CORS in backend to allow your Vercel domain

### Issue: SignalR connection fails

**Possible causes**:
1. `VITE_SIGNALR_HUB_URL` is incorrect
2. Backend SignalR hub is not running
3. CORS doesn't allow WebSocket connections

**Solution**:
- Verify `VITE_SIGNALR_HUB_URL` includes `/hubs/task` path
- Check backend logs for SignalR errors
- Ensure CORS allows credentials and WebSocket upgrade

---

## Security Considerations

⚠️ **Important Security Notes**:

1. **Never store secrets in environment variables** - Vite environment variables are embedded in the client-side bundle and are publicly accessible
2. **Use environment variables only for configuration** - API URLs, feature flags, etc.
3. **Never store API keys, passwords, or tokens** in `VITE_*` variables
4. **Authentication tokens should be stored in memory or httpOnly cookies** - never in environment variables

---

## Reference Files

- `.env.example` - Template for local development
- `vercel.json` - Vercel deployment configuration
- `vite.config.ts` - Vite build configuration

---

## Support

For issues with environment variables:
1. Check this documentation
2. Verify `.env.local` file exists and has correct values
3. Check Vercel dashboard for environment variables
4. Contact DevOps team for production environment issues
