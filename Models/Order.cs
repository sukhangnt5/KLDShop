using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLDShop.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string? OrderNumber { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal DiscountAmount { get; set; } = 0;

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TaxAmount { get; set; } = 0;

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ShippingCost { get; set; } = 0;

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal FinalAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // 'Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled'

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Unpaid"; // 'Unpaid', 'Paid', 'Refunded'

        [Required]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ShippingCity { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ShippingDistrict { get; set; }

        [StringLength(100)]
        public string? ShippingWard { get; set; }

        [StringLength(10)]
        public string? ShippingPostalCode { get; set; }

        [Phone]
        [StringLength(20)]
        public string? ShippingPhone { get; set; }

        [Required]
        [StringLength(100)]
        public string ShippingRecipient { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ShippedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public virtual Payment? Payment { get; set; }
    }
}
