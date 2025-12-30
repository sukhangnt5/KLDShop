# 🚂 Hướng dẫn Deploy KLDShop lên Railway.app

## 📋 Yêu cầu
- Tài khoản GitHub (đã có ✅)
- Tài khoản Railway.app (miễn phí)
- Code đã push lên GitHub (đã xong ✅)

## 🚀 Các bước Deploy

### Bước 1: Tạo tài khoản Railway.app

1. Truy cập: https://railway.app
2. Click **"Start a New Project"** hoặc **"Login"**
3. Chọn **"Login with GitHub"**
4. Authorize Railway truy cập GitHub của bạn

### Bước 2: Tạo Project mới

1. Sau khi đăng nhập, click **"New Project"**
2. Chọn **"Deploy from GitHub repo"**
3. Chọn repository: **`sukhangnt2/KLDShop`**
4. Railway sẽ tự động detect Dockerfile và bắt đầu deploy

### Bước 3: Thêm Database (SQL Server)

1. Trong project của bạn, click **"New"** → **"Database"**
2. Chọn **"Add PostgreSQL"** (Railway không hỗ trợ SQL Server free)
   
   **LƯU Ý:** Bạn cần chọn 1 trong 2 option:
   
   **Option A: Dùng PostgreSQL (Khuyên dùng - FREE)**
   - Click **"Add PostgreSQL"**
   - Railway sẽ tự động tạo và connect database
   - Bạn cần update code để hỗ trợ PostgreSQL (tôi sẽ giúp)
   
   **Option B: Dùng SQL Server External**
   - Dùng SQL Server từ nơi khác (Azure SQL, AWS RDS)
   - Thêm connection string vào Environment Variables

### Bước 4: Cấu hình Environment Variables

1. Trong Railway project, click vào service **KLDShop**
2. Chọn tab **"Variables"**
3. Thêm các biến sau:

```
# Database (nếu dùng PostgreSQL)
DATABASE_URL=<Railway tự động tạo khi add PostgreSQL>

# Hoặc nếu dùng SQL Server external
DATABASE_URL=Server=your_server;Database=KLDShop;User Id=user;Password=pass;

# VNPay (Optional)
VNPay__Enabled=true
VNPay__TmnCode=YOUR_TMNCODE
VNPay__HashSecret=YOUR_HASH_SECRET
VNPay__ReturnUrl=https://your-app.railway.app/Order/PaymentReturn

# PayPal (Optional)
PayPal__Enabled=true
PayPal__Mode=sandbox
PayPal__ClientId=YOUR_CLIENT_ID
PayPal__ClientSecret=YOUR_CLIENT_SECRET

# MailChimp (Optional)
MailChimp__ApiKey=YOUR_API_KEY
MailChimp__ListId=YOUR_LIST_ID

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
```

### Bước 5: Deploy

1. Railway sẽ tự động build và deploy
2. Xem logs trong tab **"Deployments"**
3. Sau khi deploy xong, Railway sẽ cung cấp URL public

### Bước 6: Lấy URL của app

1. Trong service **KLDShop**, click tab **"Settings"**
2. Scroll xuống **"Networking"**
3. Click **"Generate Domain"**
4. Railway sẽ tạo URL dạng: `https://kldshop-production.up.railway.app`

### Bước 7: Update Return URLs

Cập nhật lại các return URLs trong Environment Variables:
```
VNPay__ReturnUrl=https://your-app.railway.app/Order/PaymentReturn
```

## ✅ Hoàn thành!

App của bạn đã live tại: `https://your-app.railway.app`

---

## 🔧 Troubleshooting

### Lỗi: Database connection failed
- Kiểm tra `DATABASE_URL` đã được set đúng chưa
- Nếu dùng PostgreSQL, đảm bảo đã thêm service PostgreSQL

### Lỗi: Build failed
- Kiểm tra logs trong tab "Deployments"
- Đảm bảo Dockerfile đúng format

### App không chạy
- Kiểm tra PORT environment variable
- Railway tự động set PORT, không cần config thêm

---

## 💰 Chi phí

Railway cung cấp **$5 credit miễn phí mỗi tháng**, đủ để chạy:
- 1 web app nhỏ
- 1 PostgreSQL database
- Bandwidth hợp lý

Nếu vượt $5, app sẽ tạm dừng cho đến tháng sau.

---

## 🆘 Cần hỗ trợ?

**Option A: Dùng PostgreSQL (FREE - Khuyên dùng)**
- Tôi sẽ giúp bạn chuyển đổi từ SQL Server sang PostgreSQL
- Chỉ mất 5-10 phút

**Option B: Dùng SQL Server external**
- Cần tìm nơi host SQL Server miễn phí khác
- Hoặc dùng SQL Server local + ngrok (chỉ để test)

**Bạn chọn option nào?**
