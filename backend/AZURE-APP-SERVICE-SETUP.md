# Azure App Service F1 Tier Setup Guide

This guide walks you through deploying the Velocify backend to Azure App Service F1 (Free) tier.

## Prerequisites

- Azure account (free tier available)
- Azure CLI installed (optional, for command-line setup)
- GitHub repository with backend code
- Azure SQL Database created (see database setup guide)

## Important: F1 Tier Limitations

⚠️ **Azure App Service F1 (Free) tier has strict limitations:**

### CPU Time Quota
- **60 minutes of CPU time per day** (resets at midnight UTC)
- Monitor usage: Azure Portal → App Service → Metrics → CPU Time
- If exceeded, app will stop responding until quota resets
- Consider upgrading to B1 tier (~$13/month) for production use

### Memory Limitations
- **1 GB RAM** maximum
- Connection pooling configured (Min=2, Max=100) to optimize memory
- Serilog configured with 7-day log retention to manage disk space

### Always On NOT Available
- **Cold starts expected** after 20 minutes of inactivity
- First request after idle will take 10-30 seconds
- Health checks configured to minimize cold starts
- For production, upgrade to B1 tier for Always On feature

### Other Limitations
- **1 GB disk space** - logs auto-cleanup after 7 days
- **No custom domains** - use `*.azurewebsites.net` subdomain
- **No deployment slots** - direct production deployment only
- **No auto-scaling** - single instance only

## Step 1: Create Azure App Service

### Option A: Azure Portal (Recommended for beginners)

1. Go to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource" → "Web App"
3. Fill in the details:
   - **Subscription:** Your Azure subscription
   - **Resource Group:** Create new or use existing (e.g., `velocify-rg`)
   - **Name:** `velocify` (must be globally unique)
   - **Publish:** Code
   - **Runtime stack:** .NET 8 (LTS)
   - **Operating System:** Linux (recommended) or Windows
   - **Region:** Choose closest to your users
   - **Pricing Plan:** F1 (Free)
4. Click "Review + Create" → "Create"
5. Wait for deployment to complete

### Option B: Azure CLI

```bash
# Login to Azure
az login

# Create resource group
az group create --name velocify-rg --location eastus

# Create App Service plan (F1 tier)
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

## Step 2: Configure Environment Variables

You need to configure the following environment variables in Azure App Service:

### Navigate to Configuration

1. Azure Portal → Your App Service → Configuration
2. Click "New application setting" for each variable below
3. Click "Save" after adding all variables
4. Click "Continue" to restart the app

### Required Environment Variables

#### 1. Database Connection String

**Name:** `AZURE_SQL_CONNECTION_STRING`

**Value Format:**
```
Server=tcp:{your-server}.database.windows.net,1433;Initial Catalog={your-database};User ID={your-username};Password={your-password};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Min Pool Size=2;Max Pool Size=100;
```

**Important Connection Pooling Parameters:**
- `Min Pool Size=2` - Keeps 2 connections warm to reduce cold start latency
- `Max Pool Size=100` - Limits connections to prevent memory exhaustion on F1 tier
- These values are optimized for F1 tier's 1GB RAM limit

**Example:**
```
Server=tcp:velocify-sql.database.windows.net,1433;Initial Catalog=VelocifyDb;User ID=velocifyadmin;Password=YourSecurePassword123!;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Min Pool Size=2;Max Pool Size=100;
```

#### 2. JWT Secret Key

**Name:** `JWT_SECRET_KEY`

**How to generate:**
```bash
openssl rand -base64 32
```

**Example:**
```
3K8vN2pQ9mR5sT7wX0yZ1aB4cD6eF8gH9iJ0kL2mN4oP6qR8sT0uV2wX4yZ6aB8c
```

**Security Notes:**
- Must be at least 32 characters
- Never commit to source control
- Rotate quarterly for security

#### 3. JWT Issuer

**Name:** `JWT_ISSUER`

**Value:** Your backend API URL

**Example:**
```
https://velocify.azurewebsites.net
```

#### 4. JWT Audience

**Name:** `JWT_AUDIENCE`

**Value:** Your frontend URL

**Example:**
```
https://velocify-work.vercel.app
```

#### 5. LangChain/Groq API Key

**Name:** `LANGCHAIN_API_KEY`

**Where to get:** https://console.groq.com/keys

**Format:** `gsk-...`

**Example:**
```
gsk-abc123def456ghi789jkl012mno345pqr678stu901vwx234yz
```

**Cost Management:**
- Monitor usage in Groq dashboard
- Groq offers competitive pricing and fast inference
- AI features consume CPU time - be mindful of F1 tier's 60-minute daily limit
- Model: `openai/gpt-oss-120b`

#### 6. CORS Allowed Origins

**Name:** `CORS_ALLOWED_ORIGINS`

**Value:** Semicolon-separated list of frontend URLs

**Example:**
```
https://velocify-work.vercel.app;https://velocify-staging.vercel.app
```

**Notes:**
- Include all frontend deployment URLs
- No trailing slashes
- Separate with semicolons (;)

### Azure CLI Method

```bash
# Set all environment variables at once
az webapp config appsettings set \
  --name velocify \
  --resource-group velocify-rg \
  --settings \
    AZURE_SQL_CONNECTION_STRING="Server=tcp:velocify-sql.database.windows.net,1433;Initial Catalog=VelocifyDb;User ID=velocifyadmin;Password=YourPassword123!;MultipleActiveResultSets=True;Encrypt=True;Min Pool Size=2;Max Pool Size=100;" \
    JWT_SECRET_KEY="your-generated-secret-key" \
    JWT_ISSUER="https://velocify.azurewebsites.net" \
    JWT_AUDIENCE="https://velocify-work.vercel.app" \
    LANGCHAIN_API_KEY="gsk-your-api-key" \
    CORS_ALLOWED_ORIGINS="https://velocify-work.vercel.app"
