# ========================================
# SCRIPT DEPLOY KLDSHOP LÊN AZURE
# ========================================

Write-Host "🚀 DEPLOYING KLDSHOP TO AZURE..." -ForegroundColor Green
Write-Host ""

# Configuration
$resourceGroup = "KLDShopRG"
$location = "eastus"
$appServicePlan = "KLDShopPlan"
$webAppName = "kldshop-$(Get-Random -Maximum 9999)"  # Unique name
$runtime = "DOTNET|8.0"

Write-Host "📝 Configuration:" -ForegroundColor Yellow
Write-Host "  Resource Group: $resourceGroup"
Write-Host "  Location: $location"
Write-Host "  App Service Plan: $appServicePlan"
Write-Host "  Web App Name: $webAppName"
Write-Host "  Runtime: $runtime"
Write-Host ""

# Step 1: Login to Azure
Write-Host "🔐 Step 1: Login to Azure..." -ForegroundColor Cyan
az login
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Login failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Login successful!" -ForegroundColor Green
Write-Host ""

# Step 2: Create Resource Group
Write-Host "📦 Step 2: Creating Resource Group..." -ForegroundColor Cyan
az group create --name $resourceGroup --location $location
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Resource Group created!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Resource Group might already exist, continuing..." -ForegroundColor Yellow
}
Write-Host ""

# Step 3: Create App Service Plan (Free Tier)
Write-Host "💰 Step 3: Creating App Service Plan (FREE TIER)..." -ForegroundColor Cyan
az appservice plan create `
    --name $appServicePlan `
    --resource-group $resourceGroup `
    --sku F1 `
    --is-linux
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ App Service Plan created!" -ForegroundColor Green
} else {
    Write-Host "⚠️  App Service Plan might already exist, continuing..." -ForegroundColor Yellow
}
Write-Host ""

# Step 4: Create Web App
Write-Host "🌐 Step 4: Creating Web App..." -ForegroundColor Cyan
az webapp create `
    --resource-group $resourceGroup `
    --plan $appServicePlan `
    --name $webAppName `
    --runtime $runtime
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Web App creation failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Web App created!" -ForegroundColor Green
Write-Host ""

# Step 5: Configure Web App Settings
Write-Host "⚙️  Step 5: Configuring Web App..." -ForegroundColor Cyan
az webapp config appsettings set `
    --resource-group $resourceGroup `
    --name $webAppName `
    --settings ASPNETCORE_ENVIRONMENT=Production
Write-Host "✅ Configuration complete!" -ForegroundColor Green
Write-Host ""

# Step 6: Build and Publish
Write-Host "🔨 Step 6: Building project..." -ForegroundColor Cyan
dotnet publish -c Release -o ./publish
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

# Step 7: Create ZIP file
Write-Host "📦 Step 7: Creating deployment package..." -ForegroundColor Cyan
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force
Write-Host "✅ Package created!" -ForegroundColor Green
Write-Host ""

# Step 8: Deploy to Azure
Write-Host "🚀 Step 8: Deploying to Azure..." -ForegroundColor Cyan
az webapp deployment source config-zip `
    --resource-group $resourceGroup `
    --name $webAppName `
    --src ./publish.zip
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Deployment failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Deployment successful!" -ForegroundColor Green
Write-Host ""

# Step 9: Get URL
Write-Host "🎉 DEPLOYMENT COMPLETE!" -ForegroundColor Green -BackgroundColor Black
Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  YOUR WEBSITE IS LIVE!" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "🌐 Website URL:" -ForegroundColor Yellow
Write-Host "   https://$webAppName.azurewebsites.net" -ForegroundColor Green
Write-Host ""
Write-Host "📍 Sitemap URL:" -ForegroundColor Yellow
Write-Host "   https://$webAppName.azurewebsites.net/sitemap.xml" -ForegroundColor Green
Write-Host ""
Write-Host "🤖 Robots.txt URL:" -ForegroundColor Yellow
Write-Host "   https://$webAppName.azurewebsites.net/robots.txt" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Update robots.txt với domain mới" -ForegroundColor White
Write-Host "   2. Setup custom domain (optional)" -ForegroundColor White
Write-Host "   3. Configure database connection" -ForegroundColor White
Write-Host "   4. Submit sitemap to Google Search Console" -ForegroundColor White
Write-Host ""
Write-Host "📊 View in Azure Portal:" -ForegroundColor Yellow
Write-Host "   https://portal.azure.com" -ForegroundColor Cyan
Write-Host ""

# Cleanup
Remove-Item -Path ./publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ./publish.zip -Force -ErrorAction SilentlyContinue

Write-Host "✅ Cleanup complete!" -ForegroundColor Green
Write-Host ""
