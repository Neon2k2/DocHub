using System.ComponentModel.DataAnnotations;
using DocHub.Application.Validation;

namespace DocHub.Application.DTOs.DynamicTabs;

public class UpdateDynamicTabDto
{
    [StringLength(200)]
    [RegularExpression(@"^[a-zA-Z0-9\s_-]+$", ErrorMessage = "Display name can only contain letters, numbers, spaces, underscores, and hyphens")]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [RegularExpression(@"^(letter|database|upload)$", ErrorMessage = "TabType must be either 'letter', 'database', or 'upload'")]
    public string? TabType { get; set; }

    [AllowedDynamicTabDataSource]
    public string? DataSource { get; set; }

    [SafeSqlQuery]
    public string? DatabaseQuery { get; set; }

    public string? ExcelMapping { get; set; }

    public string? TemplateId { get; set; }

    public string? FieldMappings { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    [StringLength(100)]
    [RegularExpression(@"^[a-zA-Z0-9_-]*$")]
    public string? Icon { get; set; }

    [StringLength(50)]
    [RegularExpression(@"^#?[a-fA-F0-9]{6}$", ErrorMessage = "Color must be a valid hex color code")]
    public string? Color { get; set; }

    [StringLength(200)]
    public string? Permissions { get; set; }

    [Required]
    public string UpdatedBy { get; set; } = string.Empty;

    public List<CreateDynamicTabFieldDto>? Fields { get; set; }
}
