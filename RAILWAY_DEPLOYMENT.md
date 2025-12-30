# 🚂 Hướng dẫn Deploy KLDShop lên Railway

## 📋 Yêu cầu
- Tài khoản Railway (https://railway.app)
- Git repository (GitHub, GitLab, hoặc Bitbucket)
- Code đã được push lên repository

## 🚀 Các bước Deploy

### Bước 1: Tạo Project trên Railway

1. Đăng nhập vào Railway (https://railway.app)
2. Click **"New Project"**
3. Chọn **"Deploy from GitHub repo"**
4. Chọn repository **KLDShop** của bạn
5. Railway sẽ tự động phát hiện Dockerfile và bắt đầu build

### Bước 2: Thêm PostgreSQL Database

1. Trong project, click **"New"** → **"Database"** → **"Add PostgreSQL"**
2. Railway sẽ tự động tạo database và set biến môi trường `DATABASE_URL`
3. Code của bạn đã được cấu hình sẵn để dùng biến này (xem `Program.cs` dòng 12-17)

### Bước 3: Cấu hình Environment Variables

Trong Railway project, vào **Variables** tab và thêm các biến sau:

#### Biến bắt buộc (đã tự động):
- `DATABASE_URL` - Tự động set khi thêm PostgreSQL
- `PORT` - Tự động set bởi Railway

#### Biến tùy chọn (Payment & Email):

```bash
# VNPay (Nếu bạn dùng VNPay)
VNPay__Enabled=true
VNPay__TmnCode=YOUR_VNPAY_TMN_CODE
VNPay__HashSecret=YOUR_VNPAY_HASH_SECRET
VNPay__PaymentUrl=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
VNPay__ReturnUrl=https://your-app.railway.app/Order/PaymentReturn

# PayPal (Nếu bạn dùng PayPal)
PayPal__Enabled=true
PayPal__Mode=sandbox
PayPal__ClientId=YOUR_PAYPAL_CLIENT_ID
PayPal__ClientSecret=YOUR_PAYPAL_CLIENT_SECRET

# MailChimp (Nếu bạn dùng Newsletter)
MailChimp__ApiKey=YOUR_MAILCHIMP_API_KEY
MailChimp__ListId=YOUR_MAILCHIMP_LIST_ID

# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning

# Allowed Hosts (optional, * cho phép tất cả)
AllowedHosts=*
```

### Bước 4: Chạy Migrations

Railway sẽ tự động build và deploy, nhưng database sẽ trống. Bạn có 2 cách:

#### Cách 1: Sử dụng Railway CLI (Khuyên dùng)

```bash
# Cài Railway CLI
npm i -g @railway/cli

# Login
railway login

# Link project
railway link

# Chạy migrations
railway run dotnet ef database update
```

#### Cách 2: Sử dụng SQL Scripts

1. Kết nối tới PostgreSQL database từ Railway dashboard
2. Copy connection string từ biến `DATABASE_URL`
3. Dùng pgAdmin hoặc psql để chạy SQL scripts (cần convert từ SQL Server sang PostgreSQL syntax)

#### Cách 3: Thêm Migration vào Dockerfile (Tự động)

Cập nhật Dockerfile để tự động chạy migrations khi deploy:

```dockerfile
# Thêm vào script start.sh
RUN echo '#!/bin/sh\\n\\\
if [ ! -z \"$PORT\" ]; then\\n\\\
  export ASPNETCORE_URLS=\"http://+:$PORT\"\\n\\\
fi\\n\\\
dotnet ef database update\\n\\\
dotnet KLDShop.dll' > /app/start.sh && chmod +x /app/start.sh
```

⚠️ **Lưu ý**: Bạn cần cài `dotnet ef` tools trong Dockerfile để dùng cách này.

### Bước 5: Seed Data (Optional)

Sau khi migrations chạy xong, bạn có thể seed data:

```bash
# Qua Railway CLI
railway run dotnet run --urls "http://localhost:8080"

# Hoặc truy cập endpoint sau khi deploy:
https://your-app.railway.app/Seed/SeedAll
```

### Bước 6: Cấu hình Custom Domain (Optional)

1. Trong Railway project, vào **Settings** tab
2. Tìm **Domains** section
3. Click **Generate Domain** để có subdomain miễn phí (*.railway.app)
4. Hoặc **Add Custom Domain** nếu bạn có domain riêng

## 🔍 Kiểm tra Deployment

1. **Logs**: Xem logs trong Railway dashboard để debug
2. **Health Check**: Truy cập `https://your-app.railway.app` để kiểm tra
3. **Database**: Kiểm tra connection trong **Data** tab

## 🛠️ Troubleshooting

### Lỗi: "Failed to connect to database"
- Kiểm tra biến `DATABASE_URL` đã được set
- Đảm bảo PostgreSQL service đang chạy

### Lỗi: "Port already in use"
- Railway tự động set `PORT`, không cần lo lắng
- Code đã xử lý trong `Dockerfile` và `Program.cs`

### Lỗi: "Migrations not applied"
- Chạy `railway run dotnet ef database update`
- Hoặc thêm migration vào startup script

### App build nhưng không start
- Kiểm tra logs trong Railway dashboard
- Đảm bảo `ASPNETCORE_URLS` được set đúng

## 📊 Monitoring

Railway cung cấp:
- **Metrics**: CPU, Memory, Network usage
- **Logs**: Real-time application logs
- **Deployments**: History của các lần deploy

## 💰 Chi phí

- **Hobby Plan**: $5/month cho 1 project
- **Developer Plan**: $20/month cho unlimited projects
- **Free Trial**: 500 hours/month (khoảng 20 ngày)

## 🔒 Security Best Practices

1. ✅ Không commit `appsettings.json` với sensitive data
2. ✅ Dùng Railway environment variables cho secrets
3. ✅ Enable HTTPS (Railway tự động có SSL)
4. ✅ Rotate API keys thường xuyên
5. ✅ Set `AllowedHosts` cho production

## 📝 Cập nhật sau khi Deploy

Mỗi khi push code mới lên GitHub:
1. Railway tự động phát hiện changes
2. Tự động build lại
3. Tự động deploy version mới
4. Zero-downtime deployment

## 🆘 Support

- Railway Docs: https://docs.railway.app
- Railway Discord: https://discord.gg/railway
- GitHub Issues: https://github.com/YOUR_USERNAME/KLDShop/issues

---

**Chúc bạn deploy thành công! 🎉**
