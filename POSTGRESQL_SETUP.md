# 🐘 PostgreSQL Setup Guide

## ✅ Đã hoàn thành chuyển đổi từ SQL Server sang PostgreSQL!

### 🔧 Những thay đổi đã thực hiện:

1. ✅ Thay thế `Microsoft.EntityFrameworkCore.SqlServer` → `Npgsql.EntityFrameworkCore.PostgreSQL`
2. ✅ Cập nhật `Program.cs` để sử dụng PostgreSQL
3. ✅ Xóa migrations cũ và tạo migrations mới cho PostgreSQL
4. ✅ Cập nhật connection string trong `appsettings.json`

---

## 🚂 Deploy lên Railway.app

### Bước 1: Login Railway
1. Truy cập: https://railway.app
2. Click **"Login with GitHub"**
3. Authorize Railway

### Bước 2: Create New Project
1. Click **"New Project"**
2. Chọn **"Deploy from GitHub repo"**
3. Chọn: **`sukhangnt5/KLDShop`**
4. Railway sẽ tự động build và deploy

### Bước 3: Add PostgreSQL Database
1. Trong project, click **"New"** → **"Database"**
2. Chọn **"Add PostgreSQL"**
3. Railway sẽ tự động:
   - Tạo PostgreSQL database
   - Tạo biến `DATABASE_URL`
   - Tự động connect với app

### Bước 4: Run Migrations (Quan trọng!)
Railway sẽ tự động dùng `DATABASE_URL`, nhưng bạn cần chạy migrations:

**Option A: Trong Railway Dashboard**
1. Vào service **KLDShop**
2. Click tab **"Settings"**
3. Scroll xuống **"Deploy"**
4. Add **"Run Command"**: 
   ```
   dotnet ef database update
   ```

**Option B: Local (Nếu có PostgreSQL local)**
1. Copy `DATABASE_URL` từ Railway
2. Chạy:
   ```bash
   dotnet ef database update --connection "YOUR_DATABASE_URL"
   ```

### Bước 5: Generate Domain
1. Vào tab **"Settings"** của service KLDShop
2. Scroll xuống **"Networking"**
3. Click **"Generate Domain"**
4. Bạn sẽ có URL: `https://kldshop-production.up.railway.app`

### Bước 6: Update Return URLs (Optional - Nếu dùng Payment)
Trong tab **"Variables"**, thêm:
```
VNPay__ReturnUrl=https://your-app.railway.app/Order/PaymentReturn
PayPal__ReturnUrl=https://your-app.railway.app/Order/PaymentReturn
```

---

## 🔧 Local Development với PostgreSQL

### Cài đặt PostgreSQL Local (Optional)

**Windows:**
1. Download: https://www.postgresql.org/download/windows/
2. Install với password: `yourpassword`
3. Tạo database:
   ```sql
   CREATE DATABASE KLDShop;
   ```

**Hoặc dùng Docker:**
```bash
docker run -d \
  --name kldshop-postgres \
  -e POSTGRES_PASSWORD=yourpassword \
  -e POSTGRES_DB=KLDShop \
  -p 5432:5432 \
  postgres:16
```

### Run Migrations Local
```bash
dotnet ef database update
```

---

## 📊 Connection String Format

**Local Development:**
```
Host=localhost;Database=KLDShop;Username=postgres;Password=yourpassword
```

**Railway (Tự động):**
Railway tự động cung cấp `DATABASE_URL` dạng:
```
postgresql://user:password@host:port/database
```

Code đã được cấu hình để tự động đọc `DATABASE_URL` từ environment variable!

---

## 🆘 Troubleshooting

### Lỗi: "password authentication failed"
- Kiểm tra username/password trong connection string
- Railway: Dùng `DATABASE_URL` đã cung cấp, không cần thay đổi

### Lỗi: "database does not exist"
- Chạy migrations: `dotnet ef database update`
- Railway: Database tự động tạo khi add PostgreSQL service

### App không kết nối database
- Kiểm tra biến `DATABASE_URL` đã được set trong Railway
- Xem logs trong tab "Deployments"

---

## 💰 Railway Free Tier

**$5 credit/tháng bao gồm:**
- ✅ 1 Web Service (KLDShop)
- ✅ 1 PostgreSQL Database
- ✅ 500 GB bandwidth
- ✅ Đủ cho demo/production nhỏ

---

## ✅ Hoàn thành!

Project đã sẵn sàng deploy lên Railway với PostgreSQL! 🎉

**Next steps:**
1. Push code lên GitHub ✅
2. Deploy trên Railway
3. Add PostgreSQL database
4. Generate domain
5. Enjoy! 🚀
