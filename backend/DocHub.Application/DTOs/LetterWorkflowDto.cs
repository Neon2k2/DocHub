using DocHub.Core.Entities;

namespace DocHub.Application.DTOs;

public class LetterWorkflowRequest
{
    public string LetterId { get; set; } = string.Empty;
    public bool GenerateLetter { get; set; } = false;
    public bool SendEmail { get; set; } = false;
    public EmailTemplate? EmailTemplate { get; set; }
}

public class LetterWorkflowResult
{
    public string LetterId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? WorkflowDuration { get; set; }
    public bool EmailSent { get; set; } = false;
    public string? EmailId { get; set; }
}

public class BulkWorkflowRequest
{
    public List<string> LetterIds { get; set; } = new();
    public bool GenerateLetter { get; set; } = false;
    public bool SendEmail { get; set; } = false;
    public EmailTemplate? EmailTemplate { get; set; }
}

public class BulkWorkflowResult
{
    public string OperationId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? WorkflowDuration { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public List<LetterWorkflowResult> Results { get; set; } = new();
}

public class LetterWorkflowStatus
{
    public string LetterId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public List<LetterStatusHistory> StatusHistory { get; set; } = new();
    public DateTime? LastUpdated { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class LetterGenerationResult
{
    public bool Success { get; set; }
    public string LetterId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class EmailSendingResult
{
    public bool Success { get; set; }
    public string? EmailId { get; set; }
    public string? ErrorMessage { get; set; }
}
