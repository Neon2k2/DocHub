using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class DynamicTab
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string TabType { get; set; } = string.Empty; // "letter", "database", "upload"

    [Required]
    public string DataSource { get; set; } = string.Empty; // "excel_upload", "database_query", "api"

    public string? DatabaseQuery { get; set; } // SQL query for database tabs

    public string? ExcelMapping { get; set; } // JSON mapping for Excel fields

    [Required]
    [StringLength(50)]
    public string Icon { get; set; } = string.Empty; // Material icon name

    [Required]
    [StringLength(20)]
    public string Color { get; set; } = string.Empty; // Color code (e.g., #2196F3)

    public int SortOrder { get; set; } = 0; // For ordering tabs

    public bool IsActive { get; set; } = true;

    public string? TemplateId { get; set; } // Associated letter template

    public string? FieldMappings { get; set; } // JSON field mappings

    public string? Permissions { get; set; } // Required permissions to access

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual LetterTemplate? Template { get; set; }
    public virtual ICollection<DynamicTabField> Fields { get; set; } = new List<DynamicTabField>();
    public virtual ICollection<DynamicTabData> DataRecords { get; set; } = new List<DynamicTabData>();
}

public class DynamicTabField
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string DynamicTabId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = string.Empty; // "string", "number", "date", "boolean"

    public bool IsRequired { get; set; } = false;

    public bool IsEditable { get; set; } = true;

    public bool IsVisible { get; set; } = true;

    public string? ValidationRules { get; set; } // JSON validation rules

    public string? DefaultValue { get; set; }

    public int SortOrder { get; set; } = 0;

    public string? ExcelColumnName { get; set; } // Excel column mapping

    public string? DatabaseColumnName { get; set; } // Database column mapping

    // Navigation property
    public virtual DynamicTab DynamicTab { get; set; } = null!;
}

public class DynamicTabData
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string DynamicTabId { get; set; } = string.Empty;

    [Required]
    public string DataSource { get; set; } = string.Empty; // "excel", "database", "manual"

    public string? ExternalId { get; set; } // ID from external source

    public string? DataContent { get; set; } // JSON data content

    public string? Status { get; set; } = "active";

    public DateTime? ProcessedAt { get; set; }

    public string? ProcessedBy { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual DynamicTab DynamicTab { get; set; } = null!;
}
