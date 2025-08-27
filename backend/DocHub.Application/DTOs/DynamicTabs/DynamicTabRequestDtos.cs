using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs.DynamicTabs;

public class CreateDynamicTabDto
{
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Name can only contain letters, numbers, underscores, and hyphens")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    [RegularExpression(@"^[a-zA-Z0-9\s_-]+$", ErrorMessage = "Display name can only contain letters, numbers, spaces, underscores, and hyphens")]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string TabType { get; set; } = string.Empty; // "letter", "database", "upload"

    [Required]
    public string DataSource { get; set; } = string.Empty; // "excel_upload", "database_query", "api"

    public string? DatabaseQuery { get; set; }

    public string? ExcelMapping { get; set; }

    public string? TemplateId { get; set; }

    public string? FieldMappings { get; set; }

    public int SortOrder { get; set; } = 0;

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public string? Permissions { get; set; }

    [Required]
    public string CreatedBy { get; set; } = string.Empty;

    public List<CreateDynamicTabFieldDto>? Fields { get; set; }
}

public class UpdateDynamicTabDto
{
    [StringLength(200)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public string? TabType { get; set; }

    public string? DataSource { get; set; }

    public string? DatabaseQuery { get; set; }

    public string? ExcelMapping { get; set; }

    public string? TemplateId { get; set; }

    public string? FieldMappings { get; set; }

    public int? SortOrder { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public string? Permissions { get; set; }

    [Required]
    public string UpdatedBy { get; set; } = string.Empty;

    public List<CreateDynamicTabFieldDto>? Fields { get; set; }
}

public class AddTabDataDto
{
    [Required]
    public string DataSource { get; set; } = string.Empty;

    public string? ExternalId { get; set; }

    [Required]
    public Dictionary<string, object> Data { get; set; } = new();

    [Required]
    public string CreatedBy { get; set; } = string.Empty;
}

public class TabReorderDto
{
    public string Id { get; set; } = string.Empty;
    public int NewSortOrder { get; set; }
}
