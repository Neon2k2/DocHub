using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class LetterAttachmentDto
{
    public string Id { get; set; } = string.Empty;
    
    public string GeneratedLetterId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? FileType { get; set; }
    
    public long FileSize { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime? UpdatedAt { get; set; }
    
    public string? UpdatedBy { get; set; }
}

public class CreateLetterAttachmentDto
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
}

public class UpdateLetterAttachmentDto
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
}
