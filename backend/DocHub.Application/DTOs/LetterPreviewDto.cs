using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class LetterPreviewDto
{
    public string Id { get; set; } = string.Empty;
    public string LetterType { get; set; } = string.Empty;
    public string LetterTemplateId { get; set; } = string.Empty;
    public string LetterTemplateName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? DigitalSignatureId { get; set; }
    public string? AuthorityName { get; set; }
    public string? AuthorityDesignation { get; set; }
    public string PreviewContent { get; set; } = string.Empty;
    public string? PreviewFilePath { get; set; }
    public string? PreviewImagePath { get; set; }
    public DateTime? LastGeneratedAt { get; set; }
    public bool IsActive { get; set; }
    public string? GeneratedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public ICollection<LetterAttachmentDto> Attachments { get; set; } = new List<LetterAttachmentDto>();
}

public class CreateLetterPreviewDto
{
    [Required]
    public string LetterType { get; set; } = string.Empty;
    
    [Required]
    public string LetterTemplateId { get; set; } = string.Empty;
    
    [Required]
    public string EmployeeId { get; set; } = string.Empty;
    
    public string? DigitalSignatureId { get; set; }
    
    [Required]
    public string PreviewContent { get; set; } = string.Empty;
    
    public string? PreviewFilePath { get; set; }
    
    public string? PreviewImagePath { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string? GeneratedBy { get; set; }
}

public class UpdateLetterPreviewDto
{
    public string? PreviewContent { get; set; }
    public string? PreviewFilePath { get; set; }
    public string? PreviewImagePath { get; set; }
    public string? DigitalSignatureId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePreviewRequest
{
    public string? PreviewContent { get; set; }
    public string? PreviewFilePath { get; set; }
    public string? PreviewImagePath { get; set; }
    public string? DigitalSignatureId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BulkPreviewRequest
{
    [Required]
    public string LetterTemplateId { get; set; } = string.Empty;
    
    [Required]
    public List<string> EmployeeIds { get; set; } = new List<string>();
    
    public string? DigitalSignatureId { get; set; }
    
    public bool UseLatestSignature { get; set; } = true;
    
    public bool OverwriteExisting { get; set; } = false;
}

public class ClonePreviewRequest
{
    public string NewEmployeeId { get; set; } = string.Empty;
}

public class PreviewGenerationResult
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PreviewId { get; set; }
    public string? PreviewFilePath { get; set; }
}

public class BulkPreviewResult
{
    public int TotalRequested { get; set; }
    public int SuccessfullyGenerated { get; set; }
    public int Failed { get; set; }
    public List<PreviewGenerationResult> Results { get; set; } = new List<PreviewGenerationResult>();
    public DateTime CompletedAt { get; set; }
}

public class GeneratePreviewRequest
{
    public string LetterTemplateId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string? DigitalSignatureId { get; set; }
}
