# Deployment & Best Practices Guide

## 🚀 Deployment Options

### Option 1: Azure (Recommended)

#### Frontend (Next.js) → Azure App Service

```bash
# Install Azure CLI
# https://learn.microsoft.com/en-us/cli/azure/install-azure-cli

# Login to Azure
az login

# Create resource group
az group create --name cv-builder-rg --location eastus

# Create App Service Plan
az appservice plan create \
  --name cv-builder-plan \
  --resource-group cv-builder-rg \
  --sku B1 \
  --is-linux

# Create Web App for Node.js
az webapp create \
  --resource-group cv-builder-rg \
  --plan cv-builder-plan \
  --name cv-builder-frontend \
  --runtime "NODE|18-lts"

# Configure deployment
az webapp deployment source config-zip \
  --resource-group cv-builder-rg \
  --name cv-builder-frontend \
  --src build.zip
```

#### Backend (.NET) → Azure App Service

```bash
# Create Web App for .NET
az webapp create \
  --resource-group cv-builder-rg \
  --plan cv-builder-plan \
  --name cv-builder-api \
  --runtime "DOTNETCORE|6.0"

# Set connection strings and environment variables
az webapp config appsettings set \
  --resource-group cv-builder-rg \
  --name cv-builder-api \
  --settings API_KEY=your-api-key CORS_ORIGIN=https://cv-builder-frontend.azurewebsites.net
```

### Option 2: Docker & Kubernetes

#### Build Docker Images

```bash
# Frontend
cd frontend
docker build -t cv-builder-frontend:latest .

# Backend
cd backend
docker build -t cv-builder-api:latest .

# Push to Docker Hub or Azure Container Registry
docker tag cv-builder-frontend:latest yourrepo/cv-builder-frontend:latest
docker push yourrepo/cv-builder-frontend:latest

docker tag cv-builder-api:latest yourrepo/cv-builder-api:latest
docker push yourrepo/cv-builder-api:latest
```

#### Kubernetes Deployment

```bash
kubectl apply -f k8s/frontend-deployment.yaml
kubectl apply -f k8s/backend-deployment.yaml
kubectl apply -f k8s/services.yaml
```

### Option 3: Vercel + Render

#### Frontend (Vercel)

```bash
# Install Vercel CLI
npm install -g vercel

# Deploy
cd frontend
vercel
```

#### Backend (Render)

1. Create account at https://render.com
2. Connect GitHub repository
3. Create new Web Service
4. Set build command: `dotnet publish -c Release`
5. Set start command: `dotnet CVBuilder.API.dll`

---

## 📋 Pre-Deployment Checklist

### Frontend (Next.js)

- [ ] Update API URL in `.env.production`: `NEXT_PUBLIC_API_URL=https://your-api-domain.com/api`
- [ ] Build and test locally: `npm run build && npm start`
- [ ] Check for console errors: `npm run build`
- [ ] Update `next.config.js` with production settings
- [ ] Add security headers in `next.config.js`
- [ ] Set up proper logging/monitoring
- [ ] Test all features in production build
- [ ] Add Google Analytics / Sentry for monitoring

### Backend (.NET)

- [ ] Review and update `appsettings.Production.json`
- [ ] Add database migrations if using EF Core
- [ ] Set strong API keys and secrets
- [ ] Enable HTTPS/SSL certificate
- [ ] Configure logging to Application Insights
- [ ] Test API with production settings: `dotnet run --configuration Release`
- [ ] Set up proper CORS policy for frontend domain
- [ ] Add API rate limiting
- [ ] Implement proper error handling and logging

### Database

- [ ] Create production database
- [ ] Set up automated backups
- [ ] Configure connection string for production
- [ ] Run migrations on production database
- [ ] Test database connectivity from app

### Security

- [ ] Set environment variables securely (never commit secrets)
- [ ] Enable HTTPS/TLS on all endpoints
- [ ] Set up firewall rules
- [ ] Implement API authentication (JWT, OAuth2)
- [ ] Add input validation and sanitization
- [ ] Set up Web Application Firewall (WAF)
- [ ] Implement rate limiting
- [ ] Regular security audits

---

## 🛡️ Security Best Practices

### Environment Variables

Never commit secrets to git!

