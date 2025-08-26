using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class LetterAttachment : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? FileType { get; set; }
    
    public long FileSize { get; set; }
    
    // Foreign key
    public string GeneratedLetterId { get; set; } = string.Empty;
    
    // Navigation property
    public virtual GeneratedLetter GeneratedLetter { get; set; } = null!;
}
