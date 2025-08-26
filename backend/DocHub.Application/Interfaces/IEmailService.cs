using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, List<string>? attachmentPaths = null);
        Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, List<string>? attachmentPaths = null);
        Task<bool> SendBulkEmailsAsync(List<string> toEmails, string subject, string body, List<string>? attachmentPaths = null);
        Task<bool> SendEmailWithTemplateAsync(string toEmail, string templateName, Dictionary<string, object> templateData, List<string>? attachmentPaths = null);
        Task<bool> SendEmailWithHtmlAsync(string toEmail, string subject, string htmlBody, List<string>? attachmentPaths = null);
        Task<bool> SendEmailWithAttachmentsAsync(string toEmail, string subject, string body, List<EmailAttachment> attachments);
        Task<EmailStatus> GetEmailStatusAsync(string emailId);
        Task<bool> ResendEmailAsync(string emailId);
        Task<bool> ValidateEmailAsync(string email);
        Task<List<EmailStatus>> GetEmailHistoryAsync(string recipientEmail);
        Task<bool> IsEmailServiceAvailableAsync();
        Task<EmailProviderInfo> GetEmailProviderInfoAsync();
        Task<bool> ProcessWebhookEventAsync(string webhookPayload);
    }

    public class EmailAttachment
    {
        public required string FileName { get; set; }
        public required byte[] Content { get; set; }
        public required string ContentType { get; set; }
    }

    public class EmailStatus
    {
        public required string Id { get; set; }
        public required string ToEmail { get; set; }
        public required string Subject { get; set; }
        public required string Status { get; set; }
        public DateTime SentAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
    }

    public class EmailProviderInfo
    {
        public required string Provider { get; set; }
        public bool IsAvailable { get; set; }
        public int RemainingQuota { get; set; }
        public DateTime LastChecked { get; set; }
    }
}
