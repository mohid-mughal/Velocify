# Groq API Configuration for Velocify

## ✅ Changes Made

The Velocify backend has been configured to use **Groq API** instead of OpenAI for all AI features.

### Modified Files:
1. `backend/Velocify.API/appsettings.json` - Updated LangChain configuration
2. `backend/ENVIRONMENT-VARIABLES.md` - Updated API key documentation
3. `backend/DEPLOYMENT-CHECKLIST.md` - Updated deployment instructions
4. `backend/AZURE-APP-SERVICE-SETUP.md` - Updated setup guide
5. `backend/AZURE-CONFIGURATION-REFERENCE.md` - Updated configuration reference
6. `backend/AZURE-F1-TIER-BEST-PRACTICES.md` - Updated cost estimates

### Configuration Changes:
```json
"LangChain": {
  "ApiKey": "${LANGCHAIN_API_KEY}",
  "Provider": "Groq",
  "BaseUrl": "https://api.groq.com/openai/v1",
  "Model": "openai/gpt-oss-120b",
  "MaxTokens": 2000,
  "Temperature": 0.7
}
```

---

## 🔑 Your Environment Variables for Azure

Add these to **Azure Portal → App Service → Configuration → Application settings**:

### 1. AZURE_SQL_CONNECTION_STRING
```
Server=tcp:velocify-server-db.database.windows.net,1433;Initial Catalog=velocify-free-sql-db-0695809;Persist Security Info=False;User ID=CloudSAf85a98d4;Password=Iammohid@123;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Min Pool Size=2;Max Pool Size=100;
```

### 2. JWT_SECRET_KEY
```
7xK9mP2qR5sT8wV1yZ4aB6cD9eF2gH5iJ8kL1mN4oP7qR0sT3uV6wX9yZ2aB5cD8e
```

### 3. JWT_ISSUER
```
https://velocify.azurewebsites.net
```
*(Replace with your actual Azure App Service URL)*

### 4. JWT_AUDIENCE
```
https://velocify-work.vercel.app
```
*(Replace with your actual Vercel frontend URL)*

### 5. LANGCHAIN_API_KEY
```
gsk_YOUR_GROQ_API_KEY_HERE
```
**Note:** Use your actual Groq API key from https://console.groq.com/keys

### 6. CORS_ALLOWED_ORIGINS
```
https://velocify-work.vercel.app
```
*(Replace with your actual Vercel frontend URL)*

---

## 🚀 Groq API Benefits

### Why Groq?
- **Fast inference** - Groq's LPU (Language Processing Unit) provides extremely fast response times
- **Cost-effective** - Competitive pricing compared to OpenAI
- **OpenAI-compatible API** - Drop-in replacement, minimal code changes needed
- **Free tier available** - Great for development and testing

### Model: openai/gpt-oss-120b
- Large open-source model with 120 billion parameters
- Excellent performance for task management AI features
- Fast inference on Groq's infrastructure

---

## 📝 How to Add Variables to Azure

1. **Azure Portal** → Your App Service → **Configuration** → **Application settings**
2. For each variable above:
   - Click **"New application setting"**
   - Enter the **Name** (e.g., `LANGCHAIN_API_KEY`)
   - Enter the **Value** (your actual Groq API key starting with `gsk_`)
   - Click **OK**
3. After adding all 6 variables, click **Save** → **Continue** (to restart the app)

---

## ✅ Verification Steps

After deployment:

1. **Check health endpoint:**
   ```bash
   curl https://velocify.azurewebsites.net/health
   ```

2. **Test AI features:**
   - Natural language task parsing
   - Task decomposition
   - Semantic search
   - Daily digest generation

3. **Monitor Groq usage:**
   - Visit https://console.groq.com/
   - Check API usage and rate limits

---

## 🔒 Security Notes

**⚠️ IMPORTANT:** Your Groq API key is now visible in this document and our conversation.

**Recommended actions:**
1. ✅ Add to Azure App Service Configuration (encrypted)
2. ✅ Add to GitHub Secrets for CI/CD (if using automated deployment)
3. ❌ Never commit API keys to source control
4. 🔄 Consider rotating the API key after initial setup

---

## 📊 Cost Comparison

| Provider | Pricing | Speed | Notes |
|----------|---------|-------|-------|
| **Groq** | Free tier + pay-as-you-go | ⚡ Very Fast | LPU-powered inference |
| OpenAI | Pay-as-you-go (~$0.002/request) | Fast | Industry standard |

**Estimated monthly cost with Groq:** ~$5-15 (mostly Azure SQL, Groq free tier covers most usage)

---

## 🎯 Next Steps

1. ✅ Code changes pushed to GitHub
2. ⏳ Add all 6 environment variables to Azure App Service
3. ⏳ Deploy backend to Azure
4. ⏳ Test AI features
5. ⏳ Deploy frontend to Vercel

---

## 📚 Additional Resources

- **Groq Console:** https://console.groq.com/
- **Groq Documentation:** https://console.groq.com/docs
- **Groq API Keys:** https://console.groq.com/keys
- **Model Information:** https://console.groq.com/docs/models

---

**All changes have been committed and pushed to GitHub!** 🚀

Your backend is now configured to use Groq API with the `openai/gpt-oss-120b` model.
