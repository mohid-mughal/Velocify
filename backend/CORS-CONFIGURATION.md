# CORS Configuration

## Overview

The Velocify API implements Cross-Origin Resource Sharing (CORS) to allow the frontend application to communicate with the backend API from different origins. This is essential for the frontend (deployed on Vercel) to make requests to the backend (deployed on Azure App Service).

## Configuration

CORS is configured in `Program.cs` and reads allowed origins from the `CorsSettings:AllowedOrigins` configuration setting.

### Environment Variables

Set the `CorsSettings__AllowedOrigins` environment variable with a semicolon or comma-separated list of allowed origins:

**Development:**
```
CorsSettings__AllowedOrigins=http://localhost:3000;http://localhost:5173
```

**Production:**
```
CorsSettings__AllowedOrigins=https://your-frontend.vercel.app
```

### Configuration Files

The CORS settings are defined in the appsettings files:

- `appsettings.json`: Default development origins (localhost:3000, localhost:5173)
- `appsettings.Development.json`: Development-specific origins
- `appsettings.Production.json`: Production origins (must be set via environment variable)

### CORS Policy

The CORS policy is named `AllowFrontend` and includes:

- **WithOrigins**: Specific origins from configuration (never wildcard)
- **AllowCredentials**: Required for JWT authentication and SignalR
- **AllowAnyHeader**: Permits all headers (Authorization, Content-Type, etc.)
- **AllowAnyMethod**: Permits all HTTP methods (GET, POST, PUT, DELETE, PATCH)

## Security Considerations

1. **Never use AllowAnyOrigin in production** - Always specify exact origins
2. **AllowCredentials requires specific origins** - Cannot be used with wildcard (*)
3. **Use HTTPS in production** - All production origins should use HTTPS protocol
4. **Validate origins carefully** - Only add trusted frontend domains

## Testing CORS

### Manual Testing

Use the `test-cors.http` file to manually test CORS:

1. Start the API: `dotnet run --project backend/Velocify.API`
2. Open `test-cors.http` in VS Code with REST Client extension
3. Execute the test requests to verify CORS headers

### Expected Response Headers

A successful CORS request should include:

```
Access-Control-Allow-Origin: http://localhost:3000
Access-Control-Allow-Credentials: true
```

A preflight (OPTIONS) request should include:

```
Access-Control-Allow-Origin: http://localhost:3000
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH
Access-Control-Allow-Headers: Content-Type, Authorization
Access-Control-Allow-Credentials: true
```

## Troubleshooting

### CORS not working

1. **Check configuration**: Verify `CorsSettings:AllowedOrigins` is set correctly
2. **Check logs**: Look for "CORS configured with allowed origins" message on startup
3. **Check origin**: Ensure the Origin header matches exactly (including protocol and port)
4. **Check middleware order**: CORS must be applied before Authentication/Authorization

### Common Issues

- **Origin mismatch**: The Origin header must match exactly (case-sensitive)
- **Missing protocol**: Include http:// or https:// in the origin
- **Port mismatch**: localhost:3000 is different from localhost:5173
- **Trailing slash**: Don't include trailing slashes in origins

## Requirements

This implementation satisfies **Requirement 29.5**:
- Reads allowed origins from environment variable
- Configures CORS policy with credentials support
- Supports multiple origins separated by semicolon or comma
- Applies CORS policy to the application pipeline
