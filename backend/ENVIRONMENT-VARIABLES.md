# Environment Variables Configuration

This document describes all environment variables used by the Velocify API.

## Azure App Service Environment Variable Naming

**IMPORTANT**: Azure App Service uses double underscores (`__`) instead of colons (`:`) for nested configuration keys.

### Correct Azure Format:
```
ConnectionStrings__DefaultConnection
JwtSettings__SecretKey
JwtSettings__Issuer
JwtSettings__Audience
LangChain__ApiKey
LangChain__Model
CorsSettings__AllowedOrigins
```

### Local Development Format (appsettings.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "JwtSettings": {
    "SecretKey": "..."
  }
}
```

## Database Configuration

### `ConnectionStrings__DefaultConnection` (Azure) / `ConnectionStrings:DefaultConnection` (Local)
**Required**: Yes  
**Description**: Connection string for the SQL Server database  

**Local Development Format**: 
```
Server=localhost;Database=VelocifyDb;Integrated Security=true;TrustServerCertificate=true;
```

**Azure SQL Format**:
```
Server=tcp:YOUR-SERVER.database.windows.net,1433;Initial Catalog=YOUR-DATABASE;Persist Security Info=False;User ID=YOUR-USERNAME;Password=YOUR-PASSWORD;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Min Pool Size=2;Max Pool Size=100;
```

**Azure Configuration**: Set as Connection String (not Application Setting)
- Name: `DefaultConnection`
- Value: Your Azure SQL connection string
- Type: `SQLAzure`

**Notes:**
- Connection pooling parameters are CRITICAL for F1 tier:
  - `Min Pool Size=2` - Keeps 2 connections warm to reduce cold start latency
  - `Max Pool Size=100` - Limits connections to prevent memory exhaustion on F1 tier's 1GB RAM

## JWT Authentication Configuration

### `JwtSettings__SecretKey` (Azure) / `JwtSettings:SecretKey` (Local)
**Required**: Yes  
**Description**: Secret key used to sign JWT tokens  
**Format**: String (minimum 32 characters recommended)  
**Example**: `your-super-secret-jwt-key-min-32-chars-long`

### `JwtSettings__Issuer` (Azure) / `JwtSettings:Issuer` (Local)
**Required**: Yes  
**Description**: JWT token issuer (typically your API URL)  
**Example**: 
- Local: `https://localhost:5000`
- Azure: `https://velocify.azurewebsites.net`

### `JwtSettings__Audience` (Azure) / `JwtSettings:Audience` (Local)
**Required**: Yes  
**Description**: JWT token audience (typically your frontend URL)  
**Example**: 
- Local: `https://localhost:3000`
- Azure: `https://velocify-work.vercel.app`

### `JwtSettings__AccessTokenExpirationMinutes` (Azure) / `JwtSettings:AccessTokenExpirationMinutes` (Local)
**Required**: No  
**Default**: `15`  
**Description**: Access token expiration time in minutes

### `JwtSettings__RefreshTokenExpirationDays` (Azure) / `JwtSettings:RefreshTokenExpirationDays` (Local)
**Required**: No  
**Default**: `7`  
**Description**: Refresh token expiration time in days

## AI/LangChain Configuration

### `LangChain__ApiKey` (Azure) / `LangChain:ApiKey` (Local)
**Required**: Yes (for AI features)  
**Description**: API key for LangChain integration (supports OpenAI, Groq, and other providers)  

**For OpenAI**:
```
sk-proj-...
```

**For Groq**:
```
gsk_...
```

**Note**: Required for sentiment analysis and AI-powered import features

### `LangChain__Model` (Azure) / `LangChain:Model` (Local)
**Required**: No  
**Default**: `gpt-3.5-turbo`  
**Description**: Model to use for LangChain operations  

**For Groq**:
```
openai/gpt-oss-120b
```

**For OpenAI**:
```
gpt-3.5-turbo
gpt-4
gpt-4-turbo
```

## CORS Configuration

### `CorsSettings__AllowedOrigins` (Azure) / `CorsSettings:AllowedOrigins` (Local)
**Required**: Yes  
**Description**: Comma-separated list of allowed origins for CORS  
**Example**: `https://localhost:3000,https://localhost:5173,https://velocify-work.vercel.app`

## Logging Configuration

