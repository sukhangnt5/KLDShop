# 🔐 Railway Environment Variables - Complete Setup

## 📋 Tất cả Variables cần set cho Railway

### ✅ **Copy và paste vào Railway Dashboard:**

```bash
# === Database Connection ===
# Railway tự động set biến DATABASE_URL, không cần thêm

# === PayPal Configuration ===
PayPal__Enabled=true
PayPal__Mode=sandbox
PayPal__ClientId=AR4WJh1Rsp8btdXsUYJL4qMiIKjshAsIy4YNFQO5CQ2hPrwQnm4e-bzjxPTTQ2UFcv1nImkOw7RLIZAU
PayPal__ClientSecret=EBs79_p8bCMLWveEJsWQUvaD1EqlV8JmjPvc6Hl8X_slSh_t1llJSrTVKg_SNPPeegAuiCa0oNDvsKqV

# === VNPay Configuration ===
VNPay__Enabled=true
VNPay__TmnCode=YOUR_VNPAY_TMN_CODE
VNPay__HashSecret=YOUR_VNPAY_HASH_SECRET
VNPay__PaymentUrl=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
VNPay__ApiUrl=https://sandbox.vnpayment.vn/merchant_webapi/merchant.do
VNPay__ReturnUrl=https://YOUR_RAILWAY_APP_URL/Order/PaymentReturn

# === MailChimp Configuration (Optional) ===
MailChimp__ApiKey=YOUR_MAILCHIMP_API_KEY
MailChimp__ListId=YOUR_MAILCHIMP_LIST_ID

# === Logging ===
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning
```

---

## 🚀 Cách 1: Set qua Railway Dashboard (Khuyến nghị)

### Bước 1: Truy cập Railway Dashboard
1. Vào: https://railway.app/dashboard
2. Chọn project **KLDShop**
3. Click vào service (web app)
4. Click tab **"Variables"**

### Bước 2: Thêm từng biến

Click **"+ New Variable"** và thêm lần lượt:

#### **PayPal (4 biến):**
```
Name: PayPal__Enabled
Value: true

Name: PayPal__Mode
Value: sandbox

Name: PayPal__ClientId
Value: AR4WJh1Rsp8btdXsUYJL4qMiIKjshAsIy4YNFQO5CQ2hPrwQnm4e-bzjxPTTQ2UFcv1nImkOw7RLIZAU

Name: PayPal__ClientSecret
Value: EBs79_p8bCMLWveEJsWQUvaD1EqlV8JmjPvc6Hl8X_slSh_t1llJSrTVKg_SNPPeegAuiCa0oNDvsKqV
```

#### **VNPay (6 biến):**
```
Name: VNPay__Enabled
Value: true

Name: VNPay__TmnCode
Value: YOUR_VNPAY_TMN_CODE
(Thay bằng TmnCode thật của bạn)

Name: VNPay__HashSecret
Value: YOUR_VNPAY_HASH_SECRET
(Thay bằng HashSecret thật của bạn)

Name: VNPay__PaymentUrl
Value: https://sandbox.vnpayment.vn/paymentv2/vpcpay.html

Name: VNPay__ApiUrl
Value: https://sandbox.vnpayment.vn/merchant_webapi/merchant.do

Name: VNPay__ReturnUrl
Value: https://YOUR_RAILWAY_APP_URL/Order/PaymentReturn
(Thay YOUR_RAILWAY_APP_URL bằng domain Railway của bạn, ví dụ: https://kldshop-production.up.railway.app)
```

#### **MailChimp (2 biến - Optional):**
```
Name: MailChimp__ApiKey
Value: YOUR_MAILCHIMP_API_KEY
(Chỉ cần nếu dùng Newsletter)

Name: MailChimp__ListId
Value: YOUR_MAILCHIMP_LIST_ID
(Chỉ cần nếu dùng Newsletter)
```

### Bước 3: Redeploy
- Railway sẽ **tự động redeploy** sau khi thêm variables
- Hoặc click **"Redeploy"** manually

---

## 🚀 Cách 2: Set qua Railway CLI (Nhanh hơn)

### Cài đặt Railway CLI:
```bash
npm install -g @railway/cli
railway login
railway link
```

