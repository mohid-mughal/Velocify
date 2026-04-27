@echo off
REM Deploy Critical API Fixes to Azure
REM This script deploys the fixes for:
REM - Delete Task (FK constraint violation)
REM - Update User Profile (duplicate email)
REM - Dashboard endpoint routes
REM - Notification endpoint routes

echo ==========================================
echo Deploying Critical API Fixes to Azure
echo ==========================================
echo.

REM Step 1: Build the solution
echo Step 1: Building solution...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo X Build failed. Please fix errors and try again.
    exit /b 1
)
echo √ Build successful
echo.

REM Step 2: Run tests
echo Step 2: Running tests...
dotnet test --no-build -c Release
if %errorlevel% neq 0 (
    echo ! Some tests failed. Do you want to continue? (y/n)
    set /p response=
    if /i not "%response%"=="y" (
        echo Deployment cancelled.
        exit /b 1
    )
)
echo √ Tests completed
echo.

REM Step 3: Publish the API
echo Step 3: Publishing API...
cd Velocify.API
dotnet publish -c Release -o ./publish
if %errorlevel% neq 0 (
    echo X Publish failed. Please fix errors and try again.
    exit /b 1
)
echo √ Publish successful
echo.

REM Step 4: Deploy to Azure (using Azure CLI)
echo Step 4: Deploying to Azure App Service...
echo Please ensure you have Azure CLI installed and are logged in.
echo.
echo Run the following command to deploy:
echo az webapp deploy --resource-group ^<your-resource-group^> --name velocify --src-path ./publish --type zip
echo.
echo Or use GitHub Actions by pushing to main branch:
echo git add .
echo git commit -m "Fix: Critical API issues - delete task, duplicate email, endpoint routes"
echo git push origin main
echo.

echo ==========================================
echo Deployment preparation complete!
echo ==========================================
echo.
echo Files modified:
echo   √ DeleteTaskCommandHandler.cs
echo   √ TaskRepository.cs
echo   √ ITaskRepository.cs
echo   √ UpdateCurrentUserCommandHandler.cs
echo   √ GlobalExceptionHandler.cs
echo   √ test-azure-deployment.http
echo.
echo After deployment, test using:
echo   - backend/test-azure-deployment.http
echo   - Check Azure Application Insights for errors
echo.

pause
