using System.Security.Cryptography;
using System.Text;

namespace KLDShop.Services.PaymentGateway
{
    /// <summary>
    /// VNPay Payment Gateway Service
    /// </summary>
    public class VNPayService : IPaymentGateway
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // VNPay Configuration
        private string _vnpayTmnCode;
        private string _vnpayHashSecret;
        private string _vnpayUrl;
        private string _vnpayApiUrl;

        public VNPayService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

            // Load configuration từ appsettings.json
            _vnpayTmnCode = _configuration["VNPay:TmnCode"] ?? "";
            _vnpayHashSecret = _configuration["VNPay:HashSecret"] ?? "";
            _vnpayUrl = _configuration["VNPay:PaymentUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            _vnpayApiUrl = _configuration["VNPay:ApiUrl"] ?? "https://sandbox.vnpayment.vn/merchant_webapi/merchant.do";
        }

        /// <summary>
        /// Tạo payment URL cho VNPay
        /// </summary>
        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(_vnpayTmnCode) || string.IsNullOrEmpty(_vnpayHashSecret))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "VNPay configuration không đầy đủ. Vui lòng kiểm tra appsettings.json"
                    };
                }

                var tick = DateTime.Now.Ticks.ToString();
                var vnpay = new VnPayLibrary();

                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", _vnpayTmnCode);
                vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", GetClientIpAddress());
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng: {request.OrderNumber}");
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", request.ReturnUrl);
                vnpay.AddRequestData("vnp_TxnRef", tick);

                var paymentUrl = vnpay.CreateRequestUrl(_vnpayUrl, _vnpayHashSecret);

                System.Diagnostics.Debug.WriteLine($"Payment URL: {paymentUrl}");

                return await Task.FromResult(new PaymentResponse
                {
                    Success = true,
                    Message = "Tạo payment URL thành công",
                    PaymentUrl = paymentUrl,
                    TransactionId = tick
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Lỗi khi tạo VNPay payment: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xác minh callback từ VNPay
        /// </summary>
        public async Task<bool> VerifyPaymentAsync(IQueryCollection queryParams)
        {
            try
            {
                var vnpay = new VnPayLibrary();
                foreach (var (key, value) in queryParams)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(key, value.ToString());
                    }
                }

                var vnp_SecureHash = queryParams["vnp_SecureHash"].ToString();
                return await Task.FromResult(vnpay.ValidateSignature(vnp_SecureHash, _vnpayHashSecret));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VNPay verification error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy chi tiết giao dịch từ VNPay
        /// </summary>
        public async Task<PaymentDetail> GetPaymentDetailAsync(string transactionId)
        {
            try
            {
                // TODO: Implement VNPay API call to get transaction details
                // 1. Call VNPay API endpoint
                // 2. Parse response
                // 3. Return PaymentDetail

                return await Task.FromResult(new PaymentDetail());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting VNPay payment detail: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Lấy IP address của client
        /// </summary>
        private string GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Request.Headers.ContainsKey("X-Forwarded-For") == true)
            {
                return context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
            }
            return context?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        }
    }
}
