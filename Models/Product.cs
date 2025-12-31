using System.ComponentModel.DataAnnotations;

namespace KLDShop.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200)]
        public required string ProductName { get; set; }

        [StringLength(250)]
        public string? Slug { get; set; }

        [StringLength(4000)]
        public string? Description { get; set; }

        [StringLength(160)]
        public string? MetaDescription { get; set; }

        [StringLength(255)]
        public string? MetaKeywords { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal? DiscountPrice { get; set; }

        [Required(ErrorMessage = "Tồn kho không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho không hợp lệ")]
        public int Quantity { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(255)]
        public string? Image { get; set; }

        // Navigation property
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? Weight { get; set; } // kg

        [StringLength(100)]
        public string? Dimensions { get; set; } // kích thước (dài x rộng x cao)

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        public int? WarrantyPeriod { get; set; } // tháng

        public bool IsActive { get; set; } = true;

        public int Views { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
