using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class BulkLetterGenerationRequestDto
{
    [Required]
    public string LetterTemplateId { get; set; } = string.Empty;
    
    [Required]
    public List<string> EmployeeIds { get; set; } = new();
    
    public string? DigitalSignatureId { get; set; }
    
    public bool UseLatestSignature { get; set; } = true;
    
    public Dictionary<string, object>? CommonData { get; set; }
    
    public bool GeneratePreviews { get; set; } = true;
    
    public bool SendEmails { get; set; } = false;
    
    public Dictionary<string, object>? CustomFields { get; set; }
    
    public bool GeneratePreview { get; set; } = true;
    
    public bool SendEmail { get; set; } = false;
}

public class BulkPreviewGenerationRequest
{
    [Required]
    public string LetterTemplateId { get; set; } = string.Empty;
    
    [Required]
    public List<string> EmployeeIds { get; set; } = new();
    
    public string? DigitalSignatureId { get; set; }
    
    public bool UseLatestSignature { get; set; } = true;
    
    public bool OverwriteExisting { get; set; } = false;
    
    public Dictionary<string, object>? FieldValues { get; set; }
}

public class BulkEmailSendingRequest
{
    [Required]
    public List<string> LetterIds { get; set; } = new();
    
    public string? CcEmail { get; set; }
    
    public string? BccEmail { get; set; }
    
    public string? Subject { get; set; }
    
    public string? Body { get; set; }
    
    public List<string>? AdditionalAttachments { get; set; }
    
    public bool UseLatestSignature { get; set; } = true;
}

public class RetryBulkOperationRequest
{
    public List<string> ItemIds { get; set; } = new();
    
    public bool RegenerateLetters { get; set; } = false;
    
    public bool ResendEmails { get; set; } = true;
}

public class BulkLetterGenerationResult
{
    public string OperationId { get; set; } = string.Empty;
    public int TotalRequested { get; set; }
    public int SuccessfullyGenerated { get; set; }
    public int Failed { get; set; }
    public List<LetterGenerationItemResult> Results { get; set; } = new();
    public DateTime CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LetterGenerationItemResult
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? LetterId { get; set; }
    public string? PreviewId { get; set; }
    public string? LetterNumber { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BulkPreviewGenerationResult
{
    public string OperationId { get; set; } = string.Empty;
    public int TotalRequested { get; set; }
    public int SuccessfullyGenerated { get; set; }
    public int Failed { get; set; }
    public List<PreviewGenerationItemResult> Results { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

public class PreviewGenerationItemResult
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? PreviewId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BulkEmailSendingResult
{
    public string OperationId { get; set; } = string.Empty;
    public int TotalRequested { get; set; }
    public int SuccessfullySent { get; set; }
    public int Failed { get; set; }
    public List<EmailSendingItemResult> Results { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

public class EmailSendingItemResult
{
    public string LetterId { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? EmailId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BulkOperationStatusDto
{
    public string OperationId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public int FailedItems { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BulkOperationRetryResult
{
    public string OperationId { get; set; } = string.Empty;
    public int TotalRetried { get; set; }
    public int SuccessfullyRetried { get; set; }
    public int Failed { get; set; }
    public List<string> RetriedItemIds { get; set; } = new();
}

public class BulkOperationHistoryDto
{
    public string OperationId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public int FailedItems { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
}

public class BulkOperationHistoryFilter
{
    public string? OperationType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? InitiatedBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class BulkOperationStatsDto
{
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public int PendingOperations { get; set; }
    public double SuccessRate { get; set; }
    public int TotalItemsProcessed { get; set; }
    public DateTime LastOperationAt { get; set; }
    public Dictionary<string, int> OperationsByType { get; set; } = new();
}
