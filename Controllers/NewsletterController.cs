using Microsoft.AspNetCore.Mvc;
using KLDShop.Data;
using KLDShop.Models;
using KLDShop.Services;
using Microsoft.EntityFrameworkCore;

namespace KLDShop.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MailChimpService _mailChimpService;
        private readonly ILogger<NewsletterController> _logger;

        public NewsletterController(
            ApplicationDbContext context, 
            MailChimpService mailChimpService,
            ILogger<NewsletterController> logger)
        {
            _context = context;
            _mailChimpService = mailChimpService;
            _logger = logger;
        }

        // POST: Newsletter/Subscribe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string email, string? firstName = null, string? lastName = null, string? source = "website")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["NewsletterError"] = "Vui lòng nhập địa chỉ email.";
                    return RedirectToAction("Index", "Home");
                }

                // Check if email already exists
                var existingSubscriber = await _context.Newsletters
                    .FirstOrDefaultAsync(n => n.Email == email);

                if (existingSubscriber != null && existingSubscriber.IsActive)
                {
                    TempData["NewsletterInfo"] = "Email này đã được đăng ký nhận tin.";
                    return RedirectToAction("Index", "Home");
                }

                // Subscribe to MailChimp
                var mailChimpSuccess = await _mailChimpService.SubscribeAsync(email, firstName, lastName);

                if (mailChimpSuccess)
                {
                    // Save to database
                    if (existingSubscriber != null)
                    {
                        existingSubscriber.IsActive = true;
                        existingSubscriber.Status = "subscribed";
                        existingSubscriber.SubscribedAt = DateTime.UtcNow;
                        existingSubscriber.UnsubscribedAt = null;
                    }
                    else
                    {
                        var newsletter = new Newsletter
                        {
                            Email = email,
                            FirstName = firstName,
                            LastName = lastName,
                            SubscribedAt = DateTime.UtcNow,
                            IsActive = true,
                            Status = "subscribed",
                            Source = source
                        };
                        _context.Newsletters.Add(newsletter);
                    }

                    await _context.SaveChangesAsync();

                    TempData["NewsletterSuccess"] = "Cảm ơn bạn đã đăng ký nhận tin! Vui lòng kiểm tra email để xác nhận.";
                }
                else
                {
                    TempData["NewsletterError"] = "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại sau.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to newsletter");
                TempData["NewsletterError"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Newsletter/Unsubscribe
        public async Task<IActionResult> Unsubscribe(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Index", "Home");
            }

            var subscriber = await _context.Newsletters
                .FirstOrDefaultAsync(n => n.Email == email);

            if (subscriber != null)
            {
                ViewBag.Email = email;
                ViewBag.IsSubscribed = subscriber.IsActive;
                return View();
            }

            TempData["NewsletterError"] = "Email không tồn tại trong danh sách.";
            return RedirectToAction("Index", "Home");
        }

        // POST: Newsletter/ConfirmUnsubscribe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmUnsubscribe(string email)
        {
            try
            {
                var subscriber = await _context.Newsletters
                    .FirstOrDefaultAsync(n => n.Email == email);

                if (subscriber != null)
                {
                    // Unsubscribe from MailChimp
                    await _mailChimpService.UnsubscribeAsync(email);

                    // Update database
                    subscriber.IsActive = false;
                    subscriber.Status = "unsubscribed";
                    subscriber.UnsubscribedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    TempData["NewsletterSuccess"] = "Bạn đã hủy đăng ký thành công.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from newsletter");
                TempData["NewsletterError"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