### `Logging__LogLevel__Default` (Azure) / `Logging:LogLevel:Default` (Local)
**Required**: No  
**Default**: `Information`  
**Description**: Default logging level  
**Options**: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`

### `Logging__LogLevel__Microsoft.AspNetCore` (Azure) / `Logging:LogLevel:Microsoft.AspNetCore` (Local)
**Required**: No  
**Default**: `Warning`  
**Description**: Logging level for ASP.NET Core framework logs

## Environment-Specific Settings

### `ASPNETCORE_ENVIRONMENT`
**Required**: No  
**Default**: `Production`  
**Description**: Determines which appsettings file to load  
**Options**: `Development`, `Staging`, `Production`, `Test`

## Azure-Specific Configuration

### Application Insights (Optional)

#### `APPLICATIONINSIGHTS_CONNECTION_STRING`
**Required**: No  
**Description**: Connection string for Azure Application Insights  
**Example**: `InstrumentationKey=...;IngestionEndpoint=...`

## Example Configuration Files

### Local Development (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VelocifyDb;Integrated Security=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-jwt-key-min-32-chars-long",
    "Issuer": "https://localhost:5000",
    "Audience": "https://localhost:3000",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "LangChain": {
    "ApiKey": "gsk_...",
    "Model": "openai/gpt-oss-120b"
  },
  "CorsSettings": {
    "AllowedOrigins": "https://localhost:3000,https://localhost:5173"
  }
}
```

### Azure App Service Configuration

In Azure Portal → App Service → Configuration:

**Connection Strings:**
- Name: `DefaultConnection`
- Value: `Server=tcp:YOUR-SERVER.database.windows.net,1433;Initial Catalog=YOUR-DATABASE;...`
- Type: `SQLAzure`

**Application Settings:**
```
ASPNETCORE_ENVIRONMENT = Production
JwtSettings__SecretKey = your-super-secret-jwt-key-min-32-chars-long
JwtSettings__Issuer = https://velocify.azurewebsites.net
JwtSettings__Audience = https://velocify-work.vercel.app
LangChain__ApiKey = gsk_YOUR_GROQ_API_KEY_HERE
LangChain__Model = openai/gpt-oss-120b
CorsSettings__AllowedOrigins = https://velocify-work.vercel.app,https://localhost:3000
```

**Note**: Use double underscores (`__`) for nested keys in Azure App Service Application Settings.

## Azure F1 Tier Limitations

The Azure App Service F1 (Free) tier has the following limitations:

1. **CPU Time:** 60 minutes per day
   - Monitor usage in Azure Portal → App Service → Metrics → CPU Time
   - AI features consume significant CPU time - monitor usage carefully
   - Set up alerts at 50 minutes to get 10-minute warning

2. **Memory:** 1 GB RAM
   - Connection pooling configured to optimize memory usage
   - Max Pool Size=100 prevents memory exhaustion

3. **Storage:** 1 GB disk space
   - Log files stored in `/home/LogFiles/Application/`
   - Monitor disk usage in Azure Portal

4. **Always On:** Not available
   - First request after idle period will be slow (cold start ~10-30 seconds)
   - Health checks configured to minimize cold starts

5. **Custom Domains:** Not available
   - Use default `*.azurewebsites.net` domain
   - Upgrade to B1 tier for custom domain support

## Configuration in Azure App Service

To set environment variables in Azure App Service:

1. Navigate to Azure Portal
2. Go to your App Service
3. Select "Configuration" under Settings
4. Click "New application setting"
5. Add each environment variable as a key-value pair (use `__` for nested keys)
6. Click "Save" and restart the app

**Alternative:** Use Azure CLI:
```bash
az webapp config appsettings set --name velocify --resource-group YOUR-RESOURCE-GROUP --settings \
  "JwtSettings__SecretKey=your-secret-key" \
  "JwtSettings__Issuer=https://velocify.azurewebsites.net" \
  "JwtSettings__Audience=https://velocify-work.vercel.app" \
  "LangChain__ApiKey=gsk_..." \
  "LangChain__Model=openai/gpt-oss-120b" \
  "CorsSettings__AllowedOrigins=https://velocify-work.vercel.app"
```

## Security Checklist

- [ ] All sensitive values stored as environment variables (not in appsettings.json)
- [ ] JwtSettings__SecretKey is at least 32 characters and cryptographically secure
- [ ] LangChain__ApiKey has usage limits configured in provider dashboard
- [ ] CorsSettings__AllowedOrigins includes only trusted frontend URLs
- [ ] Azure SQL firewall rules are properly configured
- [ ] Connection string uses strong password (12+ characters, mixed case, numbers, symbols)
- [ ] All environment variables are documented and shared securely with team
- [ ] Secrets are rotated periodically (quarterly recommended)

## Monitoring and Alerts

Set up Azure Monitor alerts for:
- CPU time approaching daily limit (50 minutes)
- Memory usage exceeding 80%
- Disk space usage exceeding 80%
- Failed authentication attempts
- AI API errors or rate limits

## Support

For issues or questions:
- Check Azure App Service logs: `https://{your-app}.scm.azurewebsites.net/api/logs/docker`
- Review Application Insights for errors and performance
- Consult Azure documentation: https://docs.microsoft.com/azure/app-service/