```

## Step 3: Configure GitHub Actions Deployment

### Get Publish Profile

1. Azure Portal → Your App Service → Deployment Center
2. Click "Manage publish profile" → "Download publish profile"
3. Save the `.PublishSettings` file

### Add GitHub Secret

1. Go to your GitHub repository
2. Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
5. Value: Paste the entire contents of the `.PublishSettings` file
6. Click "Add secret"

### Verify Workflow

The GitHub Actions workflow is already configured in `.github/workflows/azure-app-service.yml`.

It will automatically deploy when you push to the `main` branch with changes in the `backend/` directory.

## Step 4: Configure Azure SQL Firewall

Your App Service needs access to Azure SQL Database:

### Option A: Azure Portal

1. Azure Portal → Your SQL Server → Networking
2. Under "Firewall rules", check "Allow Azure services and resources to access this server"
3. Click "Save"

### Option B: Azure CLI

```bash
az sql server firewall-rule create \
  --resource-group velocify-rg \
  --server velocify-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

## Step 5: Deploy the Application

### Automatic Deployment (Recommended)

1. Push your code to the `main` branch:
   ```bash
   git add .
   git commit -m "Configure Azure deployment"
   git push origin main
   ```

2. Monitor deployment:
   - GitHub → Your repository → Actions tab
   - Watch the "Deploy Backend to Azure App Service" workflow

3. Deployment steps:
   - Checkout code
   - Setup .NET 8
   - Restore dependencies
   - Build project
   - Run tests
   - Publish artifacts
   - Deploy to Azure App Service
   - Run EF Core migrations

### Manual Deployment (Alternative)

```bash
# Navigate to backend directory
cd backend

# Publish the application
dotnet publish Velocify.API/Velocify.API.csproj \
  --configuration Release \
  --output ./publish

# Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group velocify-rg \
  --name velocify \
  --src ./publish.zip
```

## Step 6: Verify Deployment

### Check Health Endpoint

```bash
curl https://velocify.azurewebsites.net/health
```

Expected response:
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "langchain": "Healthy",
    "disk_space": "Healthy"
  }
}
```

### Test Authentication

```bash
# Register a new user
curl -X POST https://velocify.azurewebsites.net/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Test",
    "lastName": "User",
    "email": "test@example.com",
    "password": "SecurePassword123!"
  }'
