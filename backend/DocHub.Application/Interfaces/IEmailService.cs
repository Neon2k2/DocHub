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
        
        // New enhanced email features
        Task<bool> SendTemplatedEmailAsync(string toEmail, string templateId, Dictionary<string, string> templateData, List<string>? attachmentPaths = null);
        Task<EmailDeliveryStatus> GetEmailDeliveryStatusAsync(string messageId);
        Task<bool> SendScheduledEmailAsync(string toEmail, string subject, string body, DateTime scheduledTime, List<string>? attachmentPaths = null);
        Task<bool> CancelScheduledEmailAsync(string messageId);
        Task<EmailAnalytics> GetEmailAnalyticsAsync(DateTime startDate, DateTime endDate);
    }

    public class EmailAttachment
    {
        public required string FileName { get; set; }
        public required byte[] Content { get; set; }
        public required string ContentType { get; set; }
    }
}
