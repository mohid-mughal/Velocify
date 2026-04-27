@echo off
REM Deploy All Backend Fixes to Azure
REM This script stages and commits all backend code fixes

echo.
echo 🚀 Deploying all backend fixes to Azure...
echo.

REM Stage all backend code changes
echo 📦 Staging files...
git add backend/Velocify.API/Controllers/HealthController.cs
git add backend/Velocify.API/Program.cs
git add backend/Velocify.API/Controllers/TasksController.cs

REM Show what will be committed
echo.
echo 📋 Files to be committed:
git status --short

REM Commit with comprehensive message
echo.
echo 💾 Creating commit...
git commit -m "Fix: Multiple backend issues" -m "" -m "- Health check: Support both LangChain and OpenAI API keys" -m "- Enum serialization: Add JsonStringEnumConverter for string enum support" -m "- User ID tracking: Set user IDs from JWT token in all endpoints" -m "  - UpdateTask: Set UpdatedByUserId" -m "  - UpdateTaskStatus: Set UpdatedByUserId" -m "  - DeleteTask: Set DeletedByUserId" -m "  - CreateComment: Set UserId" -m "  - DeleteComment: Set UserId" -m "" -m "These fixes resolve:" -m "- Foreign key constraint violations in audit logs" -m "- Enum deserialization errors (now supports both string and numeric values)" -m "- Health check failures for Groq API configuration" -m "" -m "Closes: Task creation, update, delete, and comment operations now work correctly"

REM Push to trigger GitHub Actions deployment
echo.
echo 🌐 Pushing to GitHub...
git push origin main

echo.
echo ✅ Done! GitHub Actions will deploy to Azure in ~5-10 minutes.
echo.
echo 📊 Monitor deployment:
echo    https://github.com/YOUR-USERNAME/YOUR-REPO/actions
echo.
echo 🔍 After deployment, verify:
echo    1. Health check: curl https://velocify.azurewebsites.net/health
echo    2. Create task with string enums
echo    3. Update task status
echo    4. Create comment
echo    5. Check audit logs
echo.
pause
