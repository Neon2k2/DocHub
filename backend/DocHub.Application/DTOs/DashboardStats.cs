namespace DocHub.Application.DTOs;

public class DashboardStats
{
    public int TotalTemplates { get; set; }
    public int ActiveTemplates { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalLetters { get; set; }
    public int PendingEmails { get; set; }
    public int SentEmails { get; set; }
    public int FailedEmails { get; set; }
    public int TotalSignatures { get; set; }
    public int ActiveSignatures { get; set; }
    public int TotalAttachments { get; set; }
    public long TotalStorageUsed { get; set; } // in bytes
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public List<EmailStatusSummary> EmailStatusSummary { get; set; } = new();
}

public class RecentActivity
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "letter_generated", "email_sent", "template_created", etc.
    public string Description { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? RelatedEntityId { get; set; }
    public string? RelatedEntityName { get; set; }
}

public class EmailStatusSummary
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public int SentEmails { get; set; }
    public int FailedEmails { get; set; }
    public int PendingEmails { get; set; }
    public int DeliveredEmails { get; set; }
    public int OpenedEmails { get; set; }
    public int BouncedEmails { get; set; }
    public double DeliveryRate { get; set; }
    public double OpenRate { get; set; }
}
