using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class DynamicTabDto
{
    public string Id { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string DataSource { get; set; } = "Upload"; // Upload or Database
    
    [MaxLength(4000)]
    public string? DatabaseQuery { get; set; }
    
    [MaxLength(100)]
    public string? Icon { get; set; }
    
    [MaxLength(50)]
    public string? Color { get; set; }
    
    public int SortOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsAdminOnly { get; set; } = false;
    
    [MaxLength(100)]
    public string? RequiredPermission { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<LetterTemplateDto> LetterTemplates { get; set; } = new List<LetterTemplateDto>();
}

public class CreateDynamicTabDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string DataSource { get; set; } = "Upload";
    
    [MaxLength(4000)]
    public string? DatabaseQuery { get; set; }
    
    [MaxLength(100)]
    public string? Icon { get; set; }
    
    [MaxLength(50)]
    public string? Color { get; set; }
    
    public bool IsAdminOnly { get; set; } = false;
    
    [MaxLength(100)]
    public string? RequiredPermission { get; set; }
}

public class UpdateDynamicTabDto
{
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string DataSource { get; set; } = "Upload";
    
    [MaxLength(4000)]
    public string? DatabaseQuery { get; set; }
    
    [MaxLength(100)]
    public string? Icon { get; set; }
    
    [MaxLength(50)]
    public string? Color { get; set; }
    
    public int SortOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsAdminOnly { get; set; } = false;
    
    [MaxLength(100)]
    public string? RequiredPermission { get; set; }
}

public class TabReorderDto
{
    public string Id { get; set; } = string.Empty;
    public int NewSortOrder { get; set; }
}
