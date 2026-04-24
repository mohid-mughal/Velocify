# Azure App Service Configuration Reference

This is a quick reference for all Azure App Service configuration settings. For detailed setup instructions, see [AZURE-APP-SERVICE-SETUP.md](AZURE-APP-SERVICE-SETUP.md).

## Application Settings (Environment Variables)

Configure these in: **Azure Portal → App Service → Configuration → Application settings**

| Setting Name | Required | Example Value | Notes |
|-------------|----------|---------------|-------|
| `AZURE_SQL_CONNECTION_STRING` | ✅ Yes | `Server=tcp:velocify-sql.database.windows.net,1433;Initial Catalog=VelocifyDb;User ID=admin;Password=Pass123!;MultipleActiveResultSets=True;Encrypt=True;Min Pool Size=2;Max Pool Size=100;` | **CRITICAL:** Must include `Min Pool Size=2;Max Pool Size=100;` for F1 tier CPU time optimization |
| `JWT_SECRET_KEY` | ✅ Yes | `3K8vN2pQ9mR5sT7wX0yZ1aB4cD6eF8gH9iJ0kL2mN4oP6qR8sT0uV2wX4yZ6aB8c` | Generate with `openssl rand -base64 32` |
| `JWT_ISSUER` | ✅ Yes | `https://velocify-api.azurewebsites.net` | Your backend API URL |
| `JWT_AUDIENCE` | ✅ Yes | `https://velocify.vercel.app` | Your frontend URL |
| `LANGCHAIN_API_KEY` | ✅ Yes | `sk-proj-abc123...` | OpenAI API key from https://platform.openai.com/api-keys |
| `CORS_ALLOWED_ORIGINS` | ✅ Yes | `https://velocify.vercel.app;https://velocify-staging.vercel.app` | Semicolon-separated list of frontend URLs |
| `ASPNETCORE_ENVIRONMENT` | ⚠️ Auto | `Production` | Automatically set by Azure, do not override |

## Connection String Configuration

The connection string **MUST** include these pooling parameters for F1 tier:

```
Min Pool Size=2;Max Pool Size=100;
```

**Why this matters for F1 tier:**
- **Min Pool Size=2**: Keeps 2 connections warm, reducing cold start latency and CPU time
- **Max Pool Size=100**: Prevents memory exhaustion on F1 tier's 1GB RAM limit
- Without these, you'll waste CPU time on connection creation/disposal

## General Settings

Configure these in: **Azure Portal → App Service → Configuration → General settings**

| Setting | Recommended Value | Notes |
|---------|------------------|-------|
| **Stack** | .NET 8 (LTS) | Required for ASP.NET Core 8 |
| **Platform** | 64 Bit | Better performance |
| **Always On** | ❌ OFF | Not available in F1 tier |
| **ARR affinity** | ❌ OFF | Not needed for stateless API |
| **HTTPS Only** | ✅ ON | Force HTTPS for security |
| **Minimum TLS Version** | 1.2 | Security best practice |

## Deployment Settings

Configure these in: **Azure Portal → App Service → Deployment Center**

| Setting | Value | Notes |
|---------|-------|-------|
| **Source** | GitHub Actions | Automated deployment |
| **Organization** | Your GitHub username/org | |
| **Repository** | velocify-platform | Your repo name |
| **Branch** | main | Production branch |

## Monitoring and Alerts

### CPU Time Alert (CRITICAL for F1 Tier)

**Azure Portal → App Service → Alerts → New alert rule**

| Setting | Value |
|---------|-------|
| **Metric** | CPU Time |
| **Operator** | Greater than |
| **Threshold** | 50 minutes |
| **Period** | 24 hours |
| **Action** | Send email notification |

This gives you a 10-minute warning before hitting the 60-minute daily limit.

### Memory Alert

**Azure Portal → App Service → Alerts → New alert rule**

| Setting | Value |
|---------|-------|
| **Metric** | Memory Percentage |
| **Operator** | Greater than |
| **Threshold** | 80% |
| **Period** | 5 minutes |
| **Action** | Send email notification |

### HTTP 5xx Errors Alert

**Azure Portal → App Service → Alerts → New alert rule**

| Setting | Value |
|---------|-------|
| **Metric** | Http Server Errors |
| **Operator** | Greater than |
| **Threshold** | 10 |
| **Period** | 5 minutes |
| **Action** | Send email notification |

## Networking Settings

### CORS (Configured in Code)

CORS is configured in the application code (`Program.cs`) using the `CORS_ALLOWED_ORIGINS` environment variable.

Do NOT configure CORS in Azure Portal - it will conflict with the application configuration.

### Azure SQL Firewall

**Azure Portal → SQL Server → Networking → Firewall rules**

Add rule:
- **Name:** AllowAzureServices
- **Start IP:** 0.0.0.0
- **End IP:** 0.0.0.0

This allows your App Service to connect to Azure SQL Database.

