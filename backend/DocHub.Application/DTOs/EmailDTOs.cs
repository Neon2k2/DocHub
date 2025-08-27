using System;
using System.Collections.Generic;

namespace DocHub.Application.DTOs
{
    public class EmailRequest
    {
        public string To { get; set; } = string.Empty;
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public List<EmailAttachment> Attachments { get; set; } = new();
        public Dictionary<string, string>? CustomHeaders { get; set; }
        public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }

    public class EmailHistory
    {
        public string Id { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public List<string> AttachmentNames { get; set; } = new();
        public EmailStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string SendGridMessageId { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class EmailStatusUpdate
    {
        public string MessageId { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum EmailStatus
    {
        Queued,
        Sending,
        Sent,
        Delivered,
        Failed,
        Bounced,
        Spam,
        Opened,
        Clicked
    }

    public enum EmailPriority
    {
        Low,
        Normal,
        High
    }

    public class BulkEmailRequest
    {
        public List<string> Recipients { get; set; } = new();
        public string TemplateId { get; set; } = string.Empty;
        public Dictionary<string, object> TemplateData { get; set; } = new();
        public List<EmailAttachment> CommonAttachments { get; set; } = new();
        public Dictionary<string, List<EmailAttachment>> RecipientSpecificAttachments { get; set; } = new();
        public EmailPriority Priority { get; set; } = EmailPriority.Normal;
        public bool TrackOpens { get; set; } = true;
        public bool TrackClicks { get; set; } = true;
    }

    public class BulkEmailResult
    {
        public string BatchId { get; set; } = string.Empty;
        public int TotalEmails { get; set; }
        public int SuccessfulSends { get; set; }
        public int FailedSends { get; set; }
        public List<EmailHistory> EmailHistories { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
