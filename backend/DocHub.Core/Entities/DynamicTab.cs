using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class DynamicTab : BaseEntity
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
    
    // Navigation properties
    public virtual ICollection<LetterTemplate> LetterTemplates { get; set; } = new List<LetterTemplate>();
}
