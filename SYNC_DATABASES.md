# 🔄 Hướng dẫn đồng bộ SQL Server và PostgreSQL

## 🎯 Mục tiêu
- **SQL Server (SSMS)**: Development database (local)
- **PostgreSQL**: Test local trước khi deploy lên Railway

## 📋 Yêu cầu

### 1. Cài PostgreSQL local
- Download: https://www.postgresql.org/download/windows/
- Username mặc định: `postgres`
- Nhớ password bạn đã set khi cài

### 2. Cài pgAdmin (GUI cho PostgreSQL)
- Đi kèm với PostgreSQL installer
- Hoặc download riêng: https://www.pgadmin.org/

## 🚀 Các bước đồng bộ

### Bước 1: Cấu hình Connection Strings

File `appsettings.Development.json` đã được tạo với 2 connection strings:
- `DefaultConnection`: SQL Server (SSMS)
- `PostgreSQLConnection`: PostgreSQL local

**Cập nhật password PostgreSQL:**
```json
"PostgreSQLConnection": "Host=localhost;Port=5432;Database=KLDShop;Username=postgres;Password=YOUR_PASSWORD_HERE;"
```

### Bước 2: Tạo Database trên PostgreSQL

**Cách 1: Dùng pgAdmin**
1. Mở pgAdmin
2. Connect tới PostgreSQL Server
3. Right-click "Databases" → Create → Database
4. Tên: `KLDShop`
5. Click Save

**Cách 2: Dùng SQL**
```sql
CREATE DATABASE "KLDShop"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    CONNECTION LIMIT = -1;
```

### Bước 3: Chạy Migrations cho PostgreSQL

**Option A: Thay đổi tạm thời để chạy migrations**

1. Mở file `appsettings.json`
2. Thêm tạm connection string PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=KLDShop;Username=postgres;Password=YOUR_PASSWORD;"
  }
}
```

3. Chạy migrations:
```bash
dotnet ef database update
```

4. Sau khi xong, đổi lại về SQL Server

**Option B: Dùng command line với connection string**

```bash
dotnet ef database update --connection "Host=localhost;Port=5432;Database=KLDShop;Username=postgres;Password=YOUR_PASSWORD;"
```

### Bước 4: Export/Import Data

#### Method 1: Dùng Entity Framework (Khuyên dùng)

Tạo script C# để copy data:

```bash
# Tạo file script
```

**File: `tmp_rovodev_SyncData.ps1`** (PowerShell script)

```powershell
# Script để sync data từ SQL Server sang PostgreSQL
Write-Host "=== Data Sync: SQL Server -> PostgreSQL ===" -ForegroundColor Green

# Backup từ SQL Server
Write-Host "`nStep 1: Exporting data from SQL Server..." -ForegroundColor Yellow
dotnet run -- --export-data --source sqlserver --output data_backup.json

# Import vào PostgreSQL  
Write-Host "`nStep 2: Importing data to PostgreSQL..." -ForegroundColor Yellow
dotnet run -- --import-data --target postgresql --input data_backup.json

Write-Host "`nSync completed!" -ForegroundColor Green
```

#### Method 2: Manual Export/Import qua SQL

**Export từ SQL Server:**
```sql
-- Right-click database trong SSMS
-- Tasks → Generate Scripts
-- Chọn "Schema and data"
-- Save to file
```

**Convert SQL Server → PostgreSQL:**
- Dùng tool: https://www.convert-in.com/mss2pgs.htm
- Hoặc: https://www.sqlines.com/online

**Import vào PostgreSQL:**
```bash
psql -U postgres -d KLDShop -f converted_script.sql
```

#### Method 3: Dùng Seed Data (Đơn giản nhất)

Bạn có sẵn `SeedController.cs`, dùng nó:

1. **Với SQL Server:**
```
http://localhost:5000/Seed/SeedAll
```

2. **Chuyển sang PostgreSQL** (đổi connection string)

3. **Chạy lại Seed:**
```
http://localhost:5000/Seed/SeedAll
```

## 🔧 Script tự động đồng bộ

### Tạo helper script để switch databases

**File: `switch-database.ps1`**

```powershell
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("sqlserver", "postgresql")]
    [string]$Database
)

$appsettingsPath = "appsettings.json"
$appsettingsContent = Get-Content $appsettingsPath -Raw | ConvertFrom-Json

