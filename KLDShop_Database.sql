-- ============================================================================
-- KLDShop Database Schema
-- Database cho hệ thống quản lý bán hàng trực tuyến
-- ============================================================================

-- Xóa database nếu tồn tại (tuỳ chọn)
-- DROP DATABASE IF EXISTS KLDShop;

-- Tạo database
-- CREATE DATABASE KLDShop;
-- USE KLDShop;

-- ============================================================================
-- 1. BẢNG USER (Người dùng)
-- ============================================================================
CREATE TABLE [User] (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255),
    City NVARCHAR(100),
    District NVARCHAR(100),
    Ward NVARCHAR(100),
    PostalCode NVARCHAR(10),
    Gender NVARCHAR(10), -- 'Male', 'Female', 'Other'
    DateOfBirth DATE,
    IsActive BIT NOT NULL DEFAULT 1,
    IsAdmin BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    LastLoginAt DATETIME
);

-- ============================================================================
-- 2. BẢNG PRODUCT (Sản phẩm)
-- ============================================================================
CREATE TABLE Product (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    DiscountPrice DECIMAL(18, 2),
    Quantity INT NOT NULL DEFAULT 0,
    CategoryId INT,
    Image NVARCHAR(255),
    SKU NVARCHAR(50) UNIQUE,
    Weight DECIMAL(8, 2), -- kg
    Dimensions NVARCHAR(100), -- kích thước (dài x rộng x cao)
    Manufacturer NVARCHAR(100),
    WarrantyPeriod INT, -- tháng
    IsActive BIT NOT NULL DEFAULT 1,
    Views INT DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_Price CHECK (Price > 0),
    CONSTRAINT CK_Quantity CHECK (Quantity >= 0)
);

-- ============================================================================
-- 3. BẢNG ORDER (Đơn hàng)
-- ============================================================================
CREATE TABLE [Order] (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 2) NOT NULL,
    DiscountAmount DECIMAL(18, 2) DEFAULT 0,
    TaxAmount DECIMAL(18, 2) DEFAULT 0,
    ShippingCost DECIMAL(18, 2) DEFAULT 0,
    FinalAmount DECIMAL(18, 2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled'
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Unpaid', -- 'Unpaid', 'Paid', 'Refunded'
    ShippingAddress NVARCHAR(255) NOT NULL,
    ShippingCity NVARCHAR(100) NOT NULL,
    ShippingDistrict NVARCHAR(100),
    ShippingWard NVARCHAR(100),
    ShippingPostalCode NVARCHAR(10),
    ShippingPhone NVARCHAR(20),
    ShippingRecipient NVARCHAR(100) NOT NULL,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ShippedAt DATETIME,
    DeliveredAt DATETIME,
    CONSTRAINT FK_Order_User FOREIGN KEY (UserId) REFERENCES [User](UserId),
    CONSTRAINT CK_FinalAmount CHECK (FinalAmount > 0)
);

-- ============================================================================
-- 4. BẢNG ORDER DETAIL (Chi tiết đơn hàng)
-- ============================================================================
CREATE TABLE OrderDetail (
    OrderDetailId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    DiscountPrice DECIMAL(18, 2),
    TotalPrice DECIMAL(18, 2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderId) REFERENCES [Order](OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductId) REFERENCES Product(ProductId),
    CONSTRAINT CK_OrderDetail_Quantity CHECK (Quantity > 0)
);

-- ============================================================================
-- 5. BẢNG PAYMENT (Thanh toán)
-- ============================================================================
CREATE TABLE Payment (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL UNIQUE,
    PaymentMethod NVARCHAR(50) NOT NULL, -- 'CreditCard', 'DebitCard', 'BankTransfer', 'Cash', 'E-Wallet'
    PaymentAmount DECIMAL(18, 2) NOT NULL,
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Completed', 'Failed', 'Cancelled'
    TransactionId NVARCHAR(100),
    TransactionDate DATETIME,
    PaymentDate DATETIME,
    CardNumber NVARCHAR(20), -- lưu 4 chữ số cuối
    BankName NVARCHAR(100),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Payment_Order FOREIGN KEY (OrderId) REFERENCES [Order](OrderId),
    CONSTRAINT CK_PaymentAmount CHECK (PaymentAmount > 0)
);

-- ============================================================================
-- 6. CÁC CHỈ MỤC (INDEXES)
-- ============================================================================
CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_Username ON [User](Username);
CREATE INDEX IX_User_IsActive ON [User](IsActive);

CREATE INDEX IX_Product_CategoryId ON Product(CategoryId);
CREATE INDEX IX_Product_IsActive ON Product(IsActive);
CREATE INDEX IX_Product_SKU ON Product(SKU);
CREATE INDEX IX_Product_CreatedAt ON Product(CreatedAt);

