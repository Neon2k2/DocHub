using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class EmailAttachment : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? FileType { get; set; }
    
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    public long FileSize { get; set; }
    
    public string EmailHistoryId { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual EmailHistory EmailHistory { get; set; } = null!;
}
