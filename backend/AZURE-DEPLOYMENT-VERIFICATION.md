# Azure Deployment Verification Guide

## Quick Verification Steps

### 1. Check Deployment Status in Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your App Service
3. Check **Overview** page:
   - Status should be "Running"
   - URL should be accessible
4. Check **Deployment Center**:
   - Latest deployment should show "Success"

### 2. Verify Health Endpoint

**Fastest way to check if everything works:**

```bash
curl https://velocify.azurewebsites.net/health
```

Or replace with your actual Azure App Service name if different.

**Expected Response:**
```json
{
  "status": "Healthy",
  "checks": {
    "database": {
      "healthy": true,
      "message": "Database connection successful"
    },
    "langchain": {
      "healthy": true,
      "message": "LangChain service configured"
    },
    "diskSpace": {
      "healthy": true,
      "message": "XX.XX GB available"
    }
  },
  "timestamp": "2026-04-27T..."
}
```

**What Each Check Means:**

- **database**: ✅ Azure SQL connection is working
- **langchain**: ✅ LangChain API key is configured (supports OpenAI, Groq, etc.)
- **diskSpace**: ✅ Sufficient storage available

### 3. Use the HTTP Test File

Open `backend/test-azure-deployment.http` in VS Code with REST Client extension:

1. The file is already configured for `velocify.azurewebsites.net`
2. Run tests in order:
   - ✅ Health Check (Test 1.1)
   - ✅ Register User (Test 3.1)
   - ✅ Login (Test 3.2)
   - ✅ Create Task (Test 4.2)
   - ✅ Get Tasks (Test 4.1)

### 4. Check Azure Logs

If any test fails, check the logs:

**Option A: Azure Portal**
1. Go to your App Service
2. Click **Log stream** in the left menu
3. Watch for errors in real-time

**Option B: Azure CLI**
```bash
az webapp log tail --name velocify --resource-group YOUR-RESOURCE-GROUP
```

**Option C: Download Logs**
```bash
az webapp log download --name velocify --resource-group YOUR-RESOURCE-GROUP
```

### 5. Verify Database Migrations

Check if database tables were created:

1. Go to Azure Portal → SQL Databases
2. Select your database
3. Click **Query editor**
4. Run this query:

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

**Expected Tables:**
- Users
- TaskItems
- Comments
- TaskAuditLogs
- Notifications
- RefreshTokens
- __EFMigrationsHistory

### 6. Test from Browser

Open these URLs in your browser:

1. **Health Check:**
   ```
   https://velocify.azurewebsites.net/health
   ```
   Should return JSON with "Healthy" status

2. **API Documentation (if Swagger is enabled):**
   ```
   https://velocify.azurewebsites.net/swagger
   ```

## Common Issues and Solutions

### Issue 1: Health Check Returns 503 (Service Unavailable)

**Possible Causes:**
- Database connection failed
- OpenAI API key not configured
- Disk space low

**Solution:**
1. Check Azure Portal → App Service → Configuration
2. Verify connection string is set correctly
3. Verify `LangChain__ApiKey` is set in Application Settings
4. Check the health response to see which check failed

### Issue 2: Database Check Fails

**Error:** `"database": { "healthy": false, "message": "Database connection failed" }`

**Solutions:**
1. **Check Connection String:**
   - Azure Portal → App Service → Configuration → Connection strings
   - Name should be `DefaultConnection`
   - Type should be `SQLAzure`

2. **Check Firewall Rules:**
   - Azure Portal → SQL Server → Networking
   - Ensure "Allow Azure services and resources to access this server" is ON
   - Or add your App Service's outbound IP addresses

3. **Test Connection String:**
   ```bash
   # From Azure Cloud Shell
   sqlcmd -S YOUR-SERVER.database.windows.net -d YOUR-DATABASE -U YOUR-USERNAME -P YOUR-PASSWORD -Q "SELECT 1"
   ```

### Issue 3: LangChain Check Fails

**Error:** `"langchain": { "healthy": false, "message": "LangChain API key not configured" }`

**Solution:**
1. Go to Azure Portal → App Service → Configuration → Application settings
2. Add or verify: `LangChain__ApiKey` = `your-api-key`
   - For Groq: `gsk_...`
   - For OpenAI: `sk-proj-...`
3. Optionally add: `LangChain__Model` = `openai/gpt-oss-120b` (for Groq) or `gpt-3.5-turbo` (for OpenAI)
4. Click **Save** and wait for app to restart (takes 30-60 seconds)

