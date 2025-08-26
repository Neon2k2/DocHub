using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class EmailHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string? CcEmail { get; set; }
    public string? BccEmail { get; set; }
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EmailProvider { get; set; }
    public string? EmailId { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public string? GeneratedLetterId { get; set; }
    public string? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? LetterType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public ICollection<EmailAttachmentDto> Attachments { get; set; } = new List<EmailAttachmentDto>();
}

public class CreateEmailHistoryDto
{
    [Required]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string ToEmail { get; set; } = string.Empty;
    
    [EmailAddress]
    public string? CcEmail { get; set; }
    
    [EmailAddress]
    public string? BccEmail { get; set; }
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public string Status { get; set; } = "Pending";
    
    public string? GeneratedLetterId { get; set; }
    
    public string? EmployeeId { get; set; }
    
    public string? EmailProvider { get; set; }
}

public class UpdateEmailHistoryDto
{
    public string? Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EmailId { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
}

public class CreateEmailHistoryRequest
{
    [Required]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string ToEmail { get; set; } = string.Empty;
    
    [EmailAddress]
    public string? CcEmail { get; set; }
    
    [EmailAddress]
    public string? BccEmail { get; set; }
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public string? GeneratedLetterId { get; set; }
    
    public string? EmployeeId { get; set; }
    
    public string? EmailProvider { get; set; }
    
    public ICollection<AddEmailAttachmentRequest> Attachments { get; set; } = new List<AddEmailAttachmentRequest>();
}

public class ResendEmailRequest
{
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? CcEmail { get; set; }
    public string? BccEmail { get; set; }
    public bool UseLatestSignature { get; set; } = true;
    public ICollection<AddEmailAttachmentRequest> AdditionalAttachments { get; set; } = new List<AddEmailAttachmentRequest>();
}

public class AddEmailAttachmentRequest
{
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    public string? FileType { get; set; }
    
    [Required]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
}

public class EmailAttachmentDto
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
