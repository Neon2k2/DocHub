using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DocHub.Core.Entities;

namespace DocHub.Application.DTOs.DynamicTabs;

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

public class CreateDynamicTabRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
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

    public List<CreateDynamicTabFieldRequest>? Fields { get; set; }
}

public class UpdateDynamicTabRequest
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

    public List<CreateDynamicTabFieldRequest>? Fields { get; set; }
}

public class CreateDynamicTabFieldRequest
{
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = false;

    public bool IsEditable { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public string? ValidationRules { get; set; }

    public string? DefaultValue { get; set; }

    public int SortOrder { get; set; } = 0;

    public string? ExcelColumnName { get; set; }

    public string? DatabaseColumnName { get; set; }
}

public class AddTabDataRequest
{
    [Required]
    public string DataSource { get; set; } = string.Empty;

    public string? ExternalId { get; set; }

    [Required]
    public Dictionary<string, object> Data { get; set; } = new();

    [Required]
    public string CreatedBy { get; set; } = string.Empty;
}

public class DynamicTabResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TabType { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string? DatabaseQuery { get; set; }
    public string? ExcelMapping { get; set; }
    public string? TemplateId { get; set; }
    public string? FieldMappings { get; set; }
    public int DisplayOrder { get; set; }
    public string? IconName { get; set; }
    public string? ColorScheme { get; set; }
    public string? Permissions { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DynamicTabFieldResponse> Fields { get; set; } = new();
    public LetterTemplateInfo? Template { get; set; }
}

public class DynamicTabFieldResponse
{
    public string Id { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsEditable { get; set; }
    public bool IsVisible { get; set; }
    public string? ValidationRules { get; set; }
    public string? DefaultValue { get; set; }
    public int DisplayOrder { get; set; }
    public string? ExcelColumnName { get; set; }
    public string? DatabaseColumnName { get; set; }
}

public class DynamicTabDataResponse
{
    public string Id { get; set; } = string.Empty;
    public string DynamicTabId { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class LetterTemplateInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LetterType { get; set; } = string.Empty;
}

public class TabReorderDto
{
    public string Id { get; set; } = string.Empty;
    public int NewSortOrder { get; set; }
}
