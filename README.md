# Velocify Platform

An AI-augmented task management platform built with ASP.NET Core 8, React 18, and Azure SQL Database. Features real-time collaboration via SignalR, LangChain-powered AI capabilities, and advanced database optimizations.

## Architecture

- **Backend**: ASP.NET Core 8 Web API with Clean Architecture (Domain, Application, Infrastructure, API layers)
- **Frontend**: React 18 + Vite + TypeScript with Zustand and TanStack Query
- **Database**: Azure SQL Database Serverless with indexed views and table partitioning
- **AI**: LangChain.NET for natural language processing, task decomposition, and semantic search
- **Real-time**: SignalR for live notifications and updates

## Features

- 🔐 JWT authentication with refresh token rotation
- 👥 Role-based access control (SuperAdmin, Admin, Member)
- ✅ Task management with filtering, search, and hierarchical subtasks
- 💬 Real-time comments with sentiment analysis
- 📊 Dashboard analytics with velocity tracking
- 🤖 AI-powered features:
  - Natural language task creation
  - Smart task decomposition
  - Daily digest generation
  - Workload balancing suggestions
  - Semantic search
  - CSV import normalization
- 🔔 Real-time notifications via SignalR
- 📈 Structured logging with Serilog
- 🏥 Health checks for monitoring
- 🚀 Optimized database queries with compiled queries and indexed views

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Azure SQL Database](https://azure.microsoft.com/services/sql-database/) or SQL Server 2019+
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for deployment)
- OpenAI API key (for LangChain features)

## Getting Started

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Update the connection string in `Velocify.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=VelocifyDB;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;"
     }
   }
   ```

4. Run database migrations:
   ```bash
   dotnet ef database update --project Velocify.Infrastructure/Velocify.Infrastructure.csproj --startup-project Velocify.API/Velocify.API.csproj
   ```

5. Configure JWT and LangChain settings in `appsettings.Development.json`:
   ```json
   {
     "JwtSettings": {
       "SecretKey": "your-secret-key-min-32-characters-long",
       "Issuer": "https://localhost:5000",
       "Audience": "https://localhost:5000"
     },
     "LangChain": {
       "ApiKey": "your-openai-api-key",
       "Model": "gpt-4",
       "MaxTokens": 2000
     }
   }
   ```

6. Run the backend:
   ```bash
   dotnet run --project Velocify.API/Velocify.API.csproj
   ```

7. Access Swagger UI at `https://localhost:5000/swagger`

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Create `.env.local` file:
   ```env
   VITE_API_BASE_URL=https://localhost:5000
   VITE_SIGNALR_HUB_URL=https://localhost:5000/hubs/task
   ```

4. Run the development server:
   ```bash
   npm run dev
   ```

5. Access the application at `http://localhost:5173`

## Running Tests

### Backend Tests

```bash
cd backend
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm run test
```

## Deployment

### Frontend Deployment (Vercel)

1. Install Vercel CLI:
   ```bash
   npm install -g vercel
   ```

2. Deploy:
   ```bash
   cd frontend
   vercel
   ```

3. Set environment variables in Vercel dashboard:
   - `VITE_API_BASE_URL`: Your backend API URL
   - `VITE_SIGNALR_HUB_URL`: Your SignalR hub URL

### Backend Deployment (Azure App Service)

1. Create Azure resources:
   ```bash
   # Create resource group
   az group create --name velocify-rg --location eastus

   # Create App Service plan
   az appservice plan create --name velocify-plan --resource-group velocify-rg --sku F1 --is-linux

   # Create Web App
   az webapp create --name velocify-api --resource-group velocify-rg --plan velocify-plan --runtime "DOTNETCORE:8.0"
   ```

2. Configure application settings (see `infrastructure/azure-app-service-config.md`)

3. Deploy using GitHub Actions (workflow already configured in `.github/workflows/azure-app-service.yml`)

### Database Setup (Azure SQL)

1. Run the bootstrap script:
   ```bash
   sqlcmd -S your-server.database.windows.net -U admin -P password -i infrastructure/azure-sql-setup.sql
   ```

2. Run EF Core migrations:
   ```bash
   cd backend
   dotnet ef database update --project Velocify.Infrastructure/Velocify.Infrastructure.csproj --startup-project Velocify.API/Velocify.API.csproj --connection "your-connection-string"
   ```

## Project Structure

```
velocify-platform/
├── backend/
│   ├── Velocify.API/              # API layer (controllers, middleware, SignalR hubs)
│   ├── Velocify.Application/      # Application layer (commands, queries, DTOs)
│   ├── Velocify.Domain/           # Domain layer (entities, enums, business logic)
│   ├── Velocify.Infrastructure/   # Infrastructure layer (DbContext, repositories, AI services)
│   └── Velocify.Tests/            # Unit and integration tests
├── frontend/
│   ├── src/
│   │   ├── api/                   # API client and axios configuration
│   │   ├── components/            # Reusable UI components
│   │   ├── features/              # Feature-specific components and logic
│   │   ├── hooks/                 # Custom React hooks
│   │   ├── pages/                 # Page components
│   │   ├── store/                 # Zustand stores
│   │   └── utils/                 # Utility functions
│   └── public/                    # Static assets
├── infrastructure/
│   ├── azure-sql-setup.sql        # Database bootstrap script
│   └── azure-app-service-config.md # Deployment configuration guide
└── .github/
    └── workflows/
        └── azure-app-service.yml  # CI/CD workflow
```

## API Documentation

Once the backend is running, access the Swagger UI at:
- Local: `https://localhost:5000/swagger`
- Production: `https://your-app.azurewebsites.net/swagger`

## Key Technologies

### Backend
- ASP.NET Core 8
- Entity Framework Core 8
- MediatR (CQRS pattern)
- FluentValidation
- AutoMapper
- SignalR
- Serilog
- LangChain.NET
- Polly (retry policies)
- BCrypt.Net (password hashing)
- xUnit, Moq, FluentAssertions (testing)

### Frontend
- React 18
- TypeScript
- Vite
- React Router v6
- Zustand (state management)
- TanStack Query (server state)
- React Hook Form + Zod (forms and validation)
- Recharts (data visualization)
- Tailwind CSS
- SignalR client

### Database
- Azure SQL Database Serverless
- Indexed views for dashboard queries
- Table partitioning for audit logs
- Compiled queries for performance
- Filtered indexes for soft deletes

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please open an issue on GitHub.
