using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLDShop.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // 'CreditCard', 'DebitCard', 'BankTransfer', 'Cash', 'E-Wallet'

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PaymentAmount { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // 'Pending', 'Completed', 'Failed', 'Cancelled'

        [StringLength(100)]
        public string? TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(20)]
        public string? CardNumber { get; set; } // lưu 4 chữ số cuối

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(4000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
