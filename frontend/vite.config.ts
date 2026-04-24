import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': '/src',
      '@/api': '/src/api',
      '@/components': '/src/components',
      '@/features': '/src/features',
      '@/hooks': '/src/hooks',
      '@/pages': '/src/pages',
      '@/store': '/src/store',
      '@/utils': '/src/utils',
    },
  },
  build: {
    // Production build optimizations
    target: 'esnext',
    minify: 'esbuild', // Use esbuild for faster builds
    rollupOptions: {
      output: {
        manualChunks: {
          // Split vendor chunks for better caching
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'query-vendor': ['@tanstack/react-query', '@tanstack/react-query-devtools'],
          'form-vendor': ['react-hook-form', '@hookform/resolvers', 'zod'],
          'chart-vendor': ['recharts'],
          'signalr-vendor': ['@microsoft/signalr'],
        },
      },
    },
    chunkSizeWarningLimit: 1000, // Increase chunk size warning limit
    sourcemap: false, // Disable sourcemaps in production for smaller bundle
  },
  server: {
    port: 3000,
    strictPort: false,
    host: true,
    open: true,
  },
  preview: {
    port: 3000,
    strictPort: false,
    host: true,
  },
})

