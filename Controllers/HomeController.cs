using System.Diagnostics;
using KLDShop.Models;
using KLDShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KLDShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy sản phẩm nổi bật (có discount hoặc views cao)
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.DiscountPrice.HasValue)
                .ThenByDescending(p => p.Views)
                .Take(6)
                .ToListAsync();

            // Set SEO meta description (160-300 characters optimal)
            ViewData["MetaDescription"] = "KLDShop - Cửa hàng trực tuyến uy tín chuyên cung cấp sản phẩm chất lượng cao với giá tốt nhất thị trường. Giao hàng nhanh chóng toàn quốc, đổi trả dễ dàng trong 30 ngày. Mua sắm an toàn, tiện lợi với nhiều ưu đãi hấp dẫn.";

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Faq()
        {
            return View();
        }

        public IActionResult ShippingPolicy()
        {
            return View();
        }

        public IActionResult ReturnPolicy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string phone, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ các trường bắt buộc");
                return View();
            }

            // TODO: Send email or save to database
            // For now, just return success message
            ViewBag.Message = "Cảm ơn bạn! Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất.";
            ViewBag.MessageType = "success";
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
