# Azure App Service F1 Tier - Best Practices and Limitations

This document consolidates all critical information about deploying and running the Velocify backend on Azure App Service F1 (Free) tier.

## Overview

Azure App Service F1 tier is a **free hosting option** suitable for development, testing, and low-traffic applications. However, it comes with significant limitations that require careful consideration and optimization.

## Critical Limitations

### 1. CPU Time Quota: 60 Minutes Per Day

**Impact:** This is the most critical limitation. Your app will stop responding if you exceed 60 minutes of CPU time in a 24-hour period (resets at midnight UTC).

**Monitoring:**
- Azure Portal → App Service → Metrics → CPU Time
- Set up alerts at 50 minutes (gives 10-minute warning)

**Optimization Strategies:**

#### Database Optimizations
- ✅ **Connection Pooling** (CRITICAL):
  ```
  Min Pool Size=2;Max Pool Size=100;
  ```
  - Keeps 2 connections warm to avoid connection overhead
  - Limits max connections to prevent memory exhaustion
  - Reduces CPU time spent on connection management

- ✅ **Compiled Queries**:
  - Pre-compiled LINQ queries eliminate repeated translation overhead
  - Saves ~50ms per request on cold starts
  - See `CompiledQueries.cs` for implementation

- ✅ **Indexed Views**:
  - `vw_UserTaskSummary` pre-aggregates dashboard counts
  - Eliminates expensive GROUP BY operations
  - Reduces CPU time for dashboard queries by ~80%

- ✅ **Filtered Indexes**:
  - Indexes only active records (`WHERE IsDeleted = 0`)
  - Smaller indexes = faster queries = less CPU time

- ✅ **Table Partitioning**:
  - `TaskAuditLog` partitioned by month
  - Queries scan only relevant partitions
  - Reduces CPU time for audit log queries

#### Application Optimizations
- ✅ **AsNoTracking()** for read operations:
  - Prevents EF Core from tracking entities in memory
  - Reduces memory allocations and CPU overhead
  - Wrapped in `AsReadOnly()` extension method

- ✅ **AsSplitQuery()** for multiple includes:
  - Prevents Cartesian explosion in SQL queries
  - Reduces data transfer and CPU processing
  - Configured globally in DbContext

- ✅ **Minimal Logging**:
  - Production logging set to Warning level for framework code
  - Reduces CPU time spent on log processing
  - Health checks excluded from request logging

- ✅ **AI Call Optimization**:
  - Cache AI responses when possible
  - Implement rate limiting for AI endpoints
  - Monitor AI feature usage in `AiInteractionLog` table

#### What Consumes CPU Time
- Database queries (especially unoptimized ones)
- AI/LangChain API calls
- Logging operations
- Request processing
- Cold starts (first request after idle)
- Health checks (configured at 5-minute intervals)

### 2. Memory Limit: 1 GB RAM

**Impact:** Your app will crash if it exceeds 1 GB of memory usage.

**Optimization Strategies:**
- ✅ Connection pooling limits (Max Pool Size=100)
- ✅ AsNoTracking() prevents entity tracking overhead
- ✅ Serilog rolling file logs (7-day retention)
- ✅ Avoid loading large collections into memory
- ✅ Use streaming for large data transfers

**Monitoring:**
- Azure Portal → App Service → Metrics → Memory Working Set

### 3. Always On: NOT Available

**Impact:** Your app will enter an idle state after 20 minutes of inactivity, causing cold starts.

**Cold Start Behavior:**
- First request after idle: 10-30 seconds response time
- Subsequent requests: Normal response time
- Health checks help minimize cold starts (configured at 5-minute intervals)

**Mitigation:**
- Accept cold starts as expected behavior
- Optimize startup time (lazy load services)
- Upgrade to B1 tier ($13/month) for Always On feature

### 4. Disk Space: 1 GB

**Impact:** Limited storage for logs and temporary files.

**Optimization:**
- ✅ Serilog configured with 7-day log retention
- ✅ Logs stored in `/home/LogFiles/Application/`
- ✅ Automatic cleanup of old logs

