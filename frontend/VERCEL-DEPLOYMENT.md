# Vercel Deployment Guide for Velocify Frontend

## Overview

This document provides instructions for deploying the Velocify frontend to Vercel with proper configuration.

## Configuration Files

The following configuration files are already set up for Vercel deployment:

### 1. `vercel.json`
- **Framework**: Vite
- **Build Command**: `npm run build`
- **Output Directory**: `dist`
- **SPA Routing**: All routes rewrite to `/index.html` for client-side routing
- **Asset Caching**: Static assets in `/assets/` are cached for 1 year with immutable flag

### 2. `vite.config.ts`
- Path aliases configured for clean imports
- Production build optimizations enabled
- Code splitting configured for optimal caching:
  - `react-vendor`: React core libraries
  - `query-vendor`: TanStack Query
  - `form-vendor`: React Hook Form and Zod
  - `chart-vendor`: Recharts
  - `signalr-vendor`: SignalR client

### 3. `tailwind.config.js`
- Custom Velocify brand colors
- Extended typography and spacing
- Custom animations and transitions

### 4. `sonar-project.properties`
- SonarQube configuration for code quality
- Exclusions: `node_modules/`, `dist/`, `coverage/`, config files

## Required Environment Variables

You **MUST** configure the following environment variables in your Vercel project settings:

### Production Environment Variables

1. **VITE_API_BASE_URL**
   - Description: Backend API base URL
   - Example: `https://velocify-api.azurewebsites.net`
   - Required: Yes
   - Used for: All API requests to the backend

2. **VITE_SIGNALR_HUB_URL**
   - Description: SignalR Hub URL for real-time notifications
   - Example: `https://velocify-api.azurewebsites.net/hubs/task`
   - Required: Yes
   - Used for: Real-time task updates, notifications, and collaboration features

3. **VITE_DEBUG** (Optional)
   - Description: Enable debug mode for additional logging
   - Example: `false`
   - Required: No
   - Default: `false`

## Deployment Steps

### 1. Connect Repository to Vercel

1. Go to [Vercel Dashboard](https://vercel.com/dashboard)
2. Click "Add New Project"
3. Import your Git repository
4. Select the `frontend` directory as the root directory

### 2. Configure Build Settings

Vercel should auto-detect the settings from `vercel.json`, but verify:

- **Framework Preset**: Vite
- **Build Command**: `npm run build`
- **Output Directory**: `dist`
- **Install Command**: `npm install`

### 3. Set Environment Variables

In the Vercel project settings:

1. Navigate to **Settings** → **Environment Variables**
2. Add the following variables for **Production**:

```
VITE_API_BASE_URL=https://velocify-api.azurewebsites.net
VITE_SIGNALR_HUB_URL=https://velocify-api.azurewebsites.net/hubs/task
VITE_DEBUG=false
```

3. (Optional) Add the same variables for **Preview** and **Development** environments with appropriate values

### 4. Deploy

1. Click "Deploy"
2. Vercel will build and deploy your application
3. Once deployed, you'll receive a production URL (e.g., `https://velocify.vercel.app`)

## Post-Deployment Verification

After deployment, verify the following:

1. **SPA Routing**: Navigate to different routes and refresh the page - should not get 404 errors
2. **API Connection**: Check browser console for successful API requests
3. **SignalR Connection**: Verify real-time notifications are working
4. **Asset Loading**: Verify all static assets (images, fonts, etc.) load correctly
5. **Performance**: Check Lighthouse scores in Chrome DevTools

## CORS Configuration

Ensure your backend API (Azure App Service) has CORS configured to allow requests from your Vercel domain:

```csharp
// In backend Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://velocify.vercel.app",  // Your Vercel production URL
            "https://*.vercel.app"           // Preview deployments
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

## Troubleshooting

### Issue: API requests fail with CORS errors
**Solution**: Verify CORS is configured in the backend to allow your Vercel domain

### Issue: SignalR connection fails
**Solution**: 
- Verify `VITE_SIGNALR_HUB_URL` is set correctly
- Check that the backend SignalR hub is accessible
- Ensure CORS allows WebSocket connections

### Issue: Environment variables not working
**Solution**: 
- Verify variables are prefixed with `VITE_`
- Redeploy after adding/changing environment variables
- Check browser console for the actual values being used

### Issue: 404 on page refresh
**Solution**: Verify `vercel.json` has the rewrite rule for SPA routing

## Local Development

For local development, create a `.env.local` file (not committed to Git):

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/task
VITE_DEBUG=true
```

## Additional Resources

- [Vercel Documentation](https://vercel.com/docs)
- [Vite Environment Variables](https://vitejs.dev/guide/env-and-mode.html)
- [Vercel CLI](https://vercel.com/docs/cli)

## Support

For deployment issues, contact the DevOps team or refer to the main project README.
