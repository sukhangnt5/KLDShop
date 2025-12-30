using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KLDShop.Data;
using KLDShop.Models;
using KLDShop.Services;
using System.Text;

namespace KLDShop.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MailChimpService _mailChimpService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            MailChimpService mailChimpService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _mailChimpService = mailChimpService;
            _logger = logger;
        }

        // Kiểm tra xem người dùng có phải admin không
        private bool IsAdmin()
        {
            var isAdmin = HttpContext.Session.GetInt32("IsAdmin");
            return isAdmin == 1;
        }

        private IActionResult RedirectIfNotAdmin()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            return null!;
        }

        // GET: Dashboard
        public IActionResult Dashboard()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var totalUsers = _context.Users.Count();
            var totalProducts = _context.Products.Count();
            var totalOrders = _context.Orders.Count();
            var totalRevenue = _context.Orders.Where(o => o.Status != "Cancelled").Sum(o => (decimal?)o.FinalAmount) ?? 0;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue.ToString("#,##0");
            ViewBag.PendingOrders = _context.Orders.Count(o => o.Status == "Pending");

            return View();
        }

        // GET: Quản lý người dùng
        public async Task<IActionResult> Users(int page = 1)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            const int pageSize = 10;
            var query = _context.Users.OrderByDescending(u => u.CreatedAt);
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(users);
        }

        // GET: Quản lý sản phẩm
        public async Task<IActionResult> Products(int page = 1)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            const int pageSize = 10;
            var query = _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt);
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }

        // GET: Quản lý đơn hàng
        public async Task<IActionResult> Orders(int page = 1)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            const int pageSize = 10;
            var query = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate);
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        // POST: Cập nhật trạng thái đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });
            }

            order.Status = status;
            if (status == "Shipped")
            {
                order.ShippedAt = DateTime.UtcNow;
            }
            else if (status == "Delivered")
            {
                order.DeliveredAt = DateTime.UtcNow;
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        // POST: Cập nhật trạng thái sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateProductStatus(int productId, bool isActive)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            product.IsActive = isActive;
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        // GET: Create Product
        public async Task<IActionResult> CreateProduct()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View();
        }

        // POST: Create Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, List<ProductImage> productImages)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            if (ModelState.IsValid)
            {
                try
                {
                    // Generate slug from product name
                    product.Slug = Helpers.SlugHelper.GenerateSlug(product.ProductName);
                    product.CreatedAt = DateTime.UtcNow;
                    product.UpdatedAt = DateTime.UtcNow;
                    product.Views = 0;

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    // Add product images if provided
                    if (productImages != null && productImages.Any())
                    {
                        foreach (var img in productImages.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl)))
                        {
                            img.ProductId = product.ProductId;
                            img.CreatedAt = DateTime.UtcNow;
                            _context.ProductImages.Add(img);
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction(nameof(Products));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi thêm sản phẩm");
                }
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(product);
        }

        // GET: Edit Product
        public async Task<IActionResult> EditProduct(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);
                
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(product);
        }

        // POST: Edit Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, List<ProductImage> productImages)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            if (id != product.ProductId)
            {
                TempData["Error"] = "ID không hợp lệ";
                return RedirectToAction(nameof(Products));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products
                        .Include(p => p.ProductImages)
                        .FirstOrDefaultAsync(p => p.ProductId == id);
                        
                    if (existingProduct == null)
                    {
                        TempData["Error"] = "Sản phẩm không tồn tại";
                        return RedirectToAction(nameof(Products));
                    }

                    // Update fields
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Slug = Helpers.SlugHelper.GenerateSlug(product.ProductName);
                    existingProduct.Description = product.Description;
                    existingProduct.MetaDescription = product.MetaDescription;
                    existingProduct.MetaKeywords = product.MetaKeywords;
                    existingProduct.Price = product.Price;
                    existingProduct.DiscountPrice = product.DiscountPrice;
                    existingProduct.Quantity = product.Quantity;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.Image = product.Image;
                    existingProduct.SKU = product.SKU;
                    existingProduct.Weight = product.Weight;
                    existingProduct.Dimensions = product.Dimensions;
                    existingProduct.Manufacturer = product.Manufacturer;
                    existingProduct.WarrantyPeriod = product.WarrantyPeriod;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.UpdatedAt = DateTime.UtcNow;

                    // Update product images
                    if (productImages != null)
                    {
                        foreach (var img in productImages)
                        {
                            if (!string.IsNullOrWhiteSpace(img.ImageUrl))
                            {
                                // Update existing or add new
                                if (img.ProductImageId > 0)
                                {
                                    var existingImg = existingProduct.ProductImages.FirstOrDefault(i => i.ProductImageId == img.ProductImageId);
                                    if (existingImg != null)
                                    {
                                        existingImg.ImageUrl = img.ImageUrl;
                                        existingImg.DisplayOrder = img.DisplayOrder;
                                    }
                                }
                                else
                                {
                                    // Add new image
                                    existingProduct.ProductImages.Add(new ProductImage
                                    {
                                        ProductId = id,
                                        ImageUrl = img.ImageUrl,
                                        DisplayOrder = img.DisplayOrder,
                                        CreatedAt = DateTime.UtcNow
                                    });
                                }
                            }
                            else if (img.ProductImageId > 0)
                            {
                                // Delete image if URL is empty
                                var imgToDelete = existingProduct.ProductImages.FirstOrDefault(i => i.ProductImageId == img.ProductImageId);
                                if (imgToDelete != null)
                                {
                                    _context.ProductImages.Remove(imgToDelete);
                                }
                            }
                        }
                    }

                    _context.Products.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Products));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm");
                }
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(product);
        }

        // POST: Delete Product
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                // Check if product is in any orders
                var hasOrders = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
                if (hasOrders)
                {
                    return Json(new { success = false, message = "Không thể xóa sản phẩm đã có trong đơn hàng. Vui lòng vô hiệu hóa thay vì xóa." });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa sản phẩm" });
            }
        }

        // POST: Deactivate user
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại" });
            }

            if (user.IsAdmin)
            {
                return Json(new { success = false, message = "Không thể vô hiệu hóa admin" });
            }

            user.IsActive = !user.IsActive;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Người dùng đã {(user.IsActive ? "kích hoạt" : "vô hiệu hóa")}" });
        }

        // GET: Thống kê
        public async Task<IActionResult> Statistics()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            // Doanh thu theo tháng
            var monthlyRevenue = await _context.Orders
                .Where(o => o.Status != "Cancelled")
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(o => o.FinalAmount),
                    Orders = g.Count()
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(12)
                .ToListAsync();

            // Top sản phẩm
            var topProducts = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product!.ProductName,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.TotalPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToListAsync();

            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TopProducts = topProducts;

            return View();
        }

        // GET: Newsletter Management
        public async Task<IActionResult> Newsletter()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var subscribers = await _context.Newsletters
                .OrderByDescending(n => n.SubscribedAt)
                .ToListAsync();

            ViewBag.TotalSubscribers = subscribers.Count;
            ViewBag.ActiveSubscribers = subscribers.Count(s => s.IsActive);
            ViewBag.TodaySubscribers = subscribers.Count(s => s.SubscribedAt.Date == DateTime.Today);
            ViewBag.TotalCampaigns = await _context.EmailCampaigns.CountAsync();

            return View(subscribers);
        }

        // POST: Delete Subscriber
        [HttpPost]
        public async Task<IActionResult> DeleteSubscriber(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var subscriber = await _context.Newsletters.FindAsync(id);
                if (subscriber == null)
                {
                    return Json(new { success = false, message = "Subscriber không tồn tại" });
                }

                _context.Newsletters.Remove(subscriber);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa subscriber" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscriber");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        // POST: Unsubscribe User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsubscribeUser(string email)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            try
            {
                var subscriber = await _context.Newsletters
                    .FirstOrDefaultAsync(n => n.Email == email);

                if (subscriber != null)
                {
                    await _mailChimpService.UnsubscribeAsync(email);
                    subscriber.IsActive = false;
                    subscriber.Status = "unsubscribed";
                    subscriber.UnsubscribedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đã hủy đăng ký thành công";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing user");
                TempData["Error"] = "Có lỗi xảy ra";
            }

            return RedirectToAction(nameof(Newsletter));
        }

        // GET: Export Subscribers
        public async Task<IActionResult> ExportSubscribers()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var subscribers = await _context.Newsletters
                .Where(n => n.IsActive)
                .OrderBy(n => n.Email)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Email,FirstName,LastName,SubscribedAt,Source");

            foreach (var sub in subscribers)
            {
                csv.AppendLine($"{sub.Email},{sub.FirstName},{sub.LastName},{sub.SubscribedAt:yyyy-MM-dd},{sub.Source}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"subscribers_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // GET: Email Campaigns
        public async Task<IActionResult> Campaigns()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            var campaigns = await _context.EmailCampaigns
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(campaigns);
        }

        // GET: Create Campaign
        public IActionResult CreateCampaign()
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            return View();
        }

        // POST: Create Campaign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCampaign(
            string Subject,
            string PreviewText,
            string FromName,
            string FromEmail,
            string ReplyTo,
            string HtmlContent,
            DateTime? ScheduledAt,
            string action)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                
                // Create campaign in database
                var campaign = new EmailCampaign
                {
                    Subject = Subject,
                    PreviewText = PreviewText,
                    FromName = FromName,
                    FromEmail = FromEmail,
                    ReplyTo = ReplyTo,
                    HtmlContent = HtmlContent,
                    ScheduledAt = ScheduledAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = userId,
                    Status = "draft"
                };

                _context.EmailCampaigns.Add(campaign);
                await _context.SaveChangesAsync();

                // Create campaign in MailChimp
                var mailChimpCampaign = await _mailChimpService.CreateCampaignAsync(
                    Subject, FromName, FromEmail, ReplyTo, HtmlContent, PreviewText);

                if (mailChimpCampaign != null)
                {
                    campaign.MailChimpCampaignId = mailChimpCampaign.Id;
                    
                    // Handle action: send or schedule
                    if (action == "send")
                    {
                        var sent = await _mailChimpService.SendCampaignAsync(mailChimpCampaign.Id);
                        if (sent)
                        {
                            campaign.Status = "sent";
                            campaign.SentAt = DateTime.UtcNow;
                            TempData["Success"] = "Campaign đã được gửi thành công!";
                        }
                    }
                    else if (action == "schedule" && ScheduledAt.HasValue)
                    {
                        var scheduled = await _mailChimpService.ScheduleCampaignAsync(
                            mailChimpCampaign.Id, ScheduledAt.Value);
                        if (scheduled)
                        {
                            campaign.Status = "scheduled";
                            TempData["Success"] = "Campaign đã được lên lịch thành công!";
                        }
                    }
                    else
                    {
                        TempData["Success"] = "Campaign đã được lưu nháp!";
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["Error"] = "Có lỗi khi tạo campaign trên MailChimp";
                }

                return RedirectToAction(nameof(Campaigns));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campaign");
                TempData["Error"] = "Có lỗi xảy ra khi tạo campaign";
                return View();
            }
        }

        // POST: Send Campaign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCampaign(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return redirect;

            try
            {
                var campaign = await _context.EmailCampaigns.FindAsync(id);
                if (campaign == null)
                {
                    TempData["Error"] = "Campaign không tồn tại";
                    return RedirectToAction(nameof(Campaigns));
                }

                if (!string.IsNullOrEmpty(campaign.MailChimpCampaignId))
                {
                    var sent = await _mailChimpService.SendCampaignAsync(campaign.MailChimpCampaignId);
                    if (sent)
                    {
                        campaign.Status = "sent";
                        campaign.SentAt = DateTime.UtcNow;
                        campaign.RecipientCount = await _context.Newsletters.CountAsync(n => n.IsActive);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Campaign đã được gửi thành công!";
                    }
                    else
                    {
                        TempData["Error"] = "Có lỗi khi gửi campaign";
                    }
                }

                return RedirectToAction(nameof(Campaigns));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending campaign");
                TempData["Error"] = "Có lỗi xảy ra khi gửi campaign";
                return RedirectToAction(nameof(Campaigns));
            }
        }

        // POST: Delete Campaign
        [HttpPost]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            var redirect = RedirectIfNotAdmin();
            if (redirect != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var campaign = await _context.EmailCampaigns.FindAsync(id);
                if (campaign == null)
                {
                    return Json(new { success = false, message = "Campaign không tồn tại" });
                }

                _context.EmailCampaigns.Remove(campaign);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa campaign" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting campaign");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}

