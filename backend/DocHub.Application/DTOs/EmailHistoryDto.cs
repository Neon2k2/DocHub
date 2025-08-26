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

public class EmailDeliveryStatus
{
    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // sent, delivered, failed, bounced, etc.
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public string? ProviderResponse { get; set; }
}

public class EmailAnalytics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalEmailsSent { get; set; }
    public int TotalEmailsDelivered { get; set; }
    public int TotalEmailsFailed { get; set; }
    public double DeliveryRate { get; set; }
    public TimeSpan AverageDeliveryTime { get; set; }
    public List<string> TopRecipients { get; set; } = new List<string>();
    public List<string> TopSubjects { get; set; } = new List<string>();
    public Dictionary<string, int> StatusBreakdown { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> ProviderBreakdown { get; set; } = new Dictionary<string, int>();
    public string? Error { get; set; }
}

public class ScheduledEmailDto
{
    public string Id { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public string Status { get; set; } = string.Empty; // scheduled, sent, cancelled, failed
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<string> AttachmentPaths { get; set; } = new List<string>();
    public string? TemplateId { get; set; }
    public Dictionary<string, string>? TemplateData { get; set; }
}

public class CreateScheduledEmailRequest
{
    [Required]
    [EmailAddress]
    public string ToEmail { get; set; } = string.Empty;
    
    [Required]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    [Required]
    public DateTime ScheduledTime { get; set; }
    
    public List<string> AttachmentPaths { get; set; } = new List<string>();
    public string? TemplateId { get; set; }
    public Dictionary<string, string>? TemplateData { get; set; }
}

public class EmailTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // welcome, password-reset, letter-generated, etc.
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();
}

public class EmailProviderInfo
{
    public string Provider { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int RemainingQuota { get; set; } // -1 means unlimited or unknown
    public DateTime LastChecked { get; set; }
    public string? Status { get; set; }
    public Dictionary<string, object>? AdditionalInfo { get; set; }
}

public class EmailStatus
{
    public string Id { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}
