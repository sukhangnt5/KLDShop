using Microsoft.AspNetCore.Mvc;
using KLDShop.Data;
using KLDShop.Models;
using KLDShop.Helpers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace KLDShop.Controllers
{
    public class SeedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeedController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Seed test data
        public async Task<IActionResult> InitTestData()
        {
            try
            {
                // Xóa dữ liệu cũ nếu có
                var existingUsers = _context.Users.ToList();
                if (existingUsers.Any())
                {
                    _context.Users.RemoveRange(existingUsers);
                    await _context.SaveChangesAsync();
                }

                // Tạo mật khẩu hash
                string hashPassword = HashPassword("123456");
                string adminHash = HashPassword("admin123");

                // Thêm người dùng mẫu
                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@kldshop.com",
                        PasswordHash = adminHash,
                        FullName = "Admin User",
                        PhoneNumber = "0901234567",
                        Address = "123 Main St",
                        City = "Hà Nội",
                        IsAdmin = true,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new User
                    {
                        Username = "customer1",
                        Email = "customer1@kldshop.com",
                        PasswordHash = hashPassword,
                        FullName = "Nguyễn Văn A",
                        PhoneNumber = "0912345678",
                        Address = "456 Elm St",
                        City = "TP.HCM",
                        IsAdmin = false,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new User
                    {
                        Username = "customer2",
                        Email = "customer2@kldshop.com",
                        PasswordHash = hashPassword,
                        FullName = "Trần Thị B",
                        PhoneNumber = "0923456789",
                        Address = "789 Oak St",
                        City = "Đà Nẵng",
                        IsAdmin = false,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                };

                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    success = true, 
                    message = "Dữ liệu test đã được khởi tạo thành công",
                    testAccounts = new[] {
                        new { username = "admin", password = "admin123", role = "Admin" },
                        new { username = "customer1", password = "123456", role = "Customer" },
                        new { username = "customer2", password = "123456", role = "Customer" }
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Seed products
        public async Task<IActionResult> InitProducts()
        {
            try
            {
                // Xóa dữ liệu liên quan trước (OrderDetail, CartSession)
                var orderDetails = _context.Set<OrderDetail>().ToList();
                if (orderDetails.Any())
                {
                    _context.Set<OrderDetail>().RemoveRange(orderDetails);
                    await _context.SaveChangesAsync();
                }

                var cartSessions = _context.CartSessions.ToList();
                if (cartSessions.Any())
                {
                    _context.CartSessions.RemoveRange(cartSessions);
                    await _context.SaveChangesAsync();
                }

                // Sau đó xóa dữ liệu sản phẩm cũ
                var existingProducts = _context.Products.ToList();
                if (existingProducts.Any())
                {
                    _context.Products.RemoveRange(existingProducts);
                    await _context.SaveChangesAsync();
                }

                // Thêm danh mục trước
                var categories = new List<Category>
                {
                    new Category { CategoryName = "Máy Tính & Laptop", Description = "Laptop, máy tính để bàn", IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                    new Category { CategoryName = "Phụ Kiện & Thiết Bị", Description = "Chuột, bàn phím, webcam", IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                    new Category { CategoryName = "Màn Hình & Linh Kiện", Description = "Monitor, GPU, CPU, RAM, SSD", IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                    new Category { CategoryName = "Âm Thanh", Description = "Tai nghe, loa", IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
                };

                // Xóa danh mục cũ
                var existingCategories = _context.Categories.ToList();
                if (existingCategories.Any())
                {
                    _context.Categories.RemoveRange(existingCategories);
                    await _context.SaveChangesAsync();
                }

                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();

                // Thêm sản phẩm mẫu (giá VND)
                var products = new List<Product>
                {
                    new Product
                    {
                        ProductName = "Laptop Dell XPS 13",
                        Description = "Laptop cao cấp, thiết kế mỏng nhẹ, hiệu năng mạnh mẽ",
                        Price = 35000000m,
                        DiscountPrice = 29000000m,
                        Quantity = 50,
                        SKU = "SKU001",
                        Manufacturer = "Dell",
                        CategoryId = categories[0].CategoryId,
                        IsActive = true,
                        Views = 150,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "Chuột Logitech MX Master 3S",
                        Description = "Chuột không dây công nghiệp, tính năng cao cấp",
                        Price = 2500000m,
                        DiscountPrice = 2200000m,
                        Quantity = 100,
                        SKU = "SKU002",
                        Manufacturer = "Logitech",
                        CategoryId = categories[1].CategoryId,
                        IsActive = true,
                        Views = 80,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "Bàn phím Mechanical RGB",
                        Description = "Bàn phím cơ với đèn RGB, phím axis hot-swap",
                        Price = 3500000m,
                        DiscountPrice = 2900000m,
                        Quantity = 75,
                        SKU = "SKU003",
                        Manufacturer = "Keychron",
                        CategoryId = categories[1].CategoryId,
                        IsActive = true,
                        Views = 120,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "Monitor LG 27 inch 4K",
                        Description = "Màn hình 4K, 60Hz, IPS panel, màu sắc chính xác",
                        Price = 9000000m,
                        DiscountPrice = 7900000m,
                        Quantity = 30,
                        SKU = "SKU004",
                        Manufacturer = "LG",
                        CategoryId = categories[2].CategoryId,
                        IsActive = true,
                        Views = 200,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "Webcam Logitech C920",
                        Description = "Webcam Full HD 1080p, tự động lấy nét",
                        Price = 2000000m,
                        DiscountPrice = null,
                        Quantity = 60,
                        SKU = "SKU005",
                        Manufacturer = "Logitech",
                        CategoryId = categories[1].CategoryId,
                        IsActive = true,
                        Views = 90,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "SSD Samsung 970 EVO 1TB",
                        Description = "SSD NVMe tốc độ cao 1TB, đọc 3500MB/s",
                        Price = 5000000m,
                        DiscountPrice = 4200000m,
                        Quantity = 40,
                        SKU = "SKU006",
                        Manufacturer = "Samsung",
                        CategoryId = categories[2].CategoryId,
                        IsActive = true,
                        Views = 250,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "RAM Kingston Fury 16GB DDR4",
                        Description = "RAM DDR4 16GB, tần số 3600MHz, hiệu năng tốt",
                        Price = 1800000m,
                        DiscountPrice = 1500000m,
                        Quantity = 80,
                        SKU = "SKU007",
                        Manufacturer = "Kingston",
                        CategoryId = categories[2].CategoryId,
                        IsActive = true,
                        Views = 110,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "Tai nghe Sony WH-1000XM4",
                        Description = "Tai nghe chống ồn, âm thanh chất lượng cao",
                        Price = 8000000m,
                        DiscountPrice = 6900000m,
                        Quantity = 45,
                        SKU = "SKU008",
                        Manufacturer = "Sony",
                        CategoryId = categories[3].CategoryId,
                        IsActive = true,
                        Views = 175,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "GPU NVIDIA RTX 4060",
                        Description = "Card đồ họa RTX 4060, hiệu năng gaming tốt",
                        Price = 9500000m,
                        DiscountPrice = 8500000m,
                        Quantity = 25,
                        SKU = "SKU009",
                        Manufacturer = "NVIDIA",
                        CategoryId = categories[2].CategoryId,
                        IsActive = true,
                        Views = 300,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "Bộ nguồn Corsair 850W 80+ Gold",
                        Description = "Nguồn 850W chứng chỉ 80+ Gold, ổn định",
                        Price = 3200000m,
                        DiscountPrice = 2800000m,
                        Quantity = 55,
                        SKU = "SKU010",
                        Manufacturer = "Corsair",
                        CategoryId = categories[2].CategoryId,
                        IsActive = true,
                        Views = 95,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "Vỏ case Lian Li LANCOOL 215",
                        Description = "Vỏ case PC, thiết kế hiện đại, thoát nhiệt tốt",
                        Price = 1200000m,
                        DiscountPrice = null,
                        Quantity = 70,
                        SKU = "SKU011",
                        Manufacturer = "Lian Li",
                        CategoryId = categories[2].CategoryId,
                        IsActive = true,
                        Views = 88,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Product
                    {
                        ProductName = "CPU Intel Core i7-13700K",
                        Description = "Bộ xử lý Intel Core i7 thế hệ 13, hiệu năng cao",
                        Price = 15000000m,
                        DiscountPrice = 12500000m,
                        Quantity = 20,
                        SKU = "SKU012",
                        Manufacturer = "Intel",
                        CategoryId = categories[2].CategoryId,
                        IsActive = true,
                        Views = 280,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                };

                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    success = true, 
                    message = "Dữ liệu sản phẩm đã được khởi tạo thành công",
                    productsAdded = products.Count
                });
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? "";
                return Ok(new { success = false, message = "Lỗi: " + ex.Message, innerException = innerException });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Generate slugs for existing products
        [HttpGet]
        [Route("Seed/GenerateSlugs")]
        public async Task<IActionResult> GenerateSlugs()
        {
            try
            {
                var products = await _context.Products
                    .Where(p => string.IsNullOrEmpty(p.Slug))
                    .ToListAsync();

                int updated = 0;
                var results = new List<string>();

                foreach (var product in products)
                {
                    // Generate slug
                    var baseSlug = SlugHelper.GenerateSlug(product.ProductName);
                    product.Slug = SlugHelper.GenerateUniqueSlug(baseSlug, 
                        slug => _context.Products.Any(p => p.Slug == slug && p.ProductId != product.ProductId));

                    // Generate meta description if empty
                    if (string.IsNullOrEmpty(product.MetaDescription))
                    {
                        if (!string.IsNullOrEmpty(product.Description) && product.Description.Length > 160)
                        {
                            product.MetaDescription = product.Description.Substring(0, 157) + "...";
                        }
                        else if (!string.IsNullOrEmpty(product.Description))
                        {
                            product.MetaDescription = product.Description;
                        }
                        else
                        {
                            product.MetaDescription = $"Mua {product.ProductName} giá tốt tại KLDShop. Giao hàng nhanh, uy tín.";
                        }
                    }

                    // Generate meta keywords if empty
                    if (string.IsNullOrEmpty(product.MetaKeywords))
                    {
                        product.MetaKeywords = $"{product.ProductName}, mua online, KLDShop";
                    }

                    updated++;
                    results.Add($"✓ {product.ProductName} → {product.Slug}");
                }

                await _context.SaveChangesAsync();

                return Content($@"
<html>
<head>
    <title>Generate Slugs - KLDShop</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; background: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        h1 {{ color: #28a745; }}
        .success {{ background: #d4edda; padding: 15px; border-radius: 4px; margin: 20px 0; color: #155724; border: 1px solid #c3e6cb; }}
        .result {{ background: #f8f9fa; padding: 10px; margin: 5px 0; border-left: 3px solid #28a745; }}
        a {{ display: inline-block; margin-top: 20px; padding: 10px 20px; background: #007bff; color: white; text-decoration: none; border-radius: 4px; }}
        a:hover {{ background: #0056b3; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>✅ Slugs Generated Successfully!</h1>
        <div class='success'>
            <strong>Updated {updated} products</strong>
        </div>
        <h3>Results:</h3>
        {string.Join("\n", results.Select(r => $"<div class='result'>{r}</div>"))}
        <br>
        <a href='/'>← Back to Home</a>
        <a href='/Product/List'>View Products</a>
        <a href='/sitemap.xml' target='_blank'>View Sitemap</a>
    </div>
</body>
</html>
", "text/html");
            }
            catch (Exception ex)
            {
                return Content($@"
<html>
<head>
    <title>Error - Generate Slugs</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; background: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; }}
        .error {{ background: #f8d7da; padding: 15px; border-radius: 4px; color: #721c24; border: 1px solid #f5c6cb; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>❌ Error</h1>
        <div class='error'>
            {ex.Message}<br><br>
            <strong>Stack Trace:</strong><br>
            <pre>{ex.StackTrace}</pre>
        </div>
        <br>
        <a href='/'>← Back to Home</a>
    </div>
</body>
</html>
", "text/html");
            }
        }
    }
}
