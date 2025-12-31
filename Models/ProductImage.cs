using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLDShop.Models
{
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Range(1, 10)]
        public int DisplayOrder { get; set; } = 1;

        public bool IsMain { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