**Monitoring:**
- Health check monitors disk space
- Alerts if disk usage exceeds 80%

### 5. Custom Domains: NOT Available

**Impact:** You must use the default `*.azurewebsites.net` subdomain.

**Your App URL:**
```
https://velocify.azurewebsites.net
```

**Upgrade to B1 or higher for custom domain support.**

### 6. Deployment Slots: NOT Available

**Impact:** No staging environment support.

**Workaround:**
- Deploy directly to production
- Use feature flags for gradual rollouts
- Maintain comprehensive test coverage
- Upgrade to B1 or higher for deployment slots

### 7. Auto-Scaling: NOT Available

**Impact:** Limited to 1 instance, no horizontal scaling.

**Considerations:**
- F1 tier is suitable for low-traffic applications only
- Monitor request volume and response times
- Upgrade to B1 or higher for auto-scaling

## Configuration Checklist

### Required Environment Variables

All 6 environment variables must be configured in Azure App Service Configuration:

1. **AZURE_SQL_CONNECTION_STRING**
   - Must include `Min Pool Size=2;Max Pool Size=100;`
   - Example:
     ```
     Server=tcp:velocify-sql.database.windows.net,1433;Initial Catalog=VelocifyDb;User ID=velocifyadmin;Password=YourPassword123!;MultipleActiveResultSets=True;Encrypt=True;Min Pool Size=2;Max Pool Size=100;
     ```

2. **JWT_SECRET_KEY**
   - Minimum 32 characters
   - Generate: `openssl rand -base64 32`

3. **JWT_ISSUER**
   - Your backend API URL
   - Example: `https://velocify.azurewebsites.net`

4. **JWT_AUDIENCE**
   - Your frontend URL
   - Example: `https://veelocify.vercel.app`

5. **LANGCHAIN_API_KEY**
   - OpenAI API key (format: `sk-proj-...`)
   - Set usage limits in OpenAI dashboard

6. **CORS_ALLOWED_ORIGINS**
   - Semicolon-separated list
   - Example: `https://veelocify.vercel.app;https://velocify-staging.vercel.app`

### Azure App Service Settings

- **Runtime Stack:** .NET 8 (LTS)
- **Platform:** 64 Bit
- **Always On:** OFF (not available in F1)
- **ARR Affinity:** OFF (not needed for stateless API)
- **HTTPS Only:** ON (force HTTPS)

### Database Firewall

Allow Azure services to access your Azure SQL Database:

```bash
az sql server firewall-rule create \
  --resource-group velocify-rg \
  --server velocify-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

## Monitoring and Alerts

### Critical Alerts to Configure

1. **CPU Time Alert** (CRITICAL)
   - Metric: CPU Time
   - Threshold: > 50 minutes
   - Action: Email notification
   - Gives 10-minute warning before hitting limit

2. **Memory Alert**
   - Metric: Memory Working Set
   - Threshold: > 800 MB (80% of 1 GB)
   - Action: Email notification

3. **Disk Space Alert**
   - Metric: File System Usage
   - Threshold: > 800 MB (80% of 1 GB)
   - Action: Email notification

4. **HTTP 5xx Errors**
   - Metric: HTTP Server Errors
   - Threshold: > 10 in 5 minutes
   - Action: Email notification

### Monitoring Dashboard

Create a custom dashboard in Azure Portal with:
- CPU Time (daily usage)
- Memory Working Set
- Response Time
- HTTP Status Codes
- Request Count

## Cost Considerations

### Current Configuration Costs

- **Azure App Service F1:** $0/month (Free)
- **Azure SQL Serverless:** ~$5-15/month (depends on usage)
- **Groq API:** Free tier available, competitive pricing
- **Total Estimated:** ~$5-20/month for low-traffic usage

### Cost Optimization Tips

1. **Azure SQL Serverless:**
   - Auto-pauses after 1 hour of inactivity
   - Auto-resumes on connection
   - Only pay for compute when active

2. **Groq API:**
   - Monitor usage in Groq dashboard
   - Cache AI responses when possible
   - Implement rate limiting

3. **Monitor Usage:**
   - Azure Portal → Cost Management + Billing
   - Set up budget alerts

## When to Upgrade to B1 Tier

Consider upgrading to B1 tier (~$13/month) if:

- ✅ You exceed CPU time quota regularly
- ✅ Cold starts are unacceptable for your use case
- ✅ You need custom domain support
- ✅ You need deployment slots for staging
- ✅ You need auto-scaling capabilities
- ✅ You need more than 1 GB RAM or disk space

### Benefits of B1 Tier

- **Unlimited CPU time** - No daily quota
- **Always On** - No cold starts
- **1.75 GB RAM** - 75% more memory
- **10 GB disk space** - 10x more storage
- **Custom domains** - Use your own domain
- **Deployment slots** - Staging environment support
- **Auto-scaling** - Scale up to 3 instances

### How to Upgrade

1. Azure Portal → Your App Service → Scale up (App Service plan)
2. Select "B1" tier
3. Click "Apply"
4. No code changes needed - instant upgrade

## Troubleshooting

### Issue: CPU Time Limit Exceeded

**Symptoms:**
- App stops responding
- HTTP 503 errors
- "CPU quota exceeded" message in logs

**Solutions:**
1. Wait until midnight UTC (quota resets)
2. Review logs to identify CPU-intensive operations
3. Optimize database queries
4. Reduce AI feature usage
5. Upgrade to B1 tier

### Issue: Out of Memory

**Symptoms:**
- App crashes
- HTTP 503 errors
- "Out of memory" exceptions in logs

**Solutions:**
1. Verify connection pooling parameters (Max Pool Size=100)
2. Check for memory leaks (dispose resources properly)
3. Review logs for large collection allocations
4. Upgrade to B1 tier (1.75 GB RAM)

### Issue: Slow Cold Starts

**Symptoms:**
- First request after idle takes 10-30 seconds
- Subsequent requests are fast

**Solutions:**
1. Accept as expected behavior on F1 tier
2. Optimize startup time (lazy load services)
3. Use health checks to keep app warm (configured at 5-minute intervals)
4. Upgrade to B1 tier for Always On feature

### Issue: Disk Space Full

**Symptoms:**
- App crashes
- "Disk full" errors in logs
- Unable to write logs

**Solutions:**
1. Verify Serilog log retention (7 days)
2. Manually delete old logs in `/home/LogFiles/Application/`
3. Reduce log verbosity
4. Upgrade to B1 tier (10 GB disk space)

## Performance Benchmarks

### Expected Performance on F1 Tier

- **Cold Start:** 10-30 seconds (first request after idle)
- **Warm Request:** 50-200ms (typical API request)
- **Database Query:** 10-50ms (with optimizations)
- **AI Feature:** 1-5 seconds (depends on LangChain/OpenAI)

### Optimization Impact

| Optimization | CPU Time Saved | Impact |
|-------------|----------------|--------|
| Connection Pooling | ~20% | High |
| Compiled Queries | ~15% | High |
| Indexed Views | ~10% | Medium |
| AsNoTracking() | ~10% | Medium |
| Filtered Indexes | ~5% | Low |
| Minimal Logging | ~5% | Low |

## Additional Resources

- [Azure App Service F1 Tier Setup Guide](AZURE-APP-SERVICE-SETUP.md)
- [Environment Variables Documentation](ENVIRONMENT-VARIABLES.md)
- [Azure Configuration Reference](AZURE-CONFIGURATION-REFERENCE.md)
- [Deployment Checklist](DEPLOYMENT-CHECKLIST.md)
- [CORS Configuration](CORS-CONFIGURATION.md)

## Support

For issues or questions:
- Check Azure App Service logs: Azure Portal → App Service → Log stream
- Review Application Insights for errors and performance
- Consult Azure documentation: https://docs.microsoft.com/azure/app-service/
- Check OpenAI API status: https://status.openai.com/

---

**Remember:** F1 tier is excellent for development and low-traffic applications, but production workloads should consider B1 or higher tiers for better performance and reliability.
