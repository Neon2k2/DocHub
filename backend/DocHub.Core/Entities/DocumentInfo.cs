using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class DocumentInfo : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContentType { get; set; }

    [Required]
    [StringLength(100)]
    public string UploadedBy { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