CREATE INDEX IX_Order_UserId ON [Order](UserId);
CREATE INDEX IX_Order_OrderNumber ON [Order](OrderNumber);
CREATE INDEX IX_Order_Status ON [Order](Status);
CREATE INDEX IX_Order_PaymentStatus ON [Order]([PaymentStatus]);
CREATE INDEX IX_Order_OrderDate ON [Order](OrderDate);

CREATE INDEX IX_OrderDetail_OrderId ON OrderDetail(OrderId);
CREATE INDEX IX_OrderDetail_ProductId ON OrderDetail(ProductId);

CREATE INDEX IX_Payment_OrderId ON Payment(OrderId);
CREATE INDEX IX_Payment_PaymentStatus ON Payment(PaymentStatus);
CREATE INDEX IX_Payment_TransactionId ON Payment(TransactionId);

-- ============================================================================
-- 7. DỮ LIỆU MẪU (SAMPLE DATA) - Tuỳ chọn
-- ============================================================================

-- Thêm người dùng mẫu
INSERT INTO [User] (Username, Email, PasswordHash, FullName, PhoneNumber, Address, City, IsAdmin)
VALUES 
    (N'admin', N'admin@kldshop.com', N'hashed_password_123', N'Admin User', N'0901234567', N'123 Main St', N'Hà Nội', 1),
    (N'customer1', N'customer1@kldshop.com', N'hashed_password_456', N'Nguyễn Văn A', N'0912345678', N'456 Elm St', N'TP.HCM', 0),
    (N'customer2', N'customer2@kldshop.com', N'hashed_password_789', N'Trần Thị B', N'0923456789', N'789 Oak St', N'Đà Nẵng', 0);

-- Thêm sản phẩm mẫu
INSERT INTO Product (ProductName, Description, Price, Quantity, SKU, Manufacturer, IsActive)
VALUES 
    (N'Laptop Dell XPS 13', N'Laptop cao cấp, thiết kế mỏng nhẹ', 1299.99, 50, N'SKU001', N'Dell', 1),
    (N'Chuột Logitech MX Master', N'Chuột không dây công nghiệp', 99.99, 100, N'SKU002', N'Logitech', 1),
    (N'Bàn phím Mechanical RGB', N'Bàn phím cơ với đèn RGB', 149.99, 75, N'SKU003', N'Unknown', 1),
    (N'Monitor LG 27 inch 4K', N'Màn hình 4K, 60Hz', 399.99, 30, N'SKU004', N'LG', 1),
    (N'Webcam Logitech C920', N'Webcam Full HD 1080p', 79.99, 60, N'SKU005', N'Logitech', 1);
GO

-- ============================================================================
-- 8. STORED PROCEDURES (Tuỳ chọn)
-- ============================================================================

-- Procedure: Tạo đơn hàng mới
CREATE PROCEDURE sp_CreateOrder
    @UserId INT,
    @TotalAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2) = 0,
    @TaxAmount DECIMAL(18,2) = 0,
    @ShippingCost DECIMAL(18,2) = 0,
    @ShippingAddress NVARCHAR(255),
    @ShippingCity NVARCHAR(100),
    @ShippingRecipient NVARCHAR(100),
    @OrderId INT OUTPUT
AS
BEGIN
    DECLARE @FinalAmount DECIMAL(18,2);
    SET @FinalAmount = @TotalAmount - @DiscountAmount + @TaxAmount + @ShippingCost;
    
    INSERT INTO [Order] (UserId, OrderNumber, TotalAmount, DiscountAmount, TaxAmount, ShippingCost, FinalAmount, ShippingAddress, ShippingCity, ShippingRecipient)
    VALUES (@UserId, CONCAT('ORD-', FORMAT(GETDATE(), 'yyyyMMddHHmmss')), @TotalAmount, @DiscountAmount, @TaxAmount, @ShippingCost, @FinalAmount, @ShippingAddress, @ShippingCity, @ShippingRecipient);
    
    SET @OrderId = SCOPE_IDENTITY();
END;
GO

-- Procedure: Cập nhật trạng thái đơn hàng
CREATE PROCEDURE sp_UpdateOrderStatus
    @OrderId INT,
    @Status NVARCHAR(50)
AS
BEGIN
    UPDATE [Order]
    SET Status = @Status, UpdatedAt = GETDATE()
    WHERE OrderId = @OrderId;
END;
GO

-- Procedure: Lấy tổng doanh thu theo tháng
CREATE PROCEDURE sp_GetMonthlySalesReport
    @Year INT,
    @Month INT
AS
BEGIN
    SELECT 
        COUNT(DISTINCT OrderId) AS TotalOrders,
        SUM(FinalAmount) AS TotalRevenue,
        AVG(FinalAmount) AS AvgOrderValue,
        COUNT(DISTINCT UserId) AS UniqueCustomers
    FROM [Order]
    WHERE YEAR(OrderDate) = @Year
        AND MONTH(OrderDate) = @Month
        AND Status != 'Cancelled';
END;
GO

-- ============================================================================
-- END OF SCHEMA
-- ============================================================================
