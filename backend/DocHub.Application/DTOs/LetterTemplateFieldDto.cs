using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class LetterTemplateFieldDto
{
    public string Id { get; set; } = string.Empty;
    
    public string LetterTemplateId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = "Text";
    
    public bool IsRequired { get; set; } = false;
    
    public string? DefaultValue { get; set; }
    
    public string? ValidationRules { get; set; }
    
    public string? HelpText { get; set; }
    
    public int SortOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime? UpdatedAt { get; set; }
    
    public string? UpdatedBy { get; set; }
}

public class CreateLetterTemplateFieldDto
{
    [Required]
    public string LetterTemplateId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = "Text";
    
    public bool IsRequired { get; set; } = false;
    
    public string? DefaultValue { get; set; }
    
    public string? ValidationRules { get; set; }
    
    public string? HelpText { get; set; }
    
    public int SortOrder { get; set; }
}

public class UpdateLetterTemplateFieldDto
{
    public string? LetterTemplateId { get; set; }
    
    [StringLength(100)]
    public string? FieldName { get; set; }
    
    [StringLength(200)]
    public string? DisplayName { get; set; }
    
    [StringLength(50)]
    public string? DataType { get; set; }
    
    public bool? IsRequired { get; set; }
    
    public string? DefaultValue { get; set; }
    
    public string? ValidationRules { get; set; }
    
    public string? HelpText { get; set; }
    
    public int? SortOrder { get; set; }
}
