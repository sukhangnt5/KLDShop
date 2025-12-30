using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KLDShop.Data;
using KLDShop.Models;

namespace KLDShop.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Wishlist/Index
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Wishlist" });
            }

            var wishlistItems = await _context.Wishlists
                .Include(w => w.Product)
                .ThenInclude(p => p.Category)
                .Where(w => w.UserId == userId.Value)
                .OrderByDescending(w => w.AddedDate)
                .ToListAsync();

            return View(wishlistItems);
        }

        // POST: Wishlist/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào yêu thích", requireLogin = true });
            }

            // Check if product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            // Check if already in wishlist
            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId.Value && w.ProductId == productId);

            if (existingItem != null)
            {
                return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích", alreadyExists = true });
            }

            // Add to wishlist
            var wishlistItem = new Wishlist
            {
                UserId = userId.Value,
                ProductId = productId,
                AddedDate = DateTime.UtcNow
            };

            _context.Wishlists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            // Get wishlist count
            var wishlistCount = await _context.Wishlists.CountAsync(w => w.UserId == userId.Value);

            return Json(new { success = true, message = "Đã thêm vào danh sách yêu thích", wishlistCount = wishlistCount });
        }

        // POST: Wishlist/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId.Value && w.ProductId == productId);

            if (wishlistItem == null)
            {
                return Json(new { success = false, message = "Sản phẩm không có trong danh sách yêu thích" });
            }

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            // Get wishlist count
            var wishlistCount = await _context.Wishlists.CountAsync(w => w.UserId == userId.Value);

            return Json(new { success = true, message = "Đã xóa khỏi danh sách yêu thích", wishlistCount = wishlistCount });
        }

        // GET: Wishlist/GetCount
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { count = 0 });
            }

            var count = await _context.Wishlists.CountAsync(w => w.UserId == userId.Value);
            return Json(new { count = count });
        }

        // GET: Wishlist/Check
        [HttpGet]
        public async Task<IActionResult> Check(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { inWishlist = false });
            }

            var exists = await _context.Wishlists
                .AnyAsync(w => w.UserId == userId.Value && w.ProductId == productId);

            return Json(new { inWishlist = exists });
        }

        // POST: Wishlist/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            if (product.Quantity <= 0)
            {
                return Json(new { success = false, message = "Sản phẩm đã hết hàng" });
            }

            // Use CartSession to save to database
            var existingCartSession = await _context.CartSessions
                .FirstOrDefaultAsync(cs => cs.UserId == userId.Value && cs.ProductId == productId);

            if (existingCartSession != null)
            {
                existingCartSession.Quantity += 1;
                existingCartSession.UpdatedAt = DateTime.UtcNow;
                _context.CartSessions.Update(existingCartSession);
            }
            else
            {
                var cartSession = new CartSession
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    Quantity = 1,
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CartSessions.Add(cartSession);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã thêm vào giỏ hàng" });
        }

        // POST: Wishlist/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var wishlistItems = await _context.Wishlists
                .Where(w => w.UserId == userId.Value)
                .ToListAsync();

            _context.Wishlists.RemoveRange(wishlistItems);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa toàn bộ danh sách yêu thích" });
        }
    }
}

