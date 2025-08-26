using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class LetterTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LetterType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TemplateFilePath { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty; // "upload" or "database"
    public string? DatabaseQuery { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<LetterTemplateFieldDto> Fields { get; set; } = new();
}

public class CreateLetterTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string LetterType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TemplateFilePath { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string? DatabaseQuery { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public List<CreateLetterTemplateFieldDto> Fields { get; set; } = new();
}

public class UpdateLetterTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LetterType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TemplateFilePath { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string? DatabaseQuery { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public List<UpdateLetterTemplateFieldDto> Fields { get; set; } = new();
}


