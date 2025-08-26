using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class GeneratedLetterDto
{
    public string Id { get; set; } = string.Empty;
    public string LetterNumber { get; set; } = string.Empty;
    public string LetterType { get; set; } = string.Empty;
    public string LetterTemplateId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string DigitalSignatureId { get; set; } = string.Empty;
    public string? LetterFilePath { get; set; }
    public string Status { get; set; } = "Generated";
    public DateTime? GeneratedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? EmailId { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Additional properties for email functionality
    public string Subject { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? EmailBody { get; set; }
    public string? EmailSubject { get; set; }
    public string? SentTo { get; set; }
    public string? SentBy { get; set; }
    public string EmailStatus { get; set; } = string.Empty;
    public string? EmailMessageId { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public LetterTemplateDto LetterTemplate { get; set; } = new();
    public EmployeeDto Employee { get; set; } = new();
    public DigitalSignatureDto DigitalSignature { get; set; } = new();
    public List<LetterAttachmentDto> Attachments { get; set; } = new();
}

public class CreateGeneratedLetterDto
{
    public string Subject { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string LetterTemplateId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string DigitalSignatureId { get; set; } = string.Empty;
    public string? EmailBody { get; set; }
    public string? EmailSubject { get; set; }
    public List<LetterAttachmentDto> Attachments { get; set; } = new();
}

public class UpdateGeneratedLetterDto
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? EmailBody { get; set; }
    public string? EmailSubject { get; set; }
    public List<LetterAttachmentDto> Attachments { get; set; } = new();
}
