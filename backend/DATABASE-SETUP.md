# Database Setup Guide

## Overview

This project uses **SQL Server** in all environments, with different configurations for each:

| Environment | Database Type | Purpose |
|------------|---------------|---------|
| **Production** | Azure SQL Server | Real application data |
| **Development** | SQL Server LocalDB (Windows) or SQL Server (Linux/Mac) | Local development |
| **CI/CD Tests** | EF Core InMemory | Fast, isolated test execution |

## Production Setup (Azure SQL Server)

### Your Current Configuration

```
Server: velocify-server-db.database.windows.net
Database: velocify-free-sql-db-0695809
Port: 1433
User: CloudSAf85a98d4
```

### Connection String Format

```
Server=tcp:velocify-server-db.database.windows.net,1433;
Initial Catalog=velocify-free-sql-db-0695809;
Persist Security Info=False;
User ID=CloudSAf85a98d4;
Password=YOUR_PASSWORD;
MultipleActiveResultSets=True;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
Min Pool Size=2;
Max Pool Size=100;
```

### Required GitHub Secrets

For the CI/CD pipeline to work, you need to configure these secrets in your GitHub repository:

1. **AZURE_SQL_CONNECTION_STRING**
   ```
   Server=tcp:velocify-server-db.database.windows.net,1433;Initial Catalog=velocify-free-sql-db-0695809;Persist Security Info=False;User ID=CloudSAf85a98d4;Password=Iammohid@123;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

2. **AZURE_APP_SERVICE_NAME**
   ```
   velocify
   ```
   (or whatever your App Service name is)

3. **AZURE_CREDENTIALS**
   ```json
   {
     "clientId": "your-client-id",
     "clientSecret": "your-client-secret",
     "subscriptionId": "your-subscription-id",
     "tenantId": "your-tenant-id"
   }
   ```

### How to Add GitHub Secrets

1. Go to your GitHub repository
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret with the exact name shown above

### Azure App Service Configuration

Your connection string should already be configured in Azure App Service:

1. Go to Azure Portal → Your App Service
2. Navigate to **Configuration** → **Connection strings**
3. Add/verify connection string named `DefaultConnection`:
   - **Name**: `DefaultConnection`
   - **Value**: Your full connection string
   - **Type**: `SQLAzure`

## Development Setup

### Windows (SQL Server LocalDB)

LocalDB is automatically installed with Visual Studio. Connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VelocifyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Linux/Mac (SQL Server in Docker)

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sql-server \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=VelocifyDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

## CI/CD Test Setup

**Tests use EF Core InMemory database** - this is intentional and correct:

- ✅ Fast test execution
- ✅ No external database needed in CI
- ✅ Isolated test data
- ✅ Works on Linux CI runners

**Important**: The InMemory database is ONLY used for tests. Your production app uses Azure SQL Server.

## Database Migrations

### Create a New Migration

```bash
cd backend
dotnet ef migrations add MigrationName --project Velocify.Infrastructure --startup-project Velocify.API
```

### Apply Migrations Locally

```bash
cd backend
dotnet ef database update --project Velocify.Infrastructure --startup-project Velocify.API
```

### Apply Migrations to Azure (Automatic)

Migrations are automatically applied during deployment via the GitHub Actions workflow:

```yaml
- name: Run database migrations
  run: |
    dotnet tool install --global dotnet-ef
    dotnet ef database update --project backend/Velocify.Infrastructure/Velocify.Infrastructure.csproj \
      --startup-project backend/Velocify.API/Velocify.API.csproj \
      --connection "${{ secrets.AZURE_SQL_CONNECTION_STRING }}"
```

## Troubleshooting

### "Cannot connect to Azure SQL Server"

1. Check firewall rules in Azure Portal
2. Add your IP address to allowed IPs
3. Ensure "Allow Azure services" is enabled

### "Login failed for user"

1. Verify username and password in connection string
2. Check if user has proper permissions
3. Try connecting with Azure Data Studio to test credentials

### "Tests fail with database errors"

Tests should use InMemory database. If you see SQL Server errors in tests:
1. Verify tests use `TestWebApplicationFactory`
2. Check test class has `IClassFixture<TestWebApplicationFactory>`

### "Migrations fail in CI/CD"

1. Verify `AZURE_SQL_CONNECTION_STRING` secret is set correctly
2. Check Azure SQL firewall allows GitHub Actions IPs
3. Review migration logs in GitHub Actions output

## Security Best Practices

1. **Never commit connection strings** to source control
2. **Use Azure Key Vault** for production secrets (recommended upgrade)
3. **Rotate passwords** regularly
4. **Use Managed Identity** when possible (requires Azure configuration)
5. **Enable SSL/TLS** (already configured with `Encrypt=True`)

## Cost Optimization (Azure SQL Free Tier)

Your current setup uses Azure SQL Free tier:
- 32 GB storage
- 5 DTUs (Database Transaction Units)
- Good for development/testing
- Consider upgrading for production load

Monitor usage in Azure Portal to avoid unexpected charges.

## Next Steps

1. ✅ Add GitHub secrets (AZURE_SQL_CONNECTION_STRING, AZURE_APP_SERVICE_NAME, AZURE_CREDENTIALS)
2. ✅ Verify Azure App Service has connection string configured
3. ✅ Push to GitHub to trigger CI/CD pipeline
4. ✅ Monitor deployment in GitHub Actions
5. ✅ Verify migrations applied successfully in Azure Portal

## Support

If you encounter issues:
1. Check GitHub Actions logs for detailed error messages
2. Review Azure App Service logs in Azure Portal
3. Test connection string locally with Azure Data Studio
4. Verify all secrets are configured correctly
