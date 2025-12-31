using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLDShop.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? DiscountPrice { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
