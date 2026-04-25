# Velocify Backend Deployment Checklist

## Required Environment Variables

Before deploying to Azure App Service F1 tier, you need to provide the following environment variables:

### 1. Database Connection String
- **Variable Name:** `AZURE_SQL_CONNECTION_STRING`
- **What it is:** Connection string for your Azure SQL Database
- **Where to get it:** Azure Portal → SQL Database → Connection strings
- **Format:** `Server=tcp:{server}.database.windows.net,1433;Initial Catalog={database};User ID={username};Password={password};MultipleActiveResultSets=True;Encrypt=True;Min Pool Size=2;Max Pool Size=100;`

### 2. JWT Secret Key
- **Variable Name:** `JWT_SECRET_KEY`
- **What it is:** Secret key for signing authentication tokens
- **Where to get it:** Generate a secure random string (minimum 32 characters)
- **How to generate:** Run `openssl rand -base64 32` in terminal
- **Example:** `your-production-secret-key-at-least-32-characters-long-change-this`

### 3. JWT Issuer
- **Variable Name:** `JWT_ISSUER`
- **What it is:** Your backend API URL
- **Example:** `https://velocify-api.azurewebsites.net`
- **Note:** Use your actual Azure App Service URL

### 4. JWT Audience
- **Variable Name:** `JWT_AUDIENCE`
- **What it is:** Your frontend application URL
- **Example:** `https://velocify.vercel.app`
- **Note:** Use your actual frontend deployment URL

### 5. LangChain/Groq API Key
- **Variable Name:** `LANGCHAIN_API_KEY`
- **What it is:** API key for AI features (Groq)
- **Where to get it:** https://console.groq.com/keys
- **Format:** `gsk_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`
- **Important:** Monitor usage in Groq dashboard

### 6. CORS Allowed Origins
- **Variable Name:** `CORS_ALLOWED_ORIGINS`
- **What it is:** Semicolon-separated list of allowed frontend URLs
- **Example:** `https://velocify.vercel.app;https://velocify-staging.vercel.app`
- **Note:** Include all frontend deployment URLs (production, staging, etc.)

## Configuration Files Created

✅ **appsettings.json** - Updated with environment variable placeholders  
✅ **sonar-project.properties** - SonarQube configuration with exclusions for:
  - Migrations folder (`**/Migrations/**`)
  - Auto-generated Designer files (`**/*.Designer.cs`)
  - Model snapshot (`VelocifyDbContextModelSnapshot.cs`)
  - Build artifacts (`**/bin/**`, `**/obj/**`)
  - Assembly info files

✅ **ENVIRONMENT-VARIABLES.md** - Comprehensive documentation of all environment variables  
✅ **DEPLOYMENT-CHECKLIST.md** - This file  
✅ **AZURE-APP-SERVICE-SETUP.md** - Detailed step-by-step Azure deployment guide  
✅ **AZURE-CONFIGURATION-REFERENCE.md** - Quick reference for Azure App Service settings  
✅ **.deployment** - Azure deployment configuration file  
✅ **web.config** - IIS/Azure App Service configuration with CPU time management comments

## Next Steps

1. **Create Azure SQL Database**
   - Use Serverless tier for cost optimization
   - Configure firewall to allow Azure services
   - Note the connection string
   - **See [AZURE-APP-SERVICE-SETUP.md](AZURE-APP-SERVICE-SETUP.md) Step 4 for detailed instructions**

2. **Generate JWT Secret Key**
   ```bash
   openssl rand -base64 32
   ```

3. **Get Groq API Key**
   - Sign up at https://console.groq.com/
   - Create API key
   - Monitor usage in dashboard

4. **Configure Azure App Service**
   - Navigate to Azure Portal
   - Go to your App Service → Configuration
   - Add all environment variables listed above
   - **CRITICAL:** Ensure connection string includes `Min Pool Size=2;Max Pool Size=100;`
   - Save and restart
   - **See [AZURE-CONFIGURATION-REFERENCE.md](AZURE-CONFIGURATION-REFERENCE.md) for quick reference**
   - **See [AZURE-APP-SERVICE-SETUP.md](AZURE-APP-SERVICE-SETUP.md) for step-by-step guide**

5. **Deploy Backend**
   - Push code to GitHub
   - GitHub Actions will automatically deploy to Azure App Service
   - Monitor deployment in Actions tab
   - **See [AZURE-APP-SERVICE-SETUP.md](AZURE-APP-SERVICE-SETUP.md) Step 5 for deployment instructions**

6. **Verify Deployment**
   - Check health endpoint: `https://your-app.azurewebsites.net/health`
   - Test authentication: `POST /api/v1/auth/register`
   - Monitor logs in Azure Portal
   - **Monitor CPU time usage** (critical for F1 tier)
   - **See [AZURE-APP-SERVICE-SETUP.md](AZURE-APP-SERVICE-SETUP.md) Step 6 for verification steps**

## Azure F1 Tier Limitations

⚠️ **Important:** Azure App Service F1 (Free) tier has limitations:
- **60 minutes CPU time per day** - Monitor usage in Azure Portal
- **1 GB RAM** - Connection pooling configured to optimize memory
- **1 GB disk space** - Logs auto-cleanup after 7 days
- **No "Always On"** - First request after idle will be slow (cold start)
- **No custom domains** - Use `*.azurewebsites.net` domain

If you exceed these limits, consider upgrading to B1 tier (~$13/month).

**For detailed Azure App Service setup instructions, see [AZURE-APP-SERVICE-SETUP.md](AZURE-APP-SERVICE-SETUP.md)**

## Cost Estimates

- **Azure App Service F1:** Free
- **Azure SQL Serverless:** ~$5-15/month (depends on usage)
- **OpenAI API:** ~$0.002 per AI request (varies by model)
- **Total estimated:** ~$5-20/month for low-traffic usage

## Security Reminders

- ✅ Never commit secrets to source control
- ✅ Use strong passwords (12+ characters, mixed case, numbers, symbols)
- ✅ Rotate JWT secret key periodically (quarterly recommended)
- ✅ Set OpenAI usage limits to prevent unexpected costs
- ✅ Configure Azure SQL firewall rules properly
- ✅ Monitor failed authentication attempts

## Support

For detailed information, see:
- **ENVIRONMENT-VARIABLES.md** - Complete environment variable documentation
- **Azure App Service docs:** https://docs.microsoft.com/azure/app-service/
- **OpenAI API docs:** https://platform.openai.com/docs/

## Questions?

If you need help with any of these steps, please provide:
1. Which environment variable you need help with
2. What error message you're seeing (if any)
3. What you've tried so far

I'm here to help! 🚀
