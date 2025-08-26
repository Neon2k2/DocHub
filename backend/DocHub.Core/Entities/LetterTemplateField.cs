using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class LetterTemplateField : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = "Text";
    
    [StringLength(50)]
    public string? FieldType { get; set; }
    
    public bool IsRequired { get; set; } = false;
    
    public string? DefaultValue { get; set; }
    
    public string? ValidationRules { get; set; }
    
    public string? HelpText { get; set; }
    
    public int SortOrder { get; set; }
    
    // Foreign key
    public string LetterTemplateId { get; set; } = string.Empty;
    
    // Navigation property
    public virtual LetterTemplate LetterTemplate { get; set; } = null!;
}
