# 📊 Google Analytics Setup Guide

## 🎯 Bước 1: Tạo Google Analytics Account

### 1.1. Truy cập Google Analytics:
- Vào: https://analytics.google.com/
- Đăng nhập bằng Google Account

### 1.2. Tạo Account mới:
1. Click **"Start measuring"** hoặc **"Admin"** (góc dưới bên trái)
2. Click **"+ Create Account"**
3. Điền thông tin:
   - **Account name**: KLDShop (hoặc tên bạn muốn)
   - **Account data sharing settings**: Chọn tùy ý
4. Click **"Next"**

### 1.3. Tạo Property:
1. **Property name**: KLDShop Website
2. **Reporting time zone**: (UTC+07:00) Bangkok, Hanoi, Jakarta
3. **Currency**: Vietnamese Dong (VND)
4. Click **"Next"**

### 1.4. Business information:
1. **Industry category**: Retail / E-commerce
2. **Business size**: Small (1-10 employees)
3. Click **"Next"**

### 1.5. Business objectives:
- Chọn: ✅ **Examine user behavior**
- Chọn: ✅ **Measure advertising ROI**
- Click **"Create"**

### 1.6. Accept Terms of Service:
- Đọc và chấp nhận điều khoản
- Click **"I Accept"**

---

## 🔑 Bước 2: Lấy Measurement ID

### 2.1. Chọn Platform:
1. Màn hình "Choose a platform"
2. Click **"Web"**

### 2.2. Set up data stream:
1. **Website URL**: `https://your-railway-app.railway.app`
2. **Stream name**: KLDShop Website
3. Click **"Create stream"**

### 2.3. Copy Measurement ID:
Bạn sẽ thấy:
```
Measurement ID: G-XXXXXXXXXX
```
→ **Copy ID này!** (Dạng `G-` theo sau là 10 ký tự)

---

## 🛠️ Bước 3: Tích hợp vào Website

### Option 1: Dùng appsettings.json (Khuyến nghị)

Thêm vào `appsettings.json`:
```json
{
  "GoogleAnalytics": {
    "MeasurementId": "G-XXXXXXXXXX"
  }
}
```

### Option 2: Dùng Railway Environment Variable

Trong Railway Dashboard → Variables:
```
GoogleAnalytics__MeasurementId=G-XXXXXXXXXX
```

---

## 📝 Bước 4: Code Integration

Code sẽ được thêm vào `_Layout.cshtml` trong thẻ `<head>`:

```html
<!-- Google Analytics -->
@if (!string.IsNullOrEmpty(ViewBag.GoogleAnalyticsMeasurementId))
{
    <script async src="https://www.googletagmanager.com/gtag/js?id=@ViewBag.GoogleAnalyticsMeasurementId"></script>
    <script>
        window.dataLayer = window.dataLayer || [];
        function gtag(){dataLayer.push(arguments);}
        gtag('js', new Date());
        gtag('config', '@ViewBag.GoogleAnalyticsMeasurementId');
    </script>
}
```

Controller sẽ pass MeasurementId vào ViewBag trong `_Layout.cshtml`.

---

## 📊 Bước 5: Verify Setup

### 5.1. Real-time Test:
1. Vào Google Analytics Dashboard
2. Click **"Reports"** → **"Realtime"**
3. Mở website của bạn trong tab mới
4. Bạn sẽ thấy **"1 user right now"** trong Realtime report! ✅

### 5.2. Check trong Console:
Mở F12 → Console, search for `gtag`:
```javascript
✅ gtag('config', 'G-XXXXXXXXXX');
```

### 5.3. Check Network Tab:
F12 → Network → Filter: `google-analytics.com`
```
✅ collect?v=2&tid=G-XXXXXXXXXX... (Status: 200)
```

---

## 🎯 Metrics sẽ track được:

### Tự động:
- ✅ **Page views**: Số lượt xem trang
- ✅ **Sessions**: Số phiên truy cập
- ✅ **Users**: Số người dùng (unique)
- ✅ **Bounce rate**: Tỷ lệ thoát
- ✅ **Session duration**: Thời gian trên site
- ✅ **Traffic sources**: Nguồn traffic (organic, direct, referral)
- ✅ **Device category**: Desktop, Mobile, Tablet
- ✅ **Location**: Quốc gia, thành phố
- ✅ **Browser**: Chrome, Firefox, Safari...

### E-commerce tracking (Cần config thêm):
- Purchase events
- Add to cart events
- Product views
- Revenue tracking

---

## 🚀 Advanced Features (Optional)

### 1. E-commerce Tracking:
```javascript
// Add to cart event
gtag('event', 'add_to_cart', {
  currency: 'VND',
  value: 100000,
  items: [{
    item_id: 'SKU123',
    item_name: 'Product Name',
    price: 100000,
    quantity: 1
  }]
});

// Purchase event
gtag('event', 'purchase', {
  transaction_id: 'ORD-123',
  value: 500000,
  currency: 'VND',
  items: [...]
});
```

### 2. Custom Events:
```javascript
// Newsletter signup
gtag('event', 'newsletter_signup', {
  method: 'footer_form'
});

// Search
gtag('event', 'search', {
  search_term: 'áo thun'
});
```

### 3. User ID Tracking:
```javascript
gtag('config', 'G-XXXXXXXXXX', {
  'user_id': 'USER_12345'
});
```

---

## 🔒 Privacy & GDPR

### Cookie Consent (Nên có):
Nếu có khách hàng từ EU, cần:
1. **Cookie banner** để xin phép
2. **Disable tracking** nếu user từ chối
3. **Privacy policy** page

Code tắt tracking:
```javascript
window['ga-disable-G-XXXXXXXXXX'] = true;
```

---

## 📈 Reports bạn nên xem:

### 1. **Realtime Report**:
- Xem ai đang online
- Trang nào đang được xem
- Nguồn traffic real-time

### 2. **Acquisition Report**:
- Traffic sources (Google, Facebook, Direct...)
- Campaign performance
- SEO performance

### 3. **Engagement Report**:
- Page views
- Event counts
- Conversions

### 4. **User Report**:
- Demographics (age, gender)
- Interests
- Technology (browser, OS, device)
- Location

### 5. **E-commerce Report** (Nếu setup):
- Purchase revenue
- Product performance
- Checkout behavior

---

## 🛠️ Troubleshooting

### Issue 1: Real-time không thấy data
**Nguyên nhân:**
- Measurement ID sai
- Script chưa load
- Ad blocker chặn

**Giải pháp:**
- Check console logs
- Disable ad blocker
- Check Network tab

### Issue 2: Data không chính xác
**Nguyên nhân:**
- Bot traffic
- Internal traffic (team test)

**Giải pháp:**
- Filter internal traffic bằng IP
- Enable bot filtering trong GA settings

### Issue 3: E-commerce không track
**Nguyên nhân:**
- Chưa enable Enhanced Measurement
- Event code chưa đúng

**Giải pháp:**
- Admin → Data Streams → Enhanced Measurement: Enable
- Check event format theo docs

---

## 📚 Resources

- **GA4 Documentation**: https://support.google.com/analytics/
- **E-commerce Setup**: https://developers.google.com/analytics/devguides/collection/ga4/ecommerce
- **Event Reference**: https://developers.google.com/analytics/devguides/collection/ga4/reference/events
- **Demo Account**: https://support.google.com/analytics/answer/6367342

---

## ✅ Checklist

- [ ] Tạo Google Analytics account
- [ ] Tạo Property và Data Stream
- [ ] Copy Measurement ID
- [ ] Thêm ID vào appsettings.json hoặc Railway variables
- [ ] Code integration vào _Layout.cshtml
- [ ] Deploy lên Railway
- [ ] Test realtime tracking
- [ ] Setup e-commerce events (optional)
- [ ] Add cookie consent banner (optional)
- [ ] Review reports weekly

---

**Setup xong! Website của bạn sẽ có Analytics tracking đầy đủ! 📊**
