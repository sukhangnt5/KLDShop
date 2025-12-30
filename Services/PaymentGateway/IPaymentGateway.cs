namespace KLDShop.Services.PaymentGateway
{
    /// <summary>
    /// Interface cho các Payment Gateway
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Tạo payment link/redirect URL
        /// </summary>
        Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);

        /// <summary>
        /// Xác minh webhook/callback từ payment gateway
        /// </summary>
        Task<bool> VerifyPaymentAsync(IQueryCollection queryParams);

        /// <summary>
        /// Lấy thông tin chi tiết về giao dịch
        /// </summary>
        Task<PaymentDetail> GetPaymentDetailAsync(string transactionId);
    }

    /// <summary>
    /// Request tạo thanh toán
    /// </summary>
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderNumber { get; set; }
        public string Description { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public string NotifyUrl { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
    }

    /// <summary>
    /// Response từ payment gateway
    /// </summary>
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PaymentUrl { get; set; }
        public string TransactionId { get; set; }
    }

    /// <summary>
    /// Chi tiết giao dịch thanh toán
    /// </summary>
    public class PaymentDetail
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
