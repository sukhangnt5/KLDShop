# 🚀 HƯỚNG DẪN DEPLOY KLDSHOP LÊN AZURE

## 📋 MỤC LỤC
1. [Chuẩn bị](#chuẩn-bị)
2. [Phương án 1: Deploy qua Visual Studio](#phương-án-1-visual-studio)
3. [Phương án 2: Deploy qua PowerShell Script](#phương-án-2-powershell-script)
4. [Cấu hình Database](#cấu-hình-database)
5. [Custom Domain](#custom-domain)
6. [SSL Certificate](#ssl-certificate)
7. [Troubleshooting](#troubleshooting)

---

## 🎯 CHUẨN BỊ

### Yêu cầu:
- ✅ Tài khoản Azure (Free trial: https://azure.microsoft.com/free/)
- ✅ .NET SDK 8.0+
- ✅ Azure CLI (https://aka.ms/azure-cli)
- ✅ Visual Studio 2022 hoặc VS Code

### Đăng ký Azure:
1. Vào: https://azure.microsoft.com/free/
2. Click **Start free**
3. Nhận $200 credit miễn phí cho 30 ngày
4. Free services: App Service F1, 12 tháng miễn phí

---

## 📱 PHƯƠNG ÁN 1: VISUAL STUDIO (Dễ nhất)

### Bước 1: Mở Project
1. Mở **KLDShop.csproj** trong Visual Studio 2022
2. Build để đảm bảo không có lỗi

### Bước 2: Publish
1. Right-click vào project → **Publish**
2. Chọn **Azure** → **Next**
3. Chọn **Azure App Service (Windows)** → **Next**
4. Click **Sign in** và đăng nhập Azure

### Bước 3: Tạo App Service
1. Click **Create New**
2. Điền thông tin:
   ```
   Name: kldshop-yourusername
   Subscription: Azure subscription 1
   Resource Group: KLDShopRG (Create new)
   Hosting Plan:
     - Name: KLDShopPlan
     - Location: East US
     - Size: F1 (Free) ← QUAN TRỌNG!
   ```
3. Click **Create**

### Bước 4: Deploy
1. Sau khi tạo xong, click **Finish**
2. Click **Publish**
3. Đợi 2-5 phút
4. Website sẽ tự động mở: `https://kldshop-yourusername.azurewebsites.net`

---

## 💻 PHƯƠNG ÁN 2: POWERSHELL SCRIPT (Pro)

### Bước 1: Install Azure CLI
```powershell
# Download và install từ:
# https://aka.ms/installazurecliwindows
```

### Bước 2: Run Script
```powershell
# Chạy script deploy
.\deploy-to-azure.ps1
```

Script sẽ tự động:
- ✅ Login Azure
- ✅ Tạo Resource Group
- ✅ Tạo App Service Plan (Free tier)
- ✅ Tạo Web App
- ✅ Build project
- ✅ Deploy lên Azure
- ✅ Show URL

### Bước 3: Manual (nếu không dùng script)
```powershell
# 1. Login
az login

# 2. Tạo resource group
az group create --name KLDShopRG --location eastus

# 3. Tạo app service plan (FREE)
az appservice plan create `
  --name KLDShopPlan `
  --resource-group KLDShopRG `
  --sku F1 `
  --is-linux

# 4. Tạo web app
az webapp create `
  --resource-group KLDShopRG `
  --plan KLDShopPlan `
  --name kldshop-yourname `
  --runtime "DOTNET|8.0"

# 5. Build và publish
dotnet publish -c Release -o ./publish

# 6. Tạo zip
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

# 7. Deploy
az webapp deployment source config-zip `
  --resource-group KLDShopRG `
  --name kldshop-yourname `
  --src ./publish.zip
```

---

## 🗄️ CẤU HÌNH DATABASE

### Option A: SQLite (Đơn giản - Free)
1. Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=kldshop.db"
  }
}
```

2. Install package:
```powershell
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

3. Update `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Option B: Azure SQL Database (Paid - $5/month)
1. Tạo SQL Database trên Azure Portal
2. Copy connection string
3. Update appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Database=KLDShop;User ID=admin;Password=YourPassword123!;Encrypt=True;"
  }
}
```

---

## 🌐 CUSTOM DOMAIN

### Bước 1: Mua domain
- Namecheap, GoDaddy, CloudFlare, etc.

### Bước 2: Configure trong Azure
1. Vào Azure Portal → App Service → Custom domains
2. Click **Add custom domain**
3. Nhập domain: `kldshop.com`
4. Thêm DNS records:
   ```
   Type: CNAME
   Name: www
   Value: kldshop-yourname.azurewebsites.net
   
   Type: TXT
   Name: asuid
   Value: [provided by Azure]
   ```

### Bước 3: Verify
1. Đợi DNS propagate (5-60 phút)
2. Click **Validate** trong Azure
3. Click **Add**

---

## 🔒 SSL CERTIFICATE (HTTPS)

### Free SSL từ Azure:
1. Vào App Service → TLS/SSL settings
2. Click **Private Key Certificates (.pfx)**
3. Click **Create App Service Managed Certificate**
4. Chọn domain của bạn
5. Click **Create**

### Bind SSL:
1. Vào **Custom domains**
2. Click **Add binding**
3. Chọn certificate vừa tạo
4. SSL Type: **SNI SSL**
5. Click **Add**

---

## ⚙️ CẤU HÌNH QUAN TRỌNG

### 1. Update robots.txt
```
# Sửa trong wwwroot/robots.txt
Sitemap: https://yourdomain.com/sitemap.xml
```

### 2. Application Settings
Vào Azure Portal → Configuration → Application settings:
```
ASPNETCORE_ENVIRONMENT = Production
WEBSITE_RUN_FROM_PACKAGE = 1
```

### 3. Connection Strings
Add connection string trong Configuration

---

## 🐛 TROUBLESHOOTING

### Lỗi: "Application Error"
```powershell
# Xem logs
az webapp log tail --resource-group KLDShopRG --name kldshop-yourname
```

### Lỗi: Database connection failed
1. Check connection string
2. Verify firewall rules (Azure SQL)
3. Test local connection

### Lỗi: 500 Internal Server Error
1. Check `appsettings.Production.json`
2. Enable detailed errors temporarily
3. Check Application Insights logs

### Website chậm (Free tier)
- Free tier sleep sau 20 phút không dùng
- First request sau sleep sẽ chậm (30s)
- Upgrade lên Basic tier ($13/month) để fix

---

## 📊 MONITOR & LOGS

### View Logs:
```powershell
az webapp log tail --resource-group KLDShopRG --name kldshop-yourname
```

### Application Insights:
1. Enable trong Azure Portal
2. View performance metrics
3. Track errors automatically

---

## 💰 CHI PHÍ

### Free Tier (F1):
- ✅ 1GB RAM
- ✅ 1GB Storage
- ✅ 60 CPU minutes/day
- ✅ Custom domain supported
- ✅ Free SSL
- ⚠️ Website sleep sau 20 phút không dùng

### Basic Tier (B1) - $13/month:
- ✅ 1.75GB RAM
- ✅ 10GB Storage
- ✅ Always on (không sleep)
- ✅ Custom domain
- ✅ Free SSL
- ✅ Backup

### Standard Tier (S1) - $69/month:
- ✅ 1.75GB RAM
- ✅ 50GB Storage
- ✅ Auto-scaling
- ✅ Staging slots
- ✅ Daily backups

---

## 🎯 CHECKLIST SAU KHI DEPLOY

- [ ] Website accessible: `https://yourapp.azurewebsites.net`
- [ ] Sitemap works: `/sitemap.xml`
- [ ] Robots.txt works: `/robots.txt`
- [ ] Database connected
- [ ] Images loading correctly
- [ ] All pages working
- [ ] SSL certificate active
- [ ] Custom domain configured (optional)
- [ ] Submit sitemap to Google Search Console
- [ ] Setup monitoring/alerts

---

## 📚 TÀI LIỆU THAM KHẢO

- Azure App Service: https://docs.microsoft.com/azure/app-service/
- Azure CLI: https://docs.microsoft.com/cli/azure/
- Deploy ASP.NET Core: https://docs.microsoft.com/aspnet/core/host-and-deploy/azure-apps/

---

## 🆘 HỖ TRỢ

Nếu gặp vấn đề:
1. Check logs trong Azure Portal
2. Google error message
3. Stack Overflow
4. Azure Support (nếu có subscription)

---

**🎉 CHÚC BẠN DEPLOY THÀNH CÔNG!**
