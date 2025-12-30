using System.ComponentModel.DataAnnotations;

namespace KLDShop.Models
{
    public class EmailCampaign
    {
        [Key]
        public int CampaignId { get; set; }

        [Required]
        [StringLength(255)]
        public string Subject { get; set; }

        [Required]
        public string HtmlContent { get; set; }

        [StringLength(500)]
        public string? PreviewText { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? SentAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "draft"; // draft, scheduled, sent, cancelled

        public int? RecipientCount { get; set; }

        public int? OpenCount { get; set; }

        public int? ClickCount { get; set; }

        public string? MailChimpCampaignId { get; set; }

        [StringLength(255)]
        public string? FromName { get; set; }

        [StringLength(255)]
        public string? FromEmail { get; set; }

        [StringLength(255)]
        public string? ReplyTo { get; set; }

        public int? CreatedByUserId { get; set; }
    }
}
