using System;
using System.Collections.Generic;
using DocHub.Core.Entities;

namespace DocHub.Application.DTOs.DynamicTabs;

public class DynamicTabDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TabType { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string? DatabaseQuery { get; set; }
    public string? ExcelMapping { get; set; }
    public string? TemplateId { get; set; }
    public string? FieldMappings { get; set; }
    public int SortOrder { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Permissions { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdminOnly { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public ICollection<DynamicTabFieldDto> Fields { get; set; } = new List<DynamicTabFieldDto>();
    public LetterTemplate? Template { get; set; }
}

public class DynamicTabFieldDto
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
    public int SortOrder { get; set; }
    public string? ExcelColumnName { get; set; }
    public string? DatabaseColumnName { get; set; }
}

public class DynamicTabDataDto
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
