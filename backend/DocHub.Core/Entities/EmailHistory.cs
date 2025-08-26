using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class EmailHistory : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string ToEmail { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? CcEmail { get; set; }
    
    [StringLength(255)]
    public string? BccEmail { get; set; }
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Sent"; // Sent, Delivered, Failed, Bounced
    
    public DateTime? SentAt { get; set; }
    
    public DateTime? DeliveredAt { get; set; }
    
    public DateTime? FailedAt { get; set; }
    
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(100)]
    public string? EmailProvider { get; set; }
    
    [StringLength(100)]
    public string? EmailId { get; set; }
    
    public int RetryCount { get; set; } = 0;
    
    public DateTime? LastRetryAt { get; set; }
    
    public string? GeneratedLetterId { get; set; }
    
    public string? EmployeeId { get; set; }
    
    // Navigation properties
    public virtual GeneratedLetter? GeneratedLetter { get; set; }
    public virtual Employee? Employee { get; set; }
    public virtual ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}
