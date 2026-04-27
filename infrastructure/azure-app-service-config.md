# Azure App Service Configuration Guide

This document provides step-by-step instructions for configuring Azure App Service for the Velocify backend API.

## Prerequisites

- Azure subscription with active credits
- Azure CLI installed and configured
- GitHub repository with backend code
- Azure SQL Database created (use `azure-sql-setup.sql`)

## 1. Create Azure App Service

### Using Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource" → "Web App"
3. Configure the following settings:
   - **Subscription**: Select your subscription
   - **Resource Group**: Create new or select existing (e.g., `velocify-rg`)
   - **Name**: `velocify` (must be globally unique)
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Linux
   - **Region**: Choose closest to your users (e.g., East US)
   - **Pricing Plan**: F1 (Free tier) for development, B1 or higher for production

4. Click "Review + Create" → "Create"

### Using Azure CLI

```bash
# Login to Azure
az login

# Create resource group
az group create --name velocify-rg --location eastus

# Create App Service Plan (F1 Free tier)
az appservice plan create \
  --name velocify-plan \
  --resource-group velocify-rg \
  --sku F1 \
  --is-linux

# Create Web App
az webapp create \
  --name velocify \
  --resource-group velocify-rg \
  --plan velocify-plan \
  --runtime "DOTNETCORE:8.0"
```

## 2. Configure Application Settings (Environment Variables)

### Required Environment Variables

Add these configuration settings in Azure Portal → App Service → Configuration → Application settings:

| Name | Value | Description |
|------|-------|-------------|
| `ConnectionStrings__DefaultConnection` | `Server=tcp:your-server.database.windows.net,1433;Initial Catalog=VelocifyDB;User ID=VelocifyAppUser;Password={password};MultipleActiveResultSets=True;Encrypt=True;Min Pool Size=2;Max Pool Size=100;` | Azure SQL connection string |
| `JwtSettings__SecretKey` | `{generate-secure-key}` | JWT signing key (min 32 characters) |
| `JwtSettings__Issuer` | `https://velocify.azurewebsites.net` | JWT issuer |
| `JwtSettings__Audience` | `https://velocify.azurewebsites.net` | JWT audience |
| `JwtSettings__AccessTokenExpirationMinutes` | `15` | Access token TTL |
| `JwtSettings__RefreshTokenExpirationDays` | `7` | Refresh token TTL |
| `LangChain__ApiKey` | `{your-openai-api-key}` | OpenAI API key for LangChain |
| `LangChain__Model` | `gpt-4` | LangChain model name |
| `LangChain__MaxTokens` | `2000` | Max tokens per request |
| `CorsSettings__AllowedOrigins` | `https://your-frontend.vercel.app` | Frontend URL for CORS |
| `Serilog__MinimumLevel__Default` | `Information` | Logging level |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment name |

### Using Azure Portal

1. Navigate to App Service → Configuration → Application settings
2. Click "+ New application setting" for each variable
3. Click "Save" after adding all settings
4. Click "Continue" to restart the app

### Using Azure CLI

```bash
# Set connection string
az webapp config connection-string set \
  --name velocify \
  --resource-group velocify-rg \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:your-server.database.windows.net,1433;Initial Catalog=VelocifyDB;User ID=VelocifyAppUser;Password={password};MultipleActiveResultSets=True;Encrypt=True;Min Pool Size=2;Max Pool Size=100;"

# Set application settings
az webapp config appsettings set \
  --name velocify \
  --resource-group velocify-rg \
  --settings \
    JwtSettings__SecretKey="{generate-secure-key}" \
    JwtSettings__Issuer="https://velocify.azurewebsites.net" \
    JwtSettings__Audience="https://velocify.azurewebsites.net" \
    JwtSettings__AccessTokenExpirationMinutes="15" \
    JwtSettings__RefreshTokenExpirationDays="7" \
    LangChain__ApiKey="{your-openai-api-key}" \
    LangChain__Model="gpt-4" \
    LangChain__MaxTokens="2000" \
    CorsSettings__AllowedOrigins="https://your-frontend.vercel.app" \
    Serilog__MinimumLevel__Default="Information" \
    ASPNETCORE_ENVIRONMENT="Production"
```

## 3. Configure Deployment

### Option A: GitHub Actions (Recommended)

1. In Azure Portal, navigate to App Service → Deployment Center
2. Select "GitHub" as source
3. Authorize GitHub access
4. Select your repository and branch (main)
5. Azure will generate a publish profile
6. Download the publish profile
7. In GitHub repository, go to Settings → Secrets and variables → Actions
8. Add new secret: `AZURE_WEBAPP_PUBLISH_PROFILE` with the downloaded profile content
9. The workflow file `.github/workflows/azure-app-service.yml` will handle deployments

### Option B: Azure DevOps

1. Create Azure DevOps project
2. Create pipeline using `azure-pipelines.yml`
3. Configure service connection to Azure
4. Run pipeline

### Option C: Manual Deployment

```bash
# Publish the application
cd backend
dotnet publish Velocify.API/Velocify.API.csproj -c Release -o ./publish

# Deploy to Azure
az webapp deployment source config-zip \
  --name velocify \
  --resource-group velocify-rg \
  --src ./publish.zip
```

## 4. Configure Health Check

1. Navigate to App Service → Health check
2. Enable health check
3. Set path to `/health`
4. Set interval to 5 minutes
5. Unhealthy threshold: 3 consecutive failures
6. Click "Save"

## 5. Configure Logging

### Application Insights (Recommended)

