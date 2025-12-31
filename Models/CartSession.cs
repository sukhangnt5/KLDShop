using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLDShop.Models
{
    public class CartSession
    {
        [Key]
        public int CartSessionId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
    }
}
