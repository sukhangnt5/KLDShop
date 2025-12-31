# 🔐 Cấu hình PayPal trên Railway

## 📋 Thông tin PayPal Sandbox của bạn

```
Client ID: AR4WJh1Rsp8btdXsUYJL4qMiIKjshAsIy4YNFQO5CQ2hPrwQnm4e-bzjxPTTQ2UFcv1nImkOw7RLIZAU
Client Secret: EBs79_p8bCMLWveEJsWQUvaD1EqlV8JmjPvc6Hl8X_slSh_t1llJSrTVKg_SNPPeegAuiCa0oNDvsKqV
Mode: sandbox
```

## 🚀 Bước 1: Set Environment Variables trên Railway

### Cách 1: Qua Railway Dashboard (Dễ nhất)

1. Truy cập Railway Dashboard: https://railway.app/dashboard
2. Chọn project **KLDShop**
3. Click vào service (web app)
4. Vào tab **Variables**
5. Click **"+ New Variable"** và thêm từng biến sau:

```bash
PayPal__Enabled=true
PayPal__Mode=sandbox
PayPal__ClientId=AR4WJh1Rsp8btdXsUYJL4qMiIKjshAsIy4YNFQO5CQ2hPrwQnm4e-bzjxPTTQ2UFcv1nImkOw7RLIZAU
PayPal__ClientSecret=EBs79_p8bCMLWveEJsWQUvaD1EqlV8JmjPvc6Hl8X_slSh_t1llJSrTVKg_SNPPeegAuiCa0oNDvsKqV
```

6. Click **"Deploy"** hoặc đợi auto-deploy

### Cách 2: Qua Railway CLI (Nhanh hơn)

```bash
# Cài Railway CLI (nếu chưa có)
npm i -g @railway/cli

# Login
railway login

# Link tới project
railway link

# Set variables
railway variables set PayPal__Enabled=true
railway variables set PayPal__Mode=sandbox
railway variables set PayPal__ClientId=AR4WJh1Rsp8btdXsUYJL4qMiIKjshAsIy4YNFQO5CQ2hPrwQnm4e-bzjxPTTQ2UFcv1nImkOw7RLIZAU
railway variables set PayPal__ClientSecret=EBs79_p8bCMLWveEJsWQUvaD1EqlV8JmjPvc6Hl8X_slSh_t1llJSrTVKg_SNPPeegAuiCa0oNDvsKqV

# Trigger redeploy
railway up
```

## 🧪 Bước 2: Test PayPal Payment

### 1. Đợi Railway deploy xong (~2-3 phút)

Kiểm tra logs trong Railway dashboard:
```
✅ "PayPal payment gateway initialized successfully"
✅ "Payment page - PayPalClientId: AR4WJh1..."
```

### 2. Tạo đơn hàng test

1. Truy cập app: `https://your-app.railway.app`
2. Đăng nhập/Đăng ký
3. Thêm sản phẩm vào giỏ hàng
4. Click **"Thanh Toán"**
5. Chọn **"PayPal"**
6. Click **"Tiếp Tục Thanh Toán"**

### 3. PayPal Button sẽ xuất hiện!

Bạn sẽ thấy:
- ✅ Button màu vàng **"PayPal"**
- ✅ Click vào sẽ mở popup PayPal
- ✅ Login với **PayPal Sandbox Account**

## 💳 PayPal Sandbox Test Accounts

### Tạo test accounts trên PayPal Developer:

1. Truy cập: https://developer.paypal.com/dashboard/accounts
2. Click **"Create Account"**
3. Tạo 2 accounts:
   - **Personal** (Buyer) - Để test thanh toán
   - **Business** (Seller) - Đã được tạo sẵn với ClientId của bạn

### Test account mẫu:

```
Email: sb-buyer@personal.example.com
Password: Test1234!
```

## 🔍 Kiểm tra Logs

### Railway Logs cần xem:

