# 🔍 Debug Google Analytics - Checklist

## ✅ **Bước 1: Kiểm tra Railway đã deploy chưa**

1. Vào Railway Dashboard: https://railway.app/dashboard
2. Chọn project KLDShop
3. Check **Deployments tab**
4. Deployment mới nhất phải có status: **Success** ✅
5. Kiểm tra logs không có error

---

## ✅ **Bước 2: Kiểm tra script đã load trên website**

### **2.1. Mở website Railway:**
```
https://kldshop-production.up.railway.app/
```

### **2.2. View Page Source (Ctrl+U hoặc Right Click → View Page Source):**

Tìm đoạn code này trong `<head>`:
```html
<!-- Google Analytics -->
<script async src="https://www.googletagmanager.com/gtag/js?id=G-NRMNMF2XQN"></script>
<script>
    window.dataLayer = window.dataLayer || [];
    function gtag(){dataLayer.push(arguments);}
    gtag('js', new Date());
    gtag('config', 'G-NRMNMF2XQN');
</script>
```

**Nếu KHÔNG thấy:**
- ❌ Railway chưa deploy code mới
- ❌ _Layout.cshtml chưa được update
- **Giải pháp:** Trigger redeploy trong Railway

**Nếu THẤY:**
- ✅ Code đã được deploy
- Chuyển sang bước 3

---

## ✅ **Bước 3: Kiểm tra script load trong Browser**

### **3.1. Mở Console (F12):**
Press **F12** → Tab **Console**

### **3.2. Check dataLayer:**
Gõ trong console:
```javascript
window.dataLayer
```

**Kết quả tốt:**
```javascript
✅ Array(3) [Object, Object, Object]
```

**Kết quả xấu:**
```javascript
❌ undefined
❌ []  (empty array)
```

### **3.3. Check gtag function:**
```javascript
typeof gtag
```

**Kết quả tốt:**
```javascript
✅ "function"
```

**Kết quả xấu:**
```javascript
❌ "undefined"
```

---

## ✅ **Bước 4: Kiểm tra Network Requests**

### **4.1. Mở Network tab:**
F12 → Tab **Network**

### **4.2. Reload trang:**
Press **Ctrl+R** hoặc **F5**

### **4.3. Filter requests:**
Trong filter box, gõ: `gtag` hoặc `google-analytics`

### **4.4. Tìm requests này:**

**Requests cần có:**
```
✅ gtag/js?id=G-NRMNMF2XQN        Status: 200
✅ collect?v=2&...                 Status: 200
✅ g/collect?v=2&...               Status: 200 (GA4)
```

**Nếu requests bị chặn:**
```
❌ (blocked:other)    → Ad blocker
❌ Status: 0          → CORS issue
❌ Status: 403        → Firewall/CSP
```

---

## ✅ **Bước 5: Disable Ad Blocker**

### **Ad Blocker thường chặn Google Analytics!**

**Các ad blocker phổ biến:**
- uBlock Origin
- Adblock Plus
- Brave Shield
- Privacy Badger

**Cách test:**
1. **Mở Incognito/Private mode** (Ctrl+Shift+N)
2. Vào website: `https://kldshop-production.up.railway.app/`
3. Mở F12 → Console → Check `window.dataLayer`
4. Nếu có data → Ad blocker đang chặn!

**Giải pháp:**
- Disable ad blocker cho domain của bạn
- Hoặc test trong Incognito mode

---

## ✅ **Bước 6: Test Real-time trong Google Analytics**

### **6.1. Mở Google Analytics Dashboard:**
```
https://analytics.google.com/
```

### **6.2. Chọn Property:**
- Click **KLDShop** property

### **6.3. Vào Realtime Report:**
- Sidebar: **Reports** → **Realtime**

### **6.4. Mở website trong tab mới:**
```
https://kldshop-production.up.railway.app/
```

### **6.5. Kiểm tra Realtime:**

**Kết quả tốt:**
```
✅ "1 user right now"
✅ Thấy page path: "/"
✅ Thấy Country, Device type
```