```

### Check Logs

1. Azure Portal → Your App Service → Log stream
2. Or use Azure CLI:
   ```bash
   az webapp log tail --name velocify --resource-group velocify-rg
   ```

## Step 7: Monitor CPU Time Usage (Critical for F1 Tier)

### View CPU Time Metrics

1. Azure Portal → Your App Service → Metrics
2. Add metric: "CPU Time"
3. Set time range: "Last 24 hours"
4. Monitor daily usage to ensure you stay under 60 minutes

### Set Up Alerts

1. Azure Portal → Your App Service → Alerts
2. Click "New alert rule"
3. Condition: "CPU Time" > 50 minutes
4. Action: Send email notification
5. This gives you a 10-minute warning before hitting the limit

### CPU Time Optimization Tips

- **Minimize AI calls:** Cache AI responses when possible
- **Optimize queries:** Use compiled queries and indexed views (already configured)
- **Reduce logging:** Production logging set to Warning level for framework code
- **Health checks:** Configured to prevent unnecessary cold starts
- **Connection pooling:** Min=2, Max=100 (already configured)

### If You Exceed CPU Time Limit

Your app will stop responding until midnight UTC when the quota resets.

**Solutions:**
1. **Wait:** Quota resets at midnight UTC
2. **Upgrade:** Switch to B1 tier (~$13/month) for unlimited CPU time
3. **Optimize:** Review logs to identify CPU-intensive operations

## Step 8: Configure Custom Domain (Optional - Requires Upgrade)

Custom domains are NOT available on F1 tier. You must upgrade to B1 or higher.

Your app will be accessible at: `https://velocify.azurewebsites.net`

## Troubleshooting

### Issue: App returns 500 errors

**Solution:**
1. Check environment variables are set correctly
2. Verify database connection string
3. Check logs: Azure Portal → App Service → Log stream

### Issue: Database connection fails

**Solution:**
1. Verify Azure SQL firewall allows Azure services
2. Check connection string format
3. Ensure database exists and credentials are correct

### Issue: Cold starts are too slow

**Solution:**
1. This is expected on F1 tier (no Always On)
2. First request after idle takes 10-30 seconds
3. Upgrade to B1 tier for Always On feature

### Issue: CPU time limit exceeded

**Solution:**
1. Monitor which endpoints consume most CPU
2. Optimize AI feature usage
3. Consider upgrading to B1 tier

### Issue: Migrations fail during deployment

**Solution:**
1. Check GitHub Actions logs for error details
2. Verify `AZURE_SQL_CONNECTION_STRING` secret is set
3. Ensure database firewall allows GitHub Actions IP

## Security Checklist

- [ ] All environment variables configured in Azure App Service (not in code)
- [ ] JWT secret key is at least 32 characters and cryptographically secure
- [ ] Azure SQL firewall configured properly
- [ ] CORS origins include only trusted frontend URLs
- [ ] OpenAI API key has usage limits set
- [ ] Publish profile stored as GitHub secret (not committed to repo)
- [ ] HTTPS enforced (automatic on Azure App Service)
- [ ] Security headers configured in web.config

## Cost Monitoring

### Current Configuration Costs

- **Azure App Service F1:** Free
- **Azure SQL Serverless:** ~$5-15/month (depends on usage)
- **Groq API:** Free tier available, then pay-as-you-go (competitive pricing)
- **Total estimated:** ~$5-20/month for low-traffic usage

### Set Up Cost Alerts

1. Azure Portal → Cost Management + Billing
2. Budgets → Add budget
3. Set monthly budget (e.g., $20)
4. Configure alert at 80% threshold

## Upgrading to B1 Tier

If F1 tier limitations are too restrictive, upgrade to B1:

### Benefits of B1 Tier (~$13/month)

- **Unlimited CPU time** - no daily quota
- **Always On** - no cold starts
- **1.75 GB RAM** - 75% more memory
- **10 GB disk space** - 10x more storage
- **Custom domains** - use your own domain
- **Deployment slots** - staging environment support
- **Auto-scaling** - scale up to 3 instances

### How to Upgrade

1. Azure Portal → Your App Service → Scale up (App Service plan)
2. Select "B1" tier
3. Click "Apply"
4. No code changes needed - instant upgrade

## Next Steps

1. ✅ Deploy backend to Azure App Service
2. ✅ Configure environment variables
3. ✅ Verify health endpoint
4. ✅ Test authentication
5. ✅ Monitor CPU time usage
6. 📝 Deploy frontend to Vercel (see frontend deployment guide)
7. 📝 Configure frontend to use Azure backend URL
8. 📝 Test end-to-end functionality

## Support

For issues or questions:
- Check Azure App Service logs
- Review GitHub Actions workflow logs
- Consult Azure documentation: https://docs.microsoft.com/azure/app-service/
- Check OpenAI API status: https://status.openai.com/

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [ASP.NET Core on Azure](https://docs.microsoft.com/aspnet/core/host-and-deploy/azure-apps/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/azure-sql/)
- [GitHub Actions for Azure](https://docs.microsoft.com/azure/developer/github/github-actions)
- [OpenAI API Documentation](https://platform.openai.com/docs/)
