using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class LetterStatusHistory : BaseEntity
{
    [Required]
    [MaxLength(450)]
    public string LetterId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string OldStatus { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string NewStatus { get; set; } = string.Empty;
    
    [Required]
    public DateTime ChangedAt { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ChangedBy { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation property
    public virtual GeneratedLetter Letter { get; set; } = null!;
}