### Issue 4: 401 Unauthorized on All Endpoints

**Possible Causes:**
- JWT configuration missing
- Wrong JWT secret key

**Solution:**
1. Check Application Settings in Azure Portal (use double underscores `__`):
   ```
   JwtSettings__SecretKey = your-secret-key-here
   JwtSettings__Issuer = https://velocify.azurewebsites.net
   JwtSettings__Audience = https://velocify.vercel.app
   ```
2. Ensure secret key is at least 32 characters
3. Restart the App Service

### Issue 5: CORS Errors from Frontend

**Error:** `Access to fetch at 'https://...' from origin 'http://localhost:3000' has been blocked by CORS policy`

**Solution:**
1. Check Application Settings (use double underscores `__`):
   ```
   CorsSettings__AllowedOrigins = https://velocify.vercel.app,http://localhost:3000,http://localhost:5173
   ```
2. Separate multiple origins with commas (no spaces)
3. Restart the App Service

### Issue 6: Migrations Not Applied

**Symptoms:** Database exists but tables are missing

**Solution:**
1. Check deployment logs in GitHub Actions
2. Look for migration errors
3. Manually run migrations:
   ```bash
   # From your local machine with Azure SQL connection string
   cd backend/Velocify.API
   dotnet ef database update --connection "YOUR-AZURE-SQL-CONNECTION-STRING"
   ```

## Performance Verification

### Check Response Times

```bash
# Health endpoint should respond in < 2 seconds
time curl https://velocify.azurewebsites.net/health
```

### Check App Service Metrics

1. Azure Portal → App Service → Metrics
2. Monitor:
   - **Response Time**: Should be < 2s for most requests
   - **HTTP Server Errors**: Should be 0
   - **CPU Percentage**: Should be < 70% on F1 tier
   - **Memory Percentage**: Should be < 80%

## Security Verification

### 1. Check HTTPS Redirect

```bash
curl -I http://velocify.azurewebsites.net/health
```

Should return `301` or `302` redirect to HTTPS.

### 2. Verify Authentication

Try accessing protected endpoint without token:

```bash
curl https://velocify.azurewebsites.net/api/v1/tasks
```

Should return `401 Unauthorized`.

### 3. Check Security Headers

```bash
curl -I https://velocify.azurewebsites.net/health
```

Look for security headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Strict-Transport-Security`

## Monitoring Setup (Optional but Recommended)

### Enable Application Insights

1. Azure Portal → App Service → Application Insights
2. Click **Turn on Application Insights**
3. Create new resource or use existing
4. Monitor:
   - Request rates
   - Response times
   - Failed requests
   - Exceptions

### Set Up Alerts

1. Azure Portal → App Service → Alerts
2. Create alert rules for:
   - HTTP 5xx errors > 5 in 5 minutes
   - Response time > 5 seconds
   - CPU usage > 80%

## Next Steps After Verification

Once all checks pass:

1. ✅ **Document your Azure App Service URL** for frontend integration
2. ✅ **Set up custom domain** (optional)
3. ✅ **Configure Application Insights** for monitoring
4. ✅ **Set up alerts** for critical issues
5. ✅ **Test frontend integration** with the deployed backend
6. ✅ **Set up staging environment** (optional)
7. ✅ **Configure backup strategy** for database

## Quick Reference

### Important URLs

- **Health Check:** `https://velocify.azurewebsites.net/health`
- **API Base:** `https://velocify.azurewebsites.net/api/v1`
- **Azure Portal:** https://portal.azure.com
- **GitHub Actions:** https://github.com/YOUR-USERNAME/YOUR-REPO/actions

### Important Azure CLI Commands

```bash
# Check app status
az webapp show --name velocify --resource-group YOUR-RESOURCE-GROUP --query state

# Restart app
az webapp restart --name velocify --resource-group YOUR-RESOURCE-GROUP

# View logs
az webapp log tail --name velocify --resource-group YOUR-RESOURCE-GROUP

# List configuration
az webapp config appsettings list --name velocify --resource-group YOUR-RESOURCE-GROUP
```

## Support

If you encounter issues not covered here:

1. Check `backend/DEPLOYMENT-CHECKLIST.md`
2. Check `backend/CI-CD-FIXES.md`
3. Check `backend/DATABASE-SETUP.md`
4. Review Azure App Service logs
5. Review GitHub Actions logs