### Set PayPal variables:
```bash
railway variables set PayPal__Enabled=true
railway variables set PayPal__Mode=sandbox
railway variables set PayPal__ClientId=AR4WJh1Rsp8btdXsUYJL4qMiIKjshAsIy4YNFQO5CQ2hPrwQnm4e-bzjxPTTQ2UFcv1nImkOw7RLIZAU
railway variables set PayPal__ClientSecret=EBs79_p8bCMLWveEJsWQUvaD1EqlV8JmjPvc6Hl8X_slSh_t1llJSrTVKg_SNPPeegAuiCa0oNDvsKqV
```

### Set VNPay variables:
```bash
railway variables set VNPay__Enabled=true
railway variables set VNPay__TmnCode=YOUR_VNPAY_TMN_CODE
railway variables set VNPay__HashSecret=YOUR_VNPAY_HASH_SECRET
railway variables set VNPay__PaymentUrl=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
railway variables set VNPay__ApiUrl=https://sandbox.vnpayment.vn/merchant_webapi/merchant.do
railway variables set VNPay__ReturnUrl=https://YOUR_RAILWAY_APP_URL/Order/PaymentReturn
```

### Trigger redeploy:
```bash
railway up
```

---

## 🔍 Kiểm tra Variables đã set đúng chưa

### Via Railway CLI:
```bash
railway variables
```

### Via Dashboard:
Vào **Variables tab** → Xem list tất cả variables

---

## ✅ Verify sau khi deploy

### 1. Check Railway Logs:
```
✅ "PayPal payment gateway initialized successfully"
✅ "VNPay payment gateway initialized successfully"
✅ "Payment page - PayPalClientId: AR4WJh1..."
```

### 2. Test Payments:
- **PayPal:** Button màu vàng xuất hiện
- **VNPay:** Redirect tới VNPay sandbox
- **Cash:** Order tạo thành công

---

## 📝 Notes quan trọng

### **VNPay ReturnUrl:**
- Phải match chính xác với Railway domain
- VNPay cần bạn **whitelist URL** trong merchant portal
- Format: `https://your-app.railway.app/Order/PaymentReturn`

### **Security:**
- ✅ Variables trên Railway được **encrypt**
- ✅ KHÔNG commit secrets vào Git
- ✅ Dùng `.gitignore` cho `appsettings.json`

### **Local vs Railway:**
- **Local:** Đọc từ `appsettings.json`
- **Railway:** Đọc từ Environment Variables (override appsettings.json)

---

## 🆘 Troubleshooting

### Issue: PayPal button không hiện
```bash
# Check logs
railway logs

# Tìm:
✅ "PayPalClientId: AR4WJh1..."
❌ "PayPalClientId: YOUR_PAYPAL_CLIENT_ID" → Variables chưa set!
```

### Issue: VNPay redirect lỗi
```bash
# Check ReturnUrl có đúng không:
railway variables | grep ReturnUrl

# Phải là Railway domain, không phải localhost!
```

### Issue: Variables không apply
```bash
# Redeploy manually
railway redeploy
```

---

## 🎯 Quick Copy-Paste cho Railway CLI

**Tất cả trong 1 lệnh (PayPal + VNPay):**

```bash
railway variables set \
  PayPal__Enabled=true \
  PayPal__Mode=sandbox \
  PayPal__ClientId=AR4WJh1Rsp8btdXsUYJL4qMiIKjshAsIy4YNFQO5CQ2hPrwQnm4e-bzjxPTTQ2UFcv1nImkOw7RLIZAU \
  PayPal__ClientSecret=EBs79_p8bCMLWveEJsWQUvaD1EqlV8JmjPvc6Hl8X_slSh_t1llJSrTVKg_SNPPeegAuiCa0oNDvsKqV \
  VNPay__Enabled=true \
  VNPay__PaymentUrl=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html \
  VNPay__ApiUrl=https://sandbox.vnpayment.vn/merchant_webapi/merchant.do
```

**Lưu ý:** Thay `YOUR_VNPAY_TMN_CODE`, `YOUR_VNPAY_HASH_SECRET`, và `YOUR_RAILWAY_APP_URL` bằng giá trị thật!

---

## 📊 Checklist hoàn chỉnh

- [ ] Set PayPal variables (4 biến)
- [ ] Set VNPay variables (6 biến)
- [ ] Update VNPay ReturnUrl với Railway domain
- [ ] Railway đã redeploy
- [ ] Check logs có "initialized successfully"
- [ ] Test PayPal payment
- [ ] Test VNPay payment
- [ ] Test Cash payment

---

**Setup xong rồi! Payment gateways sẽ hoạt động bình thường! 🎉**
