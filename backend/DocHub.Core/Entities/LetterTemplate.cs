using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class LetterTemplate : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string LetterType { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string TemplateContent { get; set; } = string.Empty;
    
    public string? TemplateFilePath { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [Required]
    public string DataSource { get; set; } = "Upload"; // Upload or Database
    
    public string? DatabaseQuery { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; }
    
    // Foreign key for DynamicTab
    public string? DynamicTabId { get; set; }
    
    // Navigation properties
    public virtual ICollection<LetterTemplateField> Fields { get; set; } = new List<LetterTemplateField>();
    public virtual ICollection<GeneratedLetter> GeneratedLetters { get; set; } = new List<GeneratedLetter>();
    public virtual DynamicTab? DynamicTab { get; set; }
}
