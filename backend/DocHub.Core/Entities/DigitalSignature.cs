using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class DigitalSignature : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string SignatureName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string AuthorityName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string AuthorityDesignation { get; set; } = string.Empty;
    
    public string? SignatureImagePath { get; set; }
    
    public string? SignatureData { get; set; }
    
    public DateTime SignatureDate { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual ICollection<GeneratedLetter> GeneratedLetters { get; set; } = new List<GeneratedLetter>();
}
