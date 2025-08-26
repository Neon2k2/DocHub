using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class EmailTemplate : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string TemplateName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string TemplateType { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public string? Category { get; set; }
    
    public int SortOrder { get; set; }
    
    public string? Tags { get; set; }
}
