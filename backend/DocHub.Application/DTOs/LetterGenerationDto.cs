namespace DocHub.Application.DTOs;

public class BulkLetterGenerationRequest
{
    public List<string> EmployeeIds { get; set; } = new List<string>();
    public string LetterType { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, object>? AdditionalData { get; set; }
    public bool SendEmail { get; set; } = false;
    public string? EmailSubject { get; set; }
    public string? EmailBody { get; set; }
    public string? DigitalSignatureId { get; set; }
}

public class GenerateBulkLettersRequest
{
    public string LetterTemplateId { get; set; } = string.Empty;
    public List<string> EmployeeIds { get; set; } = new List<string>();
    public string? DigitalSignatureId { get; set; }
    public Dictionary<string, string> FieldValues { get; set; } = new Dictionary<string, string>();
    public List<string> AttachmentPaths { get; set; } = new List<string>();
}

public class BulkEmailRequest
{
    public List<string> LetterIds { get; set; } = new List<string>();
    public List<string> ToEmails { get; set; } = new List<string>();
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public List<string>? CcEmails { get; set; }
    public List<string>? BccEmails { get; set; }
}

public class SendLetterRequest
{
    public string LetterId { get; set; } = string.Empty;
    public string EmailHistoryId { get; set; } = string.Empty;
    public List<string>? AdditionalAttachments { get; set; }
}

public class LetterGenerationStats
{
    public int TotalLetters { get; set; }
    public int SuccessfullyGenerated { get; set; }
    public int FailedCount { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public DateTime ProcessedAt { get; set; }
    public List<string> Errors { get; set; } = new();
}
