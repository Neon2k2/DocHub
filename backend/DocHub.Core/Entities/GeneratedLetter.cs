using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class GeneratedLetter : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string LetterNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LetterType { get; set; } = string.Empty;
    
    public string LetterTemplateId { get; set; } = string.Empty;
    
    public string EmployeeId { get; set; } = string.Empty;
    
    public string? DigitalSignatureId { get; set; }
    
    public string? LetterFilePath { get; set; }
    
    public string Status { get; set; } = "Generated"; // Generated, Sent, Delivered, Failed
    
    public DateTime? GeneratedAt { get; set; }
    
    public DateTime? SentAt { get; set; }
    
    public DateTime? DeliveredAt { get; set; }
    
    public string? EmailId { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public virtual LetterTemplate LetterTemplate { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual DigitalSignature DigitalSignature { get; set; } = null!;
    public virtual ICollection<LetterAttachment> Attachments { get; set; } = new List<LetterAttachment>();
}
