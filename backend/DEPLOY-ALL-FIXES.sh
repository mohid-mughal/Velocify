#!/bin/bash

# Deploy All Backend Fixes to Azure
# This script stages and commits all backend code fixes

echo "🚀 Deploying all backend fixes to Azure..."
echo ""

# Stage all backend code changes
echo "📦 Staging files..."
git add backend/Velocify.API/Controllers/HealthController.cs
git add backend/Velocify.API/Program.cs
git add backend/Velocify.API/Controllers/TasksController.cs

# Show what will be committed
echo ""
echo "📋 Files to be committed:"
git status --short

# Commit with comprehensive message
echo ""
echo "💾 Creating commit..."
git commit -m "Fix: Multiple backend issues

- Health check: Support both LangChain and OpenAI API keys
- Enum serialization: Add JsonStringEnumConverter for string enum support
- User ID tracking: Set user IDs from JWT token in all endpoints
  - UpdateTask: Set UpdatedByUserId
  - UpdateTaskStatus: Set UpdatedByUserId
  - DeleteTask: Set DeletedByUserId
  - CreateComment: Set UserId
  - DeleteComment: Set UserId

These fixes resolve:
- Foreign key constraint violations in audit logs
- Enum deserialization errors (now supports both string and numeric values)
- Health check failures for Groq API configuration

Closes: Task creation, update, delete, and comment operations now work correctly"

# Push to trigger GitHub Actions deployment
echo ""
echo "🌐 Pushing to GitHub..."
git push origin main

echo ""
echo "✅ Done! GitHub Actions will deploy to Azure in ~5-10 minutes."
echo ""
echo "📊 Monitor deployment:"
echo "   https://github.com/YOUR-USERNAME/YOUR-REPO/actions"
echo ""
echo "🔍 After deployment, verify:"
echo "   1. Health check: curl https://velocify.azurewebsites.net/health"
echo "   2. Create task with string enums"
echo "   3. Update task status"
echo "   4. Create comment"
echo "   5. Check audit logs"
echo ""