**Kết quả xấu:**
```
❌ "0 users right now"
```

---

## 🛠️ **Troubleshooting**

### **Issue 1: Script không load (Network Status: 0)**

**Nguyên nhân:**
- Ad blocker
- Browser extension chặn tracking
- CSP (Content Security Policy) chặn

**Giải pháp:**
```bash
# 1. Test trong Incognito mode
# 2. Disable all extensions
# 3. Try different browser
```

### **Issue 2: Script load nhưng không track (dataLayer empty)**

**Nguyên nhân:**
- Measurement ID sai
- Configuration error

**Giải pháp:**
```javascript
// Check config trong console
window.dataLayer

// Should see:
[
  ["js", Date],
  ["config", "G-NRMNMF2XQN"]
]
```

### **Issue 3: Realtime không thấy data**

**Nguyên nhân:**
- Do Not Track enabled trong browser
- Tracking blocked by firewall
- Railway domain chưa được whitelist

**Giải pháp:**
1. Check browser settings: Disable "Do Not Track"
2. Disable VPN/Proxy
3. Test từ device/network khác

### **Issue 4: "Chưa bật tính năng thu thập dữ liệu"**

**Nguyên nhân:**
- Script chưa được deploy lên production
- Railway deployment failed
- Cache issue

**Giải pháp:**
```bash
# 1. Check Railway deployment status
# 2. Hard refresh: Ctrl+Shift+R
# 3. Clear browser cache
# 4. Wait 5-10 minutes for propagation
```

---

## 🧪 **Manual Test Script**

### **Paste vào Browser Console:**

```javascript
// Test 1: Check if GA loaded
console.log('=== Google Analytics Debug ===');
console.log('dataLayer exists:', typeof window.dataLayer !== 'undefined');
console.log('dataLayer content:', window.dataLayer);
console.log('gtag function exists:', typeof gtag !== 'undefined');

// Test 2: Send test event
if (typeof gtag === 'function') {
    gtag('event', 'test_event', {
        'event_category': 'Debug',
        'event_label': 'Manual Console Test',
        'value': 1
    });
    console.log('✅ Test event sent to GA');
} else {
    console.error('❌ gtag function not available!');
}

// Test 3: Check network requests
console.log('Check Network tab for requests to:');
console.log('- googletagmanager.com/gtag/js');
console.log('- google-analytics.com/g/collect');
```

**Kết quả mong đợi:**
```javascript
=== Google Analytics Debug ===
dataLayer exists: true
dataLayer content: Array(3) [...]
gtag function exists: true
✅ Test event sent to GA
```

---

## 📊 **Expected Timeline**

- **Realtime:** Ngay lập tức (0-5 phút)
- **Overview reports:** 24-48 giờ
- **Full demographics:** 3-7 ngày

**Nếu sau 10 phút vẫn không thấy Realtime data → Có vấn đề!**

---

## ✅ **Quick Checklist**

- [ ] Railway deployment status: **Success**
- [ ] View Page Source: GA script có trong `<head>`
- [ ] Console: `window.dataLayer` có data
- [ ] Console: `typeof gtag` = "function"
- [ ] Network: requests tới google-analytics.com (Status: 200)
- [ ] Tested in Incognito mode (no ad blocker)
- [ ] Google Analytics Realtime: Thấy "1 user right now"

---

## 🆘 **Nếu tất cả đều OK nhưng vẫn không track:**

1. **Wait 5-10 minutes** - Sometimes có delay
2. **Clear cache và test lại**
3. **Test từ device khác** (mobile, tablet)
4. **Test từ network khác** (4G, WiFi khác)
5. **Contact Google Analytics support**

---

## 📞 **Support Resources**

- **GA4 Troubleshooting:** https://support.google.com/analytics/answer/9306384
- **Tag Assistant:** https://tagassistant.google.com/
- **DebugView in GA4:** Admin → DebugView (enable debug mode)

---

**Làm theo checklist này để tìm ra vấn đề! Gửi cho tôi kết quả của từng bước để tôi giúp debug!** 🔍
