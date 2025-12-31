using System.ComponentModel.DataAnnotations;

namespace KLDShop.Models
{
    public class Newsletter
    {
        [Key]
        public int NewsletterId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string Status { get; set; } = "subscribed"; // subscribed, unsubscribed, pending

        public string? MailChimpSubscriberId { get; set; }

        public DateTime? UnsubscribedAt { get; set; }

        [StringLength(50)]
        public string? Source { get; set; } // website, checkout, admin, etc.
    }
}