if ($Database -eq "sqlserver") {
    $appsettingsContent.ConnectionStrings.DefaultConnection = "Server=localhost;Database=KLDShop;Trusted_Connection=true;TrustServerCertificate=true;"
    Write-Host "Switched to SQL Server" -ForegroundColor Green
} else {
    $password = Read-Host "Enter PostgreSQL password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    
    $appsettingsContent.ConnectionStrings.DefaultConnection = "Host=localhost;Port=5432;Database=KLDShop;Username=postgres;Password=$plainPassword;"
    Write-Host "Switched to PostgreSQL" -ForegroundColor Green
}

$appsettingsContent | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
Write-Host "Connection string updated in appsettings.json"
```

**Sử dụng:**
```powershell
# Switch sang SQL Server
.\switch-database.ps1 -Database sqlserver

# Switch sang PostgreSQL
.\switch-database.ps1 -Database postgresql
```

## 📊 So sánh các phương pháp

| Phương pháp | Ưu điểm | Nhược điểm | Độ khó |
|------------|---------|------------|--------|
| **EF Migrations** | Tự động, chuẩn | Chỉ có schema, không có data | ⭐ Dễ |
| **Seed Controller** | Có data mẫu sẵn | Data cố định | ⭐ Dễ nhất |
| **SQL Export/Import** | Full data thật | Cần convert syntax | ⭐⭐⭐ Khó |
| **Custom C# Script** | Linh hoạt, chính xác | Phải code | ⭐⭐ Trung bình |

## 🎯 Workflow khuyên dùng

### Development (Local):
```
┌─────────────────┐
│  SQL Server     │ ← Main development DB (SSMS)
│  (SSMS)         │
└─────────────────┘
        ↓
   Work here daily
        ↓
   Khi cần test PostgreSQL:
        ↓
┌─────────────────┐
│  PostgreSQL     │ ← Test trước khi deploy
│  (Local)        │
└─────────────────┘
        ↓
   Chạy migrations + seed
        ↓
   Test xem có lỗi gì không
        ↓
┌─────────────────┐
│  PostgreSQL     │ ← Deploy production
│  (Railway)      │
└─────────────────┘
```

## 🔍 Kiểm tra đồng bộ

### Check Schema:
```sql
-- SQL Server
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- PostgreSQL  
SELECT tablename FROM pg_tables WHERE schemaname = 'public';
```

### Check Data Count:
```sql
-- SQL Server
SELECT 'Users' as TableName, COUNT(*) as Count FROM [User]
UNION ALL
SELECT 'Products', COUNT(*) FROM Product
UNION ALL
SELECT 'Categories', COUNT(*) FROM Category;

-- PostgreSQL (same query, works on both)
```

## ⚠️ Lưu ý quan trọng

### 1. Syntax khác biệt:
| Feature | SQL Server | PostgreSQL |
|---------|-----------|------------|
| String concat | `+` | `\|\|` |
| Top N | `TOP N` | `LIMIT N` |
| Identity | `IDENTITY(1,1)` | `SERIAL` |
| Datetime | `GETDATE()` | `NOW()` |
| Case sensitive | No | **Yes** (table/column names) |

### 2. Entity Framework sẽ handle hầu hết:
- ✅ EF Core tự động generate đúng SQL syntax
- ✅ Migrations works cho cả 2 databases
- ⚠️ Raw SQL queries cần viết riêng hoặc dùng EF LINQ

### 3. Testing:
- Test kỹ trên PostgreSQL local trước
- Đặc biệt kiểm tra: DateTime, String operations, Stored Procedures (nếu có)

## 🆘 Troubleshooting

### Lỗi: "password authentication failed"
```bash
# Reset PostgreSQL password
psql -U postgres
ALTER USER postgres PASSWORD 'new_password';
```

### Lỗi: "relation does not exist"
- PostgreSQL case-sensitive, check table names
- Chạy lại migrations: `dotnet ef database update`

### Lỗi: "column does not exist"
- Check schema differences
- Re-run migrations on PostgreSQL

## 📚 Resources

- [PostgreSQL Download](https://www.postgresql.org/download/)
- [pgAdmin 4](https://www.pgadmin.org/)
- [EF Core PostgreSQL Provider](https://www.npgsql.org/efcore/)
- [SQL Server to PostgreSQL Migration](https://wiki.postgresql.org/wiki/Things_to_find_out_about_when_moving_from_MySQL_to_PostgreSQL)

---

**Tóm lại**: Dùng SQL Server cho development hàng ngày, test PostgreSQL local trước khi deploy lên Railway! 🚀
