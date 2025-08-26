using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class LetterPreview : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string LetterType { get; set; } = string.Empty;
    
    public string LetterTemplateId { get; set; } = string.Empty;
    
    public string EmployeeId { get; set; } = string.Empty;
    
    public string? DigitalSignatureId { get; set; }
    
    [Required]
    public string PreviewContent { get; set; } = string.Empty;
    
    public string? PreviewFilePath { get; set; }
    
    public string? PreviewImagePath { get; set; }
    
    public DateTime? LastGeneratedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string? GeneratedBy { get; set; }
    
    // Navigation properties
    public virtual LetterTemplate LetterTemplate { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual DigitalSignature? DigitalSignature { get; set; }
    public virtual ICollection<LetterAttachment> Attachments { get; set; } = new List<LetterAttachment>();
}
