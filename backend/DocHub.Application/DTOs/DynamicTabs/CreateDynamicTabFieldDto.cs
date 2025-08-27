using System.ComponentModel.DataAnnotations;
using DocHub.Application.Validation;

namespace DocHub.Application.DTOs.DynamicTabs;

public class CreateDynamicTabFieldDto
{
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Field name can only contain letters, numbers, and underscores")]
    public string FieldName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [AllowedDynamicTabDataType]
    public string DataType { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = false;

    public bool IsEditable { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public string? ValidationRules { get; set; }

    public string? DefaultValue { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; } = 0;

    [StringLength(100)]
    public string? ExcelColumnName { get; set; }

    [StringLength(100)]
    public string? DatabaseColumnName { get; set; }
}