```bash
# Good logs ✅
"PayPal payment gateway initialized successfully"
"Payment page - PaymentMethod: PayPal, PayPalClientId: AR4WJh1..."
"Creating PayPal order - OrderId: X, Amount: Y"
"PayPal payment completed - OrderId: X, TransactionId: ABC123"

# Bad logs ❌
"PayPal initialization failed: Invalid credentials"
"PayPal ClientId: YOUR_PAYPAL_CLIENT_ID" # Nghĩa là chưa set biến
```

### Browser Console Logs:

Mở F12 → Console:
```javascript
✅ "PayPal ClientId: AR4WJh1Rsp8btdXsUYJL4qMiIKjshAsIy4YNFQO5CQ2h..."
✅ "Creating PayPal order - OrderId: 123, Amount: 100"
✅ "PayPal order approved: PAYID-XXXXXX"

❌ "PayPal error: Invalid client_id"
❌ "PayPal ClientId: YOUR_PAYPAL_CLIENT_ID"
```

## 🛠️ Troubleshooting

### Vấn đề 1: PayPal button không hiện

**Nguyên nhân:**
- Environment variables chưa được set
- Railway chưa deploy lại sau khi set variables

**Giải pháp:**
```bash
# Kiểm tra variables
railway variables

# Trigger redeploy
railway redeploy
```

### Vấn đề 2: Lỗi "Invalid client_id"

**Nguyên nhân:**
- ClientId sai hoặc có khoảng trắng thừa
- Dùng Production ClientId cho Sandbox mode

**Giải pháp:**
- Copy lại ClientId chính xác (không có space)
- Đảm bảo `PayPal__Mode=sandbox`

### Vấn đề 3: Thanh toán thành công nhưng lỗi database

**Nguyên nhân:**
- DateTime.Now thay vì DateTime.UtcNow (đã fix ✅)
- Duplicate Payment records (đã fix ✅)

**Giải pháp:**
- Đã được fix trong commit trước
- Pull code mới nhất và redeploy

## 🎯 Chuyển sang Production

Khi sẵn sàng production:

1. **Tạo PayPal Live App:**
   - Vào https://developer.paypal.com/dashboard/applications
   - Switch từ **Sandbox** sang **Live**
   - Create new app để lấy Live credentials

2. **Update Railway variables:**
   ```bash
   PayPal__Mode=live
   PayPal__ClientId=YOUR_LIVE_CLIENT_ID
   PayPal__ClientSecret=YOUR_LIVE_CLIENT_SECRET
   ```

3. **Business account verification:**
   - PayPal yêu cầu verify business account
   - Cần thông tin công ty, tài khoản ngân hàng
   - Quy trình ~2-3 ngày làm việc

## 📊 Transaction Monitoring

### Sandbox Transactions:

1. Vào https://developer.paypal.com/dashboard
2. Click **"Sandbox"** → **"Accounts"**
3. Click **"View/Edit"** trên Business account
4. Xem **"Transaction History"**

### Live Transactions:

1. Vào https://www.paypal.com/businessmanage/
2. Click **"Activity"**
3. Xem tất cả transactions thật

## 💡 Tips

1. **Test nhiều scenarios:**
   - Thanh toán thành công
   - Hủy giữa chừng
   - Insufficient funds
   - Expired session

2. **Monitoring:**
   - Theo dõi Railway logs real-time
   - Set up alerts cho payment failures
   - Track conversion rate

3. **Security:**
   - ✅ Không commit ClientSecret vào Git
   - ✅ Rotate credentials định kỳ
   - ✅ Enable 2FA cho PayPal account

## 🆘 Support

- PayPal Developer Docs: https://developer.paypal.com/docs/
- PayPal Sandbox Issues: https://developer.paypal.com/support/
- Railway Support: https://railway.app/help

---

**Setup xong rồi! Test PayPal payment ngay thôi! 🎉**