## Logging Configuration

### Application Logging

**Azure Portal → App Service → App Service logs**

| Setting | Value | Notes |
|---------|-------|-------|
| **Application Logging (Filesystem)** | ✅ ON | Enable for troubleshooting |
| **Level** | Information | Balance between detail and disk space |
| **Quota (MB)** | 35 | Limit disk usage |
| **Retention Period (Days)** | 7 | Auto-cleanup to save disk space |

### Web Server Logging

**Azure Portal → App Service → App Service logs**

| Setting | Value | Notes |
|---------|-------|-------|
| **Web server logging** | ✅ ON | Enable for troubleshooting |
| **Retention Period (Days)** | 7 | Auto-cleanup to save disk space |

## Health Check Configuration

**Azure Portal → App Service → Health check**

| Setting | Value | Notes |
|---------|-------|-------|
| **Enable health check** | ✅ ON | Helps minimize cold starts |
| **Path** | `/health` | Health check endpoint |
| **Interval** | 300 seconds | Check every 5 minutes |

**Note:** Health checks help keep the app warm but consume CPU time. 5-minute interval balances availability and CPU usage for F1 tier.

## Deployment Slots

❌ **Not available in F1 tier**

Upgrade to B1 or higher for staging slots.

## Custom Domains

❌ **Not available in F1 tier**

Your app will be accessible at: `https://velocify-api.azurewebsites.net`

Upgrade to B1 or higher for custom domain support.

## Scale Out (Auto-scaling)

❌ **Not available in F1 tier**

F1 tier is limited to 1 instance.

Upgrade to B1 or higher for auto-scaling.

## Backup

❌ **Not available in F1 tier**

Ensure your code is in source control (GitHub) and database has automated backups enabled.

## Quick Setup Checklist

Use this checklist when setting up a new Azure App Service:

- [ ] Create App Service with F1 tier
- [ ] Configure all 6 required environment variables
- [ ] Verify connection string includes `Min Pool Size=2;Max Pool Size=100;`
- [ ] Enable HTTPS Only
- [ ] Set Minimum TLS Version to 1.2
- [ ] Disable ARR affinity
- [ ] Configure Azure SQL firewall to allow Azure services
- [ ] Set up CPU time alert (50 minutes threshold)
- [ ] Set up memory alert (80% threshold)
- [ ] Set up HTTP 5xx errors alert
- [ ] Enable Application Logging (7-day retention)
- [ ] Enable Web Server Logging (7-day retention)
- [ ] Enable Health Check (`/health` endpoint, 5-minute interval)
- [ ] Configure GitHub Actions deployment
- [ ] Add `AZURE_WEBAPP_PUBLISH_PROFILE` secret to GitHub
- [ ] Test deployment by pushing to main branch
- [ ] Verify health endpoint: `https://your-app.azurewebsites.net/health`
- [ ] Test authentication endpoint
- [ ] Monitor CPU time usage for first 24 hours

## Troubleshooting Quick Reference

| Issue | Solution |
|-------|----------|
| 500 errors | Check Application Logs in Azure Portal |
| Database connection fails | Verify firewall rules and connection string |
| Cold starts too slow | Expected on F1 tier (no Always On) - upgrade to B1 |
| CPU time limit exceeded | Monitor usage, optimize AI calls, or upgrade to B1 |
| Out of memory | Check connection pooling parameters, review logs |
| Deployment fails | Check GitHub Actions logs, verify secrets |
| CORS errors | Verify `CORS_ALLOWED_ORIGINS` includes frontend URL |
| JWT validation fails | Check `JWT_ISSUER` and `JWT_AUDIENCE` match URLs |

## Cost Optimization Tips

1. **Monitor CPU time daily** - Set up alerts at 50 minutes
2. **Cache AI responses** - Reduce repeated AI calls
3. **Use compiled queries** - Already configured in code
4. **Optimize logging** - Production logs set to Warning level
5. **Connection pooling** - Already configured (Min=2, Max=100)
6. **7-day log retention** - Automatic cleanup saves disk space
7. **Health check interval** - 5 minutes balances availability and CPU usage

## When to Upgrade to B1 Tier

Consider upgrading to B1 tier (~$13/month) if:

- ✅ You exceed 60 minutes CPU time per day
- ✅ Cold starts impact user experience
- ✅ You need custom domain support
- ✅ You need deployment slots for staging
- ✅ You need auto-scaling capabilities
- ✅ You need more than 1GB RAM or disk space

## Additional Resources

- [Detailed Setup Guide](AZURE-APP-SERVICE-SETUP.md)
- [Environment Variables Documentation](ENVIRONMENT-VARIABLES.md)
- [Deployment Checklist](DEPLOYMENT-CHECKLIST.md)
- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [ASP.NET Core on Azure](https://docs.microsoft.com/aspnet/core/host-and-deploy/azure-apps/)
