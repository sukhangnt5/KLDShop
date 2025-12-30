using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KLDShop.Data;
using KLDShop.Models;
using System.Security.Cryptography;
using System.Text;

namespace KLDShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trang đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // POST: Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập tên đăng nhập và mật khẩu");
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác");
                return View();
            }

            // Kiểm tra mật khẩu
            if (!VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác");
                return View();
            }

            // Lưu thông tin user vào session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetInt32("IsAdmin", user.IsAdmin ? 1 : 0);

            // Cập nhật thời gian đăng nhập cuối cùng
            user.LastLoginAt = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Khôi phục giỏ hàng từ database
            var cartSessions = await _context.CartSessions
                .Where(cs => cs.UserId == user.UserId)
                .Include(cs => cs.Product)
                .ToListAsync();

            if (cartSessions.Any())
            {
                var cartItems = cartSessions.Select(cs => new CartItem
                {
                    ProductId = cs.ProductId,
                    Quantity = cs.Quantity,
                    Price = cs.Price,
                    DiscountPrice = cs.DiscountPrice,
                    Product = cs.Product
                }).ToList();

                var cartJson = System.Text.Json.JsonSerializer.Serialize(cartItems);
                HttpContext.Session.SetString("ShoppingCart", cartJson);
            }

            // Redirect đến admin dashboard nếu là admin, ngược lại về trang chủ
            if (user.IsAdmin)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Trang đăng ký
        public IActionResult Register()
        {
            return View();
        }

        // POST: Xử lý đăng ký
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, string fullName, string phoneNumber)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ các trường bắt buộc");
                return View();
            }

            if (username.Length < 3 || username.Length > 50)
            {
                ModelState.AddModelError("", "Tên đăng nhập phải từ 3-50 ký tự");
                return View();
            }

            if (password.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                return View();
            }

            // Kiểm tra tên đăng nhập đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                return View();
            }

            // Kiểm tra email đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email đã được đăng ký");
                return View();
            }

            // Tạo user mới
            var user = new User
            {
                Username = username,
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                PasswordHash = HashPassword(password),
                IsActive = true,
                IsAdmin = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Lưu thông tin user vào session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetInt32("IsAdmin", 0);

            // Chuyển hướng đến trang chủ
            return RedirectToAction("Index", "Home");
        }

        // GET: Trang hồ sơ người dùng
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // POST: Cập nhật hồ sơ người dùng
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(int userId, string fullName, string phoneNumber, string address, string city, string district, string ward, string postalCode, string gender, DateTime? dateOfBirth)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null || sessionUserId != userId)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.Address = address;
            user.City = city;
            user.District = district;
            user.Ward = ward;
            user.PostalCode = postalCode;
            user.Gender = gender;
            user.DateOfBirth = dateOfBirth;
            user.UpdatedAt = DateTime.Now;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Cập nhật session
            HttpContext.Session.SetString("FullName", user.FullName);

            ViewBag.Message = "Hồ sơ đã được cập nhật thành công";
            return View("Profile", user);
        }

        // POST: Đổi mật khẩu
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ các trường");
                var user = await _context.Users.FindAsync(userId);
                return View("Profile", user);
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu mới phải có ít nhất 6 ký tự");
                var user = await _context.Users.FindAsync(userId);
                return View("Profile", user);
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                var user = await _context.Users.FindAsync(userId);
                return View("Profile", user);
            }

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            if (!VerifyPassword(currentPassword, currentUser.PasswordHash))
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không chính xác");
                return View("Profile", currentUser);
            }

            currentUser.PasswordHash = HashPassword(newPassword);
            currentUser.UpdatedAt = DateTime.Now;
            _context.Users.Update(currentUser);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Mật khẩu đã được thay đổi thành công";
            return View("Profile", currentUser);
        }

        // POST: Đăng xuất
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            // Lưu giỏ hàng vào database trước khi logout (để khôi phục khi login lại)
            if (userId.HasValue)
            {
                var cartJson = HttpContext.Session.GetString("ShoppingCart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    try
                    {
                        var cartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                        if (cartItems != null && cartItems.Any())
                        {
                            // Xóa giỏ hàng cũ
                            var oldCartSessions = _context.CartSessions.Where(cs => cs.UserId == userId.Value);
                            _context.CartSessions.RemoveRange(oldCartSessions);

                            // Thêm giỏ hàng mới
                            foreach (var item in cartItems)
                            {
                                var cartSession = new Models.CartSession
                                {
                                    UserId = userId.Value,
                                    ProductId = item.ProductId,
                                    Quantity = item.Quantity,
                                    Price = item.Price,
                                    DiscountPrice = item.DiscountPrice,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                                _context.CartSessions.Add(cartSession);
                            }

                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving cart session: {ex.Message}");
                    }
                }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Helper methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
