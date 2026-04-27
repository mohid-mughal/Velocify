# Deployment Checklist

Before pushing to GitHub, ensure these secrets are configured:

## Required GitHub Secrets

Go to: **GitHub Repository → Settings → Secrets and variables → Actions**

### 1. AZURE_SQL_CONNECTION_STRING
```
Server=tcp:YOUR-SERVER.database.windows.net,1433;Initial Catalog=YOUR-DATABASE;Persist Security Info=False;User ID=YOUR-USERNAME;Password=YOUR-PASSWORD;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```
**Replace with your actual Azure SQL connection string**

### 2. AZURE_APP_SERVICE_NAME
```
your-app-service-name
```
**Replace with your actual Azure App Service name**

### 3. AZURE_CREDENTIALS
Get this from Azure CLI:
```bash
az ad sp create-for-rbac --name "velocify-github-actions" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
  --sdk-auth
```

The output will be JSON - copy the entire JSON object as the secret value.

### 4. SONAR_TOKEN (Optional)
Only needed if you want SonarQube code analysis. Can be added later.

## Azure App Service Configuration

Verify in Azure Portal → Your App Service → Configuration:

### Connection Strings
- **Name**: `DefaultConnection`
- **Value**: Your Azure SQL connection string
- **Type**: `SQLAzure`

### Application Settings
Add these if not already present:
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `Jwt__SecretKey`: (your JWT secret key)
- `Jwt__Issuer`: `https://velocify-api.azurewebsites.net`
- `Jwt__Audience`: `https://velocify-api.azurewebsites.net`
- `OpenAI__ApiKey`: (your OpenAI API key)
- `CorsSettings__AllowedOrigins`: (your frontend URL)

## Push to GitHub

Once secrets are configured:

```bash
git push origin main
```

## Monitor Deployment

1. Go to GitHub → Actions tab
2. Watch the "Backend CI/CD" workflow
3. Check both jobs:
   - ✅ Build, Test, and Analyze
   - ✅ Deploy to Azure App Service

## Verify Deployment

After successful deployment:

1. **Check App Service**: https://velocify-api.azurewebsites.net/health
2. **Check Database**: Verify migrations applied in Azure Portal
3. **Check Logs**: Azure Portal → App Service → Log stream

## Troubleshooting

### If tests fail:
- Check test output in GitHub Actions
- Most tests should pass now with InMemory database
- LocalDB tests are skipped (expected)

### If deployment fails:
- Verify all secrets are set correctly
- Check Azure credentials have proper permissions
- Review deployment logs in GitHub Actions

### If migrations fail:
- Check Azure SQL firewall rules
- Verify connection string is correct
- Ensure database user has proper permissions

## Next Steps After Successful Deployment

1. ✅ Test API endpoints
2. ✅ Verify database schema
3. ✅ Test authentication flow
4. ✅ Configure custom domain (optional)
5. ✅ Set up monitoring and alerts
6. ✅ Configure Application Insights (optional)

## Important Notes

- **First deployment** may take 5-10 minutes
- **Migrations** run automatically after deployment
- **Tests** use InMemory database (not Azure SQL)
- **Production** uses Azure SQL Server as configured
- **Free tier** has limitations - monitor usage

## Support

If you encounter issues:
1. Check GitHub Actions logs
2. Check Azure App Service logs
3. Review DATABASE-SETUP.md
4. Review CI-CD-FIXES.md
