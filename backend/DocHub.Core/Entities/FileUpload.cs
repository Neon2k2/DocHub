using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class FileUpload : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FileType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? DocumentType { get; set; }
    
    [StringLength(100)]
    public string? AuthorityName { get; set; }
    
    [StringLength(100)]
    public string? AuthorityDesignation { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    public string? TabId { get; set; }
    
    public string? LetterType { get; set; }
    
    public string? Version { get; set; }
    
    public bool IsProcessed { get; set; } = false;
    
    public DateTime? ProcessedAt { get; set; }
    
    public string? ProcessedBy { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    [StringLength(50)]
    public string Status { get; set; } = "Pending";
    
    public int ProcessedRows { get; set; } = 0;
    
    public List<string> Errors { get; set; } = new();
    
    // Navigation properties
    public virtual DynamicTab? Tab { get; set; }
}