```bash
# .env.production (DO NOT COMMIT)
NEXT_PUBLIC_API_URL=https://api.example.com
API_KEY=your-secret-key-here
DATABASE_URL=postgresql://user:pass@host/db
JWT_SECRET=your-jwt-secret
```

### API Security

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder
            .WithOrigins("https://yourdomain.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

### Frontend Security

```typescript
// In lib/api.ts
const headers = {
  'Content-Type': 'application/json',
  'X-Requested-With': 'XMLHttpRequest',
  // Add JWT token if using authentication
  'Authorization': `Bearer ${localStorage.getItem('token')}`
};
```

---

## 📊 Monitoring & Logging

### Application Insights (Azure)

```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddApplicationInsights();
```

### Sentry (Error Tracking)

```typescript
import * as Sentry from "@sentry/nextjs";

Sentry.init({
  dsn: process.env.NEXT_PUBLIC_SENTRY_DSN,
  environment: process.env.NODE_ENV,
  tracesSampleRate: 1.0,
});
```

### Custom Logging

```csharp
// Backend
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting at {Time}", DateTime.Now);
```

---

## 🔄 CI/CD Pipeline

### GitHub Actions Example

```yaml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Build Frontend
      run: |
        cd frontend
        npm install
        npm run build
    
    - name: Build Backend
      run: |
        cd backend
        dotnet build --configuration Release
        dotnet publish --configuration Release
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: cv-builder-frontend
        publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
        package: ./frontend/.next
```

---

## 📈 Performance Optimization

### Frontend

```typescript
// Next.js Image Optimization
import Image from 'next/image';

<Image 
  src="/thumbnail.jpg" 
  alt="Description"
  width={640}
  height={480}
  priority // For above-the-fold images
/>

// Code Splitting
const CVPreview = dynamic(() => import('@/components/CVPreview'), {
  loading: () => <p>Loading...</p>,
  ssr: false
});
```

### Backend

```csharp
// Add caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// API response caching
[ResponseCache(Duration = 300)]
[HttpGet("templates")]
public ActionResult GetTemplates()
{
    // ...
}
```

---

## 🧪 Testing

### Frontend Unit Tests

```bash
# Install testing libraries
npm install --save-dev @testing-library/react @testing-library/jest-dom jest

# Run tests
npm run test
```

### Backend Unit Tests

```bash
# Create test project
dotnet new xunit -n CVBuilder.API.Tests

# Run tests
dotnet test
```

---

## 📱 Progressive Web App (PWA)

Add to `next.config.js`:

```javascript
const withPWA = require('next-pwa')({
  dest: 'public',
  disable: process.env.NODE_ENV === 'development',
});

module.exports = withPWA({
  // ... next.js config
});
```

---

## 🌐 Domain & SSL

### Cloudflare Setup

1. Update nameservers to Cloudflare
2. Enable SSL/TLS (Full mode)
3. Set up page rules for caching
4. Enable security features (WAF, DDoS protection)

### Let's Encrypt (Self-hosted)

```bash
# Install certbot
sudo apt-get install certbot python3-certbot-nginx

# Create certificate
sudo certbot certonly --standalone -d yourdomain.com
```

---

## 🚨 Rollback Plan

1. Keep previous version deployed on separate slot
2. Use Azure deployment slots or blue-green deployment
3. Have database backup from before deployment
4. Document rollback procedures

---

## 📞 Support & Maintenance

- Set up automated monitoring alerts
- Schedule regular security updates
- Monitor error rates and performance metrics
- Keep dependencies updated
- Regular code reviews in production
- Maintain documentation

---

## Troubleshooting Production Issues

### Common Issues

| Issue | Solution |
|-------|----------|
| CORS errors | Check allowed origins in backend CORS policy |
| 502 Bad Gateway | Check backend is running and healthy |
| Slow API response | Check database performance, add caching |
| High memory usage | Implement pagination, optimize queries |
| SSL certificate errors | Renew certificate, check expiration date |

---

For more deployment guides, refer to:
- [Azure App Service Documentation](https://learn.microsoft.com/en-us/azure/app-service/)
- [Vercel Deployment Guide](https://vercel.com/docs)
- [Render Documentation](https://render.com/docs)
- [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/configuration/overview/)
