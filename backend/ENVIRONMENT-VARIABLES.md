# Required Environment Variables for Velocify Backend

This document lists all environment variables that must be configured for the Velocify backend to run in production on Azure App Service F1 tier.

## Database Configuration

### AZURE_SQL_CONNECTION_STRING
**Required:** Yes  
**Description:** Connection string for Azure SQL Database  
**Format:** `Server=tcp:{server}.database.windows.net,1433;Initial Catalog={database};Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Min Pool Size=2;Max Pool Size=100;`  
**Example:** `Server=tcp:velocify-sql.database.windows.net,1433;Initial Catalog=VelocifyDb;Persist Security Info=False;User ID=velocifyadmin;Password=YourSecurePassword123!;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Min Pool Size=2;Max Pool Size=100;`  
**Notes:**
- Use Azure SQL Database Serverless tier for cost optimization
- **Connection pooling parameters are CRITICAL for F1 tier:**
  - `Min Pool Size=2` - Keeps 2 connections warm to reduce cold start latency and CPU time
  - `Max Pool Size=100` - Limits connections to prevent memory exhaustion on F1 tier's 1GB RAM
  - These values optimize for F1 tier's 60-minute daily CPU time limit
- Ensure firewall rules allow Azure services to access the database
- **See [AZURE-APP-SERVICE-SETUP.md](AZURE-APP-SERVICE-SETUP.md) for detailed configuration instructions**

## JWT Authentication Configuration

### JWT_SECRET_KEY
**Required:** Yes  
**Description:** Secret key for signing JWT tokens  
**Format:** String (minimum 32 characters)  
**Example:** `your-production-secret-key-at-least-32-characters-long-change-this`  
**Security Notes:**
- MUST be at least 32 characters long
- Use a cryptographically secure random string
- Never commit this value to source control
- Rotate periodically for security
- Generate using: `openssl rand -base64 32` or similar

### JWT_ISSUER
**Required:** Yes  
**Description:** JWT token issuer (your backend API URL)  
**Format:** URL  
**Example:** `https://velocify-api.azurewebsites.net`  
**Notes:** Should match your Azure App Service URL

### JWT_AUDIENCE
**Required:** Yes  
**Description:** JWT token audience (your frontend URL)  
**Format:** URL  
**Example:** `https://velocify.vercel.app`  
**Notes:** Should match your frontend deployment URL

## AI/LangChain Configuration

### LANGCHAIN_API_KEY
**Required:** Yes  
**Description:** API key for Groq AI services (using OpenAI-compatible API)  
**Format:** String (Groq API key format: gsk_...)  
**Example:** `gsk_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`  
**Security Notes:**
- Obtain from Groq platform: https://console.groq.com/keys
- Never commit this value to source control
- Monitor usage to avoid unexpected costs
- Set usage limits in Groq dashboard if available
**Cost Considerations:**
- Azure F1 tier has 60 mins CPU time per day
- AI features consume API credits - monitor usage
- Consider implementing rate limiting for AI endpoints
- Groq offers competitive pricing compared to OpenAI
- Model used: `openai/gpt-oss-120b`

## CORS Configuration

### CORS_ALLOWED_ORIGINS
**Required:** Yes  
**Description:** Semicolon-separated list of allowed frontend origins  
**Format:** `url1;url2;url3`  
**Example:** `https://velocify.vercel.app;https://velocify-staging.vercel.app`  
**Notes:**
- Include all frontend deployment URLs (production, staging, etc.)
- Do NOT include trailing slashes
- Separate multiple origins with semicolons (;)
- For local development, add: `http://localhost:3000;http://localhost:5173`

## Azure App Service Configuration

These are automatically configured by Azure App Service, but you should be aware of them:

### ASPNETCORE_ENVIRONMENT
**Required:** No (auto-configured)  
**Description:** ASP.NET Core environment name  
**Default:** `Production`  
**Options:** `Development`, `Staging`, `Production`  
**Notes:** Azure App Service sets this automatically

### WEBSITE_SITE_NAME
**Required:** No (auto-configured)  
**Description:** Azure App Service site name  
**Notes:** Automatically set by Azure

## Azure F1 Tier Limitations

The Azure App Service F1 (Free) tier has the following limitations:

1. **CPU Time:** 60 minutes per day
   - Monitor usage in Azure Portal → App Service → Metrics → CPU Time
   - AI features consume significant CPU time - monitor usage carefully
   - Set up alerts at 50 minutes to get 10-minute warning
   - Consider upgrading to B1 tier if limits are exceeded
   - **CPU Time Optimization Strategies:**
     - Connection pooling (Min=2, Max=100) reduces connection overhead
     - Compiled queries eliminate repeated LINQ-to-SQL translation
     - Indexed views pre-compute aggregations
     - Production logging set to Warning level for framework code
     - Health checks prevent unnecessary cold starts

2. **Memory:** 1 GB RAM
   - Connection pooling configured to optimize memory usage
   - Serilog configured with rolling file logs (7-day retention)

3. **Storage:** 1 GB disk space
   - Log files stored in `/home/LogFiles/Application/`
   - Automatic cleanup after 7 days
   - Monitor disk usage in Azure Portal

4. **Always On:** Not available
   - First request after idle period will be slow (cold start ~10-30 seconds)
   - Health checks configured to minimize cold starts
   - Consider using Azure Functions or upgrading tier for production

5. **Custom Domains:** Not available
   - Use default `*.azurewebsites.net` domain
   - Upgrade to B1 tier for custom domain support

**For detailed Azure App Service setup and CPU time management, see [AZURE-APP-SERVICE-SETUP.md](AZURE-APP-SERVICE-SETUP.md)**

## Configuration in Azure App Service

To set environment variables in Azure App Service:

1. Navigate to Azure Portal
2. Go to your App Service
3. Select "Configuration" under Settings
4. Click "New application setting"
5. Add each environment variable as a key-value pair
6. Click "Save" and restart the app

**Alternative:** Use Azure CLI:
```bash
az webapp config appsettings set --name velocify-api --resource-group velocify-rg --settings \
  AZURE_SQL_CONNECTION_STRING="your-connection-string" \
  JWT_SECRET_KEY="your-secret-key" \
  JWT_ISSUER="https://velocify-api.azurewebsites.net" \
  JWT_AUDIENCE="https://velocify.vercel.app" \
  LANGCHAIN_API_KEY="sk-proj-..." \
  CORS_ALLOWED_ORIGINS="https://velocify.vercel.app"
```

## Local Development

For local development, use `appsettings.Development.json` (already configured with local values).

**DO NOT** commit `appsettings.Development.json` with real credentials to source control.

## Security Checklist

- [ ] All sensitive values stored as environment variables (not in appsettings.json)
- [ ] JWT_SECRET_KEY is at least 32 characters and cryptographically secure
- [ ] LANGCHAIN_API_KEY has usage limits configured in OpenAI dashboard
- [ ] CORS_ALLOWED_ORIGINS includes only trusted frontend URLs
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

## Cost Optimization Tips

1. **Database:** Use Azure SQL Serverless tier with auto-pause
2. **AI Features:** Implement caching for repeated queries
3. **Logging:** Use 7-day retention to manage disk space
4. **Connection Pooling:** Already configured (Min=2, Max=100)
5. **Health Checks:** Configured to prevent unnecessary cold starts

## Support

For issues or questions:
- Check Azure App Service logs: `https://{your-app}.scm.azurewebsites.net/api/logs/docker`
- Review Application Insights for errors and performance
- Consult Azure documentation: https://docs.microsoft.com/azure/app-service/
