namespace DocHub.Application.DTOs;

public class EmailStatusDto
{
    public string EmailId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ErrorDetails { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryTime { get; set; }
}

public enum EmailStatus
{
    Pending,
    Sending,
    Sent,
    Delivered,
    Failed,
    Retrying
}
