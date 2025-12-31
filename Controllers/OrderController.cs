using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KLDShop.Models;
using KLDShop.Data;
using KLDShop.Services.PaymentGateway;
using System.Text.Json;

namespace KLDShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly VNPayService _vnpayService;
        private readonly PayPalService _paypalService;
        private readonly ILogger<OrderController> _logger;
        private readonly IConfiguration _configuration;
        private const string CartSessionKey = "ShoppingCart";

        public OrderController(ApplicationDbContext context, VNPayService vnpayService, PayPalService paypalService, ILogger<OrderController> logger, IConfiguration configuration)
        {
            _context = context;
            _vnpayService = vnpayService;
            _paypalService = paypalService;
            _logger = logger;
            _configuration = configuration;
        }

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

        // Trang thanh toán - Checkout
        public async Task<IActionResult> Checkout()
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Product");
            }

            // Lấy thông tin sản phẩm
            var cartWithDetails = new List<CartItem>();
            decimal totalAmount = 0;

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    item.Product = product;
                    var price = item.DiscountPrice ?? item.Price;
                    totalAmount += price * item.Quantity;
                    cartWithDetails.Add(item);
                }
            }

            ViewBag.TotalAmount = totalAmount;
            ViewBag.CartItems = cartWithDetails;

            return View();
        }

        // Trang xác nhận đơn hàng - Tạo order từ checkout info
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(Order order, string paymentMethod)
        {
            try
            {
                var cart = GetCartFromSession();
                if (!cart.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng trống" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ: " + string.Join(", ", errors.Select(e => e.ErrorMessage)) });
                }

                if (string.IsNullOrEmpty(paymentMethod))
                {
                    return Json(new { success = false, message = "Vui lòng chọn phương thức thanh toán" });
                }

                // Generate order number
                order.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";
                order.OrderDate = DateTime.UtcNow;
                order.Status = "Pending";
                order.PaymentStatus = paymentMethod == "Cash" ? "Pending" : "Unpaid";

                // Calculate totals and verify stock availability
                decimal totalAmount = 0;
                foreach (var item in cart)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        // Check if enough stock available
                        if (product.Quantity < item.Quantity)
                        {
                            return Json(new { success = false, message = $"Sản phẩm '{product.ProductName}' không đủ số lượng trong kho" });
                        }
                        
                        var price = item.DiscountPrice ?? item.Price;
                        totalAmount += price * item.Quantity;
                    }
                }

                order.TotalAmount = totalAmount;
                order.FinalAmount = totalAmount + order.ShippingCost + order.TaxAmount - order.DiscountAmount;
                
                // Lấy UserId từ session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập trước khi đặt hàng" });
                }
                order.UserId = userId.Value;

                // Create order
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order details
                foreach (var item in cart)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price,
                            DiscountPrice = product.DiscountPrice,
                            TotalPrice = (product.DiscountPrice ?? product.Price) * item.Quantity,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.OrderDetails.Add(orderDetail);

                        // Update product quantity
                        product.Quantity -= item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                await _context.SaveChangesAsync();

                // Clear cart
                HttpContext.Session.Remove(CartSessionKey);

                // Nếu là tiền mặt, xác nhận luôn
                if (paymentMethod == "Cash")
                {
                    order.PaymentStatus = "Pending";
                    order.Status = "Confirmed";
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Đơn hàng đã được tạo", orderId = order.OrderId, paymentMethod = "Cash" });
                }

                // Nếu không, trả về orderId để redirect sang Payment
                return Json(new { success = true, message = "Đơn hàng đã được tạo", orderId = order.OrderId, paymentMethod = paymentMethod });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Trang thanh toán - Payment
        public async Task<IActionResult> Payment(int orderId, string paymentMethod)
        {
            if (orderId <= 0)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.OrderId = orderId;
            ViewBag.PaymentMethod = paymentMethod;
            return View(order);
        }

        // Xử lý thanh toán
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int orderId, string paymentMethod)
        {
            if (orderId <= 0)
            {
                return Json(new { success = false, message = "Đơn hàng không hợp lệ" });
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            
            if (order == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });
            }

            try
            {
                // Create payment request
                var returnUrl = _configuration["VNPay:ReturnUrl"] ?? Url.Action("PaymentReturn", "Order", null, Request.Scheme) ?? "";
                var paymentRequest = new PaymentRequest
                {
                    OrderId = orderId,
                    Amount = order.FinalAmount,
                    OrderNumber = order.OrderNumber ?? "",
                    Description = $"Thanh toán đơn hàng {order.OrderNumber ?? "N/A"}",
                    ReturnUrl = returnUrl,
                    CancelUrl = Url.Action("Payment", "Order", new { orderId }, Request.Scheme) ?? "",
                    NotifyUrl = Url.Action("PaymentNotify", "Order", null, Request.Scheme) ?? "",
                    CustomerEmail = "customer@example.com", // TODO: Get from current user
                    CustomerPhone = order.ShippingPhone ?? ""
                };

                PaymentResponse? paymentResponse = null;

                // Redirect tới payment gateway dựa trên phương thức
                if (paymentMethod == "VNPay")
                {
                    paymentResponse = await _vnpayService.CreatePaymentAsync(paymentRequest);
                    
                    if (paymentResponse?.Success == true)
                    {
                        return Json(new 
                        { 
                            success = true, 
                            message = "Chuyển hướng đến VNPay", 
                            redirectUrl = paymentResponse.PaymentUrl,
                            paymentMethod = "VNPay",
                            orderId = orderId
                        });
                    }
                }
                else if (paymentMethod == "PayPal")
                {
                    paymentResponse = await _paypalService.CreatePaymentAsync(paymentRequest);
                    
                    if (paymentResponse?.Success == true)
                    {
                        return Json(new 
                        { 
                            success = true, 
                            message = "Chuyển hướng đến PayPal", 
                            redirectUrl = paymentResponse.PaymentUrl,
                            paymentMethod = "PayPal",
                            orderId = orderId
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"PayPal CreatePayment failed: {paymentResponse?.Message}");
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Phương thức thanh toán không được hỗ trợ" });
                }

                return Json(new { success = false, message = paymentResponse?.Message ?? "Lỗi xử lý thanh toán" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Callback từ VNPay sau khi thanh toán
        [HttpGet]
        public async Task<IActionResult> PaymentReturn()
        {
            try
            {
                var vnp_ResponseCode = Request.Query["vnp_ResponseCode"].ToString();
                var vnp_TxnRef = Request.Query["vnp_TxnRef"].ToString();
                var vnp_Amount = Request.Query["vnp_Amount"].ToString();
                var vnp_OrderInfo = Request.Query["vnp_OrderInfo"].ToString();

                System.Diagnostics.Debug.WriteLine($"VNPay Return - ResponseCode: {vnp_ResponseCode}, TxnRef: {vnp_TxnRef}");

                if (vnp_ResponseCode == "00")
                {
                    // Payment successful - tìm order gần nhất của user
                    var userId = HttpContext.Session.GetInt32("UserId");
                    if (userId == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var order = await _context.Orders
                        .Include(o => o.Payment)
                        .Where(o => o.UserId == userId && o.Status == "Pending")
                        .OrderByDescending(o => o.OrderDate)
                        .FirstOrDefaultAsync();
                    
                    if (order != null)
                    {
                        // Check if payment already exists for this order
                        if (order.Payment != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Payment already exists for order {order.OrderId}, skipping duplicate creation");
                            return RedirectToAction("Confirmation", new { id = order.OrderId });
                        }
                        
                        // Create payment record
                        var payment = new Payment
                        {
                            OrderId = order.OrderId,
                            PaymentMethod = "VNPay",
                            PaymentAmount = order.FinalAmount,
                            PaymentStatus = "Paid",
                            TransactionId = vnp_TxnRef,
                            TransactionDate = DateTime.UtcNow,
                            PaymentDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Payments.Add(payment);
                        order.PaymentStatus = "Paid";
                        order.Status = "Confirmed";
                        _context.Orders.Update(order);
                        
                        // Clear shopping cart from database
                        if (userId.HasValue)
                        {
                            var cartSessions = _context.CartSessions.Where(cs => cs.UserId == userId.Value);
                            _context.CartSessions.RemoveRange(cartSessions);
                        }
                        
                        await _context.SaveChangesAsync();

                        // Clear shopping cart from session
                        HttpContext.Session.Remove(CartSessionKey);

                        System.Diagnostics.Debug.WriteLine($"VNPay payment successful - OrderId: {order.OrderId}");
                        return RedirectToAction("Confirmation", new { id = order.OrderId });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No pending order found for user {userId}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"VNPay payment failed - ResponseCode: {vnp_ResponseCode}");
                }

                return RedirectToAction("Checkout", "Order");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VNPay return error: {ex.Message}");
                return RedirectToAction("Checkout", "Order");
            }
        }

        // Callback từ PayPal sau khi thanh toán
        [HttpGet]
        public async Task<IActionResult> PayPalReturn(string token, string PayerID, int orderId = 0)
        {
            try
            {
                _logger.LogInformation($"PayPalReturn called - token: {token}, PayerID: {PayerID}, orderId: {orderId}");
                
                // Verify payment and get transaction ID from PayPal
                var queryParams = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    { "token", new Microsoft.Extensions.Primitives.StringValues(token) },
                    { "PayerID", new Microsoft.Extensions.Primitives.StringValues(PayerID) }
                });
                
                var (isValid, transactionId) = await _paypalService.VerifyPaymentAndGetTransactionAsync(queryParams);
                
                _logger.LogInformation($"PayPal verification result: {isValid}, TransactionId: {transactionId}");

                if (!isValid)
                {
                    _logger.LogWarning($"PayPal verification failed - redirecting to Checkout");
                    return RedirectToAction("Checkout", "Order");
                }

                // Tìm order bằng orderId từ URL
                Order? order = null;
                if (orderId > 0)
                {
                    order = await _context.Orders
                        .Include(o => o.Payment)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);
                    _logger.LogInformation($"Order found by orderId {orderId}: {order != null}");
                }

                // Nếu không tìm thấy, tìm order gần nhất của user
                int? userIdForPayPal = null;
                if (order == null)
                {
                    userIdForPayPal = HttpContext.Session.GetInt32("UserId");
                    if (userIdForPayPal != null)
                    {
                        order = await _context.Orders
                            .Include(o => o.Payment)
                            .Where(o => o.UserId == userIdForPayPal && o.Status == "Pending")
                            .OrderByDescending(o => o.OrderDate)
                            .FirstOrDefaultAsync();
                        _logger.LogInformation($"Order found by userId: {order != null}");
                    }
                }

                if (order == null)
                {
                    _logger.LogWarning($"No order found - redirecting to Checkout");
                    return RedirectToAction("Checkout", "Order");
                }
                
                // Check if payment already exists for this order
                if (order.Payment != null)
                {
                    _logger.LogInformation($"Payment already exists for order {order.OrderId}, skipping duplicate creation");
                    return RedirectToAction("Confirmation", new { id = order.OrderId });
                }
                
                _logger.LogInformation($"Processing PayPal return for order {order.OrderId}");

                // Use the actual transaction ID (capture ID) from PayPal
                var finalTransactionId = !string.IsNullOrEmpty(transactionId) ? transactionId : token;

                // Cập nhật trạng thái thanh toán
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = "PayPal",
                    PaymentAmount = order.FinalAmount,
                    PaymentStatus = "Paid",
                    TransactionId = finalTransactionId, // Use capture ID instead of order ID
                    TransactionDate = DateTime.UtcNow,
                    PaymentDate = DateTime.UtcNow,
                    Notes = $"PayPal Order ID: {token}", // Store order ID in notes for reference
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                order.PaymentStatus = "Paid";
                order.Status = "Confirmed";
                _context.Orders.Update(order);
                
                // Clear shopping cart from database
                userIdForPayPal = userIdForPayPal ?? HttpContext.Session.GetInt32("UserId");
                if (userIdForPayPal.HasValue)
                {
                    var cartSessions = _context.CartSessions.Where(cs => cs.UserId == userIdForPayPal.Value);
                    _context.CartSessions.RemoveRange(cartSessions);
                }
                
                await _context.SaveChangesAsync();

                // Clear shopping cart from session
                HttpContext.Session.Remove(CartSessionKey);

                _logger.LogInformation($"PayPal payment completed - OrderId: {order.OrderId}, TransactionId: {finalTransactionId}");

                return RedirectToAction("Confirmation", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"PayPal return error: {ex.Message} - {ex.StackTrace}");
                return RedirectToAction("Checkout", "Order");
            }
        }

        // Approve PayPal Payment - Called from PayPal Checkout JS after successful capture
        [HttpPost]
        public async Task<IActionResult> ApprovePayPalPayment(int orderId, string paypalOrderId, string paypalTransactionId)
        {
            try
            {
                _logger.LogInformation($"ApprovePayPalPayment called - OrderId: {orderId}, PayPalOrderId: {paypalOrderId}, TransactionId: {paypalTransactionId}");

                if (orderId <= 0 || string.IsNullOrEmpty(paypalOrderId))
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.Payment)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại" });
                }

                // Check if payment already exists for this order
                if (order.Payment != null)
                {
                    _logger.LogInformation($"Payment already exists for order {orderId}, skipping duplicate creation");
                    return Json(new { success = true, message = "Thanh toán đã được xử lý", orderId = orderId });
                }

                // Verify payment details from PayPal
                var paymentDetail = await _paypalService.GetPaymentDetailAsync(paypalOrderId);
                
                _logger.LogInformation($"PayPal Payment Detail - Status: {paymentDetail.Status}, Amount: {paymentDetail.Amount}");

                // Use paypalTransactionId if provided, otherwise use paypalOrderId as fallback
                var transactionId = !string.IsNullOrEmpty(paypalTransactionId) ? paypalTransactionId : paypalOrderId;
                
                _logger.LogInformation($"Using Transaction ID: {transactionId}");

                // Create payment record
                var payment = new Payment
                {
                    OrderId = orderId,
                    PaymentMethod = "PayPal",
                    PaymentAmount = order.FinalAmount,
                    PaymentStatus = "Paid",
                    TransactionId = transactionId, // Use the actual transaction ID (capture ID)
                    TransactionDate = DateTime.UtcNow,
                    PaymentDate = DateTime.UtcNow,
                    Notes = $"PayPal Order ID: {paypalOrderId}", // Store order ID in notes for reference
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                order.PaymentStatus = "Paid";
                order.Status = "Confirmed";
                _context.Orders.Update(order);
                
                // Clear shopping cart from database
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    var cartSessions = _context.CartSessions.Where(cs => cs.UserId == userId.Value);
                    _context.CartSessions.RemoveRange(cartSessions);
                }
                
                await _context.SaveChangesAsync();

                // Clear shopping cart from session
                HttpContext.Session.Remove(CartSessionKey);

                _logger.LogInformation($"PayPal payment approved and saved - OrderId: {orderId}, TransactionId: {transactionId}");

                return Json(new { success = true, message = "Thanh toán thành công", orderId = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ApprovePayPalPayment: {ex.Message} - {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Webhook notification từ Payment Gateway
        [HttpPost]
        public async Task<IActionResult> PaymentNotify()
        {
            try
            {
                // TODO: Handle webhook notification from VNPay/PayPal
                // Update order status based on payment gateway response
                
                return Ok();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Payment notify error: {ex.Message}");
                return BadRequest();
            }
        }

        // Trang xác nhận đơn hàng
        public async Task<IActionResult> Confirmation(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Trang chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Danh sách đơn hàng của người dùng
        public async Task<IActionResult> MyOrders(int page = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            const int pageSize = 10;
            var query = _context.Orders.Where(o => o.UserId == userId);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        // Hủy đơn hàng
        [HttpPost]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequest? request)
        {
            if (request == null || request.OrderId <= 0)
            {
                return Json(new { success = false, message = "Đơn hàng không hợp lệ" });
            }

            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });
            }

            if (order.Status != "Pending" && order.Status != "Confirmed")
            {
                return Json(new { success = false, message = "Không thể hủy đơn hàng này" });
            }

            order.Status = "Cancelled";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đơn hàng đã được hủy" });
        }

        public class CancelOrderRequest
        {
            public int OrderId { get; set; }
        }

        // API: Lấy thông tin đơn hàng (AJAX)
        [HttpGet]
        [Route("api/orders/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    orderId = order.OrderId,
                    orderNumber = order.OrderNumber,
                    orderDate = order.OrderDate,
                    status = order.Status,
                    totalAmount = order.FinalAmount,
                    items = order.OrderDetails.Select(od => new
                    {
                        productName = od.Product?.ProductName ?? "N/A",
                        quantity = od.Quantity,
                        price = od.UnitPrice,
                        total = od.TotalPrice
                    })
                }
            });
        }

        // API: Cập nhật trạng thái đơn hàng (Admin only)
        [HttpPost]
        [Route("api/orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            if (id <= 0 || string.IsNullOrEmpty(status))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var order = await _context.Orders.FindAsync(id);
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
    }
}

