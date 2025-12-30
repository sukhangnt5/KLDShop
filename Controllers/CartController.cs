using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KLDShop.Models;
using KLDShop.Data;
using System.Text.Json;

namespace KLDShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "ShoppingCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng từ session
        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        // Lưu giỏ hàng vào session
        private void SaveCartToSession(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            var cart = GetCartFromSession();
            var cartWithDetails = new List<CartItem>();

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    item.Product = product;
                    item.Price = product.Price;
                    item.DiscountPrice = product.DiscountPrice;
                    cartWithDetails.Add(item);
                }
            }

            return View(cartWithDetails);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Kiểm tra người dùng đã đăng nhập
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng" });
                }

                if (productId <= 0 || quantity <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                if (product.Quantity < quantity)
                {
                    return Json(new { success = false, message = $"Số lượng tồn kho không đủ. Chỉ còn {product.Quantity} sản phẩm" });
                }

                var cart = GetCartFromSession();
                var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

                if (existingItem != null)
                {
                    if (existingItem.Quantity + quantity > product.Quantity)
                    {
                        return Json(new { success = false, message = "Số lượng tồn kho không đủ" });
                    }
                    existingItem.Quantity += quantity;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartItemId = cart.Any() ? cart.Max(c => c.CartItemId) + 1 : 1,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price,
                        DiscountPrice = product.DiscountPrice,
                        AddedAt = DateTime.UtcNow
                    };
                    cart.Add(cartItem);
                }

                SaveCartToSession(cart);

                // Also save to database
                try
                {
                    var existingCartSession = await _context.CartSessions
                        .FirstOrDefaultAsync(cs => cs.UserId == userId.Value && cs.ProductId == productId);

                    if (existingCartSession != null)
                    {
                        existingCartSession.Quantity += quantity;
                        existingCartSession.UpdatedAt = DateTime.UtcNow;
                        _context.CartSessions.Update(existingCartSession);
                    }
                    else
                    {
                        var cartSession = new CartSession
                        {
                            UserId = userId.Value,
                            ProductId = productId,
                            Quantity = quantity,
                            Price = product.Price,
                            DiscountPrice = product.DiscountPrice,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.CartSessions.Add(cartSession);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving to database: {dbEx.Message}");
                }

                return Json(new { success = true, message = $"Đã thêm {product.ProductName} vào giỏ hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Cập nhật số lượng sản phẩm trong giỏ
        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để quản lý giỏ hàng" });
            }

            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
            }

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.CartItemId == cartItemId);

            if (item == null)
            {
                return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng" });
            }

            item.Quantity = quantity;
            SaveCartToSession(cart);

            // Also update in database
            try
            {
                var cartSession = await _context.CartSessions
                    .FirstOrDefaultAsync(cs => cs.UserId == userId.Value && cs.ProductId == item.ProductId);
                
                if (cartSession != null)
                {
                    cartSession.Quantity = quantity;
                    cartSession.UpdatedAt = DateTime.UtcNow;
                    _context.CartSessions.Update(cartSession);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating cart in database: {ex.Message}");
            }

            return Json(new { success = true, message = "Cập nhật giỏ hàng thành công" });
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để quản lý giỏ hàng" });
            }

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.CartItemId == cartItemId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);

                // Also remove from database
                try
                {
                    var productId = item.ProductId;
                    var cartSession = await _context.CartSessions
                        .FirstOrDefaultAsync(cs => cs.UserId == userId.Value && cs.ProductId == productId);
                    
                    if (cartSession != null)
                    {
                        _context.CartSessions.Remove(cartSession);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error removing from database: {ex.Message}");
                }
            }

            return Json(new { success = true, message = "Xóa khỏi giỏ hàng thành công" });
        }

        // Xóa tất cả sản phẩm trong giỏ hàng
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            // Get userId before clearing session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để quản lý giỏ hàng" });
            }

            // Clear from database first
            try
            {
                var cartSessions = _context.CartSessions.Where(cs => cs.UserId == userId.Value);
                _context.CartSessions.RemoveRange(cartSessions);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing cart from database: {ex.Message}");
            }

            // Then clear from session
            HttpContext.Session.Remove(CartSessionKey);
            
            return Json(new { success = true, message = "Giỏ hàng đã được làm trống" });
        }

        // API: Lấy thông tin giỏ hàng (AJAX)
        [HttpGet]
        public IActionResult GetCartSummary()
        {
            var cart = GetCartFromSession();
            var itemCount = cart.Sum(c => c.Quantity);

            return Json(new { itemCount = itemCount, totalPrice = 0 });
        }

        // API: Lấy chi tiết giỏ hàng (AJAX)
        [HttpGet]
        [Route("api/cart/items")]
        public IActionResult GetCartItems()
        {
            var cart = GetCartFromSession();
            var items = cart.Select(c => new
            {
                cartItemId = c.CartItemId,
                productId = c.ProductId,
                quantity = c.Quantity,
                price = c.Price,
                discountPrice = c.DiscountPrice
            }).ToList();

            return Json(new { success = true, items = items });
        }
    }
}