```bash
# Create Application Insights
az monitor app-insights component create \
  --app velocify-insights \
  --location eastus \
  --resource-group velocify-rg \
  --application-type web

# Get instrumentation key
az monitor app-insights component show \
  --app velocify-insights \
  --resource-group velocify-rg \
  --query instrumentationKey

# Add to App Service settings
az webapp config appsettings set \
  --name velocify \
  --resource-group velocify-rg \
  --settings ApplicationInsights__InstrumentationKey="{instrumentation-key}"
```

### App Service Logs

1. Navigate to App Service → App Service logs
2. Enable "Application Logging (Filesystem)"
3. Set level to "Information"
4. Enable "Detailed error messages"
5. Enable "Failed request tracing"
6. Click "Save"

## 6. Configure Scaling (Optional)

### Manual Scaling

1. Navigate to App Service → Scale up (App Service plan)
2. Select appropriate tier (B1, S1, P1V2, etc.)
3. Click "Apply"

### Auto-scaling (Requires Standard tier or higher)

1. Navigate to App Service → Scale out (App Service plan)
2. Enable "Custom autoscale"
3. Add scale rule:
   - Metric: CPU Percentage
   - Operator: Greater than
   - Threshold: 70
   - Duration: 5 minutes
   - Action: Increase count by 1
4. Add scale rule for scale down:
   - Metric: CPU Percentage
   - Operator: Less than
   - Threshold: 30
   - Duration: 5 minutes
   - Action: Decrease count by 1
5. Set instance limits (min: 1, max: 3)
6. Click "Save"

## 7. Configure Custom Domain (Optional)

1. Purchase domain or use existing
2. Navigate to App Service → Custom domains
3. Click "+ Add custom domain"
4. Enter your domain name
5. Add DNS records as instructed:
   - CNAME: `www` → `velocify.azurewebsites.net`
   - TXT: `asuid` → `{verification-id}`
6. Click "Validate" → "Add"

## 8. Configure SSL Certificate

### Option A: Free Managed Certificate (Recommended)

1. Navigate to App Service → TLS/SSL settings
2. Click "Private Key Certificates (.pfx)"
3. Click "+ Create App Service Managed Certificate"
4. Select your custom domain
5. Click "Create"
6. Navigate to Custom domains
7. Click "Add binding" for your domain
8. Select the managed certificate
9. Click "Add"

### Option B: Upload Custom Certificate

1. Navigate to App Service → TLS/SSL settings
2. Click "Private Key Certificates (.pfx)"
3. Click "+ Upload Certificate"
4. Select your .pfx file and enter password
5. Click "Upload"
6. Bind to custom domain as above

## 9. Run Database Migrations

After deployment, run EF Core migrations:

```bash
# Option 1: From local machine
cd backend
dotnet ef database update \
  --project Velocify.Infrastructure/Velocify.Infrastructure.csproj \
  --startup-project Velocify.API/Velocify.API.csproj \
  --connection "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=VelocifyDB;User ID=VelocifyAppUser;Password={password};MultipleActiveResultSets=True;Encrypt=True;"

# Option 2: Migrations run automatically on app startup (configured in Program.cs)
# The application will apply pending migrations when it starts
```

## 10. Verify Deployment

1. Navigate to `https://velocify.azurewebsites.net/health`
2. Should return 200 OK with health check status
3. Navigate to `https://velocify.azurewebsites.net/swagger`
4. Should display Swagger UI with API documentation
5. Test authentication endpoint: POST `/api/v1/auth/register`

## 11. Security Checklist

- [ ] All secrets stored in Azure Key Vault or App Service configuration (not in code)
- [ ] HTTPS enforced (HTTP redirects to HTTPS)
- [ ] CORS configured with specific origins (not wildcard)
- [ ] JWT secret key is strong (min 32 characters, randomly generated)
- [ ] Database connection uses encrypted connection (Encrypt=True)
- [ ] Application Insights enabled for monitoring
- [ ] Health check endpoint configured
- [ ] Firewall rules configured on Azure SQL (allow Azure services)
- [ ] Managed identity enabled for App Service (optional, for Key Vault access)

## 12. Monitoring and Troubleshooting

### View Logs

```bash
# Stream logs in real-time
az webapp log tail --name velocify --resource-group velocify-rg

# Download logs
az webapp log download --name velocify --resource-group velocify-rg
```

### Common Issues

**Issue**: App fails to start
- Check Application Insights or App Service logs
- Verify all required environment variables are set
- Check database connection string

**Issue**: Database connection fails
- Verify Azure SQL firewall allows Azure services
- Check connection string format
- Verify database user has correct permissions

**Issue**: Migrations fail
- Ensure VelocifyAppUser has db_ddladmin role
- Check connection string in migration command
- Verify EF Core tools are installed

**Issue**: 502 Bad Gateway
- App is starting (wait 1-2 minutes)
- Check App Service logs for startup errors
- Verify .NET 8 runtime is selected

## 13. Cost Optimization

- Use F1 (Free) tier for development/testing
- Use B1 (Basic) tier for small production workloads
- Enable auto-pause on Azure SQL Serverless (included in setup script)
- Monitor Application Insights costs (sampling enabled by default)
- Use deployment slots for staging (requires Standard tier)

## 14. Backup and Disaster Recovery

### Database Backups

Azure SQL automatically creates backups:
- Full backup: Weekly
- Differential backup: Every 12-24 hours
- Transaction log backup: Every 5-10 minutes
- Retention: 7-35 days (configurable)

### App Service Backups

1. Navigate to App Service → Backups
2. Configure storage account
3. Set backup schedule
4. Click "Save"

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/azure-sql/)
- [EF Core Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [Application Insights](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)
