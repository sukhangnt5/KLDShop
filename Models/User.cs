using System.ComponentModel.DataAnnotations;

namespace KLDShop.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? Ward { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; } // 'Male', 'Female', 'Other'

        public DateTime? DateOfBirth { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAdmin { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // Navigation property
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
