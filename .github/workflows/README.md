# CI/CD Workflows

This directory contains GitHub Actions workflows for automated building, testing, code quality analysis, and deployment of the Velocify platform.

## Workflows

### Backend CI/CD (`backend-ci.yml`)

**Triggers:**
- Push to `develop` or `main` branches (backend files only)
- Pull requests to `develop` or `main` branches (backend files only)

**Jobs:**

1. **build-and-test**
   - Restores NuGet packages
   - Runs SonarQube analysis
   - Builds the solution
   - Runs unit tests with code coverage
   - Uploads test results and coverage reports

2. **deploy-to-azure** (only on `main` branch push)
   - Builds and publishes the API
   - Deploys to Azure App Service
   - Runs EF Core database migrations
   - Requires production environment approval

### Frontend CI/CD (`frontend-ci.yml`)

**Triggers:**
- Push to `develop` or `main` branches (frontend files only)
- Pull requests to `develop` or `main` branches (frontend files only)

**Jobs:**

1. **build-and-test**
   - Installs npm dependencies
   - Runs ESLint
   - Runs TypeScript type checking
   - Runs tests with coverage
   - Performs SonarQube analysis
   - Builds the application
   - Uploads build artifacts and coverage reports

2. **deploy-to-vercel** (only on `main` branch push)
   - Builds production bundle
   - Deploys to Vercel
   - Requires production environment approval

## Required Secrets

Configure the following secrets in your GitHub repository settings (`Settings > Secrets and variables > Actions`):

### Backend Secrets

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `SONAR_TOKEN` | SonarCloud authentication token | `sqp_xxxxxxxxxxxxx` |
| `AZURE_CREDENTIALS` | Azure service principal credentials (JSON) | `{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}` |
| `AZURE_APP_SERVICE_NAME` | Name of the Azure App Service | `velocify-api` |
| `AZURE_SQL_CONNECTION_STRING` | Connection string for Azure SQL Database | `Server=tcp:velocify.database.windows.net,1433;Initial Catalog=velocify;...` |

### Frontend Secrets

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `SONAR_TOKEN` | SonarCloud authentication token (same as backend) | `sqp_xxxxxxxxxxxxx` |
| `VITE_API_BASE_URL` | Production API base URL | `https://velocify-api.azurewebsites.net` |
| `VITE_SIGNALR_HUB_URL` | Production SignalR hub URL | `https://velocify-api.azurewebsites.net/hubs/tasks` |
| `VERCEL_TOKEN` | Vercel deployment token | `xxxxxxxxxxxxx` |
| `VERCEL_ORG_ID` | Vercel organization ID | `team_xxxxxxxxxxxxx` |
| `VERCEL_PROJECT_ID` | Vercel project ID | `prj_xxxxxxxxxxxxx` |

## SonarQube Configuration

### Setup Instructions

1. **Create SonarCloud Account**
   - Go to https://sonarcloud.io
   - Sign in with GitHub
   - Create an organization

2. **Create Projects**
   - Create two projects: `velocify-backend` and `velocify-frontend`
   - Note the project keys and organization name

3. **Generate Token**
   - Go to `My Account > Security > Generate Tokens`
   - Create a token with analysis permissions
   - Add the token to GitHub secrets as `SONAR_TOKEN`

4. **Update Workflow Files**
   - Replace `your-org` with your SonarCloud organization name in both workflow files
   - Update project keys if you used different names

### Quality Gates

SonarQube will analyze:
- Code quality and maintainability
- Security vulnerabilities
- Code coverage
- Code duplication
- Technical debt

The analysis runs automatically on every push and pull request to `develop` or `main` branches.

## Azure Deployment Setup

### Prerequisites

1. **Azure App Service**
   - Create an App Service (F1 Free tier or higher)
   - Note the app name

2. **Azure SQL Database**
   - Create an Azure SQL Database (Serverless tier recommended)
   - Note the connection string

3. **Service Principal**
   - Create a service principal with contributor access to the App Service
   - Generate credentials JSON:
   ```bash
   az ad sp create-for-rbac --name "velocify-github-actions" \
     --role contributor \
     --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
     --sdk-auth
   ```
   - Add the output JSON to GitHub secrets as `AZURE_CREDENTIALS`

### Database Migrations

Database migrations run automatically after deployment using the `dotnet ef database update` command. Ensure:
- The connection string has sufficient permissions
- The database firewall allows GitHub Actions IP addresses (or enable "Allow Azure services")

## Vercel Deployment Setup

### Prerequisites

1. **Vercel Account**
   - Sign up at https://vercel.com
   - Connect your GitHub repository

2. **Create Project**
   - Import the repository
   - Set root directory to `frontend`
   - Framework preset: Vite

3. **Get Credentials**
   - Vercel Token: `Settings > Tokens > Create Token`
   - Organization ID: Found in team settings URL
   - Project ID: Found in project settings

4. **Environment Variables**
   - Set `VITE_API_BASE_URL` and `VITE_SIGNALR_HUB_URL` in Vercel project settings
   - Also add them to GitHub secrets for the workflow

## Local Testing

### Test Backend Workflow Locally

```bash
# Install dependencies
cd backend
dotnet restore

# Run tests
dotnet test --configuration Release

# Build
dotnet build --configuration Release
```

### Test Frontend Workflow Locally

```bash
# Install dependencies
cd frontend
npm ci

# Run linter
npm run lint

# Run type check
npx tsc --noEmit

# Run tests
npm run test

# Build
npm run build
```

## Troubleshooting

### SonarQube Analysis Fails

- Verify `SONAR_TOKEN` is valid and has analysis permissions
- Check that project keys match in SonarCloud and workflow files
- Ensure organization name is correct

### Azure Deployment Fails

- Verify `AZURE_CREDENTIALS` JSON is valid
- Check that the service principal has contributor role
- Ensure App Service name is correct
- Verify database connection string is valid

### Vercel Deployment Fails

- Verify all Vercel secrets are correct
- Check that the project is properly linked
- Ensure environment variables are set in Vercel dashboard

### Database Migration Fails

- Check connection string permissions
- Verify firewall rules allow GitHub Actions
- Ensure migrations are in the correct project
- Check for migration conflicts

## Monitoring

- **GitHub Actions**: View workflow runs in the Actions tab
- **SonarCloud**: View code quality reports at https://sonarcloud.io
- **Azure**: Monitor App Service in Azure Portal
- **Vercel**: View deployments in Vercel dashboard
