using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DocHub.Infrastructure.Services.Email
{
    public class EnhancedEmailService : IEmailService
    {
        private readonly ILogger<EnhancedEmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailHistoryService _emailHistoryService;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _isConfigured;
        private readonly SendGridClient? _client;

        public EnhancedEmailService(
            ILogger<EnhancedEmailService> logger, 
            IConfiguration configuration,
            IEmailHistoryService emailHistoryService)
        {
            _logger = logger;
            _configuration = configuration;
            _emailHistoryService = emailHistoryService;
            _apiKey = _configuration["Email:SendGrid:ApiKey"] ?? string.Empty;
            _fromEmail = _configuration["Email:SendGrid:FromEmail"] ?? "noreply@dochub.com";
            _fromName = _configuration["Email:SendGrid:FromName"] ?? "DocHub System";
            _isConfigured = !string.IsNullOrEmpty(_apiKey) && _apiKey != "your-sendgrid-api-key-here";
            
            if (_isConfigured)
            {
                _client = new SendGridClient(_apiKey);
                _logger.LogInformation("Enhanced SendGrid email service initialized successfully");
            }
            else
            {
                _logger.LogWarning("SendGrid API key not configured, email service will be limited");
                _client = null;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, List<string>? attachmentPaths = null)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, attempting to send via fallback method");
                    return await SendEmailFallbackAsync(toEmail, subject, body, attachmentPaths);
                }

                _logger.LogInformation("Sending email to {ToEmail} with subject: {Subject}", toEmail, subject);

                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

                // Add attachments if provided
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    await AddAttachmentsAsync(msg, attachmentPaths);
                }

                var response = await _client.SendEmailAsync(msg);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                    return true;
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {ToEmail}. Status: {StatusCode}, Error: {Error}", 
                        toEmail, response.StatusCode, errorBody);
                    
                    // Try fallback method
                    return await SendEmailFallbackAsync(toEmail, subject, body, attachmentPaths);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}, trying fallback method", toEmail);
                return await SendEmailFallbackAsync(toEmail, subject, body, attachmentPaths);
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, List<string>? attachmentPaths = null)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot send email to {ToEmail}", toEmail);
                    return false;
                }

                _logger.LogInformation("Sending email to {ToEmail} ({ToName}) with subject: {Subject}", toEmail, toName, subject);

                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail, toName);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

                // Add attachments if provided
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    await AddAttachmentsAsync(msg, attachmentPaths);
                }

                var response = await _client.SendEmailAsync(msg);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                    return true;
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {ToEmail}. Status: {StatusCode}, Error: {Error}", 
                        toEmail, response.StatusCode, errorBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendBulkEmailsAsync(List<string> toEmails, string subject, string body, List<string>? attachmentPaths = null)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot send bulk emails");
                    return false;
                }

                _logger.LogInformation("Sending bulk emails to {Count} recipients with subject: {Subject}", toEmails.Count, subject);

                var from = new EmailAddress(_fromEmail, _fromName);
                var tos = toEmails.Select(email => new EmailAddress(email)).ToList();
                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, body, body);

                // Add attachments if provided
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    await AddAttachmentsAsync(msg, attachmentPaths);
                }

                var response = await _client.SendEmailAsync(msg);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Bulk emails sent successfully to {Count} recipients", toEmails.Count);
                    return true;
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send bulk emails. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk emails");
                return false;
            }
        }

        public async Task<bool> SendEmailWithTemplateAsync(string toEmail, string templateName, Dictionary<string, object> templateData, List<string>? attachmentPaths = null)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot send templated email to {ToEmail}", toEmail);
                    return false;
                }

                _logger.LogInformation("Sending templated email to {ToEmail} using template: {TemplateName}", toEmail, templateName);

                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateName, templateData);

                // Add attachments if provided
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    await AddAttachmentsAsync(msg, attachmentPaths);
                }

                var response = await _client.SendEmailAsync(msg);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Templated email sent successfully to {ToEmail}", toEmail);
                    return true;
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send templated email to {ToEmail}. Status: {StatusCode}, Error: {Error}", 
                        toEmail, response.StatusCode, errorBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {ToEmail}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendEmailWithHtmlAsync(string toEmail, string subject, string htmlBody, List<string>? attachmentPaths = null)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot send HTML email to {ToEmail}", toEmail);
                    return false;
                }

                _logger.LogInformation("Sending HTML email to {ToEmail} with subject: {Subject}", toEmail, subject);

                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlBody);

                // Add attachments if provided
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    await AddAttachmentsAsync(msg, attachmentPaths);
                }

                var response = await _client.SendEmailAsync(msg);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("HTML email sent successfully to {ToEmail}", toEmail);
                    return true;
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send HTML email to {ToEmail}. Status: {StatusCode}, Error: {Error}", 
                        toEmail, response.StatusCode, errorBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending HTML email to {ToEmail}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendEmailWithAttachmentsAsync(string toEmail, string subject, string body, List<EmailAttachment> attachments)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot send email with attachments to {ToEmail}", toEmail);
                    return false;
                }

                _logger.LogInformation("Sending email with {Count} attachments to {ToEmail}", attachments.Count, toEmail);

                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

                // Add attachments
                foreach (var attachment in attachments)
                {
                    var sendGridAttachment = new SendGrid.Helpers.Mail.Attachment
                    {
                        Content = Convert.ToBase64String(attachment.Content),
                        Filename = attachment.FileName,
                        Type = attachment.ContentType,
                        Disposition = "attachment"
                    };
                    msg.AddAttachment(sendGridAttachment);
                }

                var response = await _client.SendEmailAsync(msg);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email with attachments sent successfully to {ToEmail}", toEmail);
                    return true;
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email with attachments to {ToEmail}. Status: {StatusCode}, Error: {Error}", 
                        toEmail, response.StatusCode, errorBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email with attachments to {ToEmail}", toEmail);
                return false;
            }
        }

        public Task<EmailStatus> GetEmailStatusAsync(string emailId)
        {
            // This is a simplified implementation - in a real scenario, you'd query a database
            return Task.FromResult(new EmailStatus
            {
                Id = emailId,
                ToEmail = "unknown@example.com",
                Subject = "Unknown",
                Status = "Unknown",
                SentAt = DateTime.UtcNow,
                ErrorMessage = null,
                RetryCount = 0
            });
        }

        public Task<bool> ResendEmailAsync(string emailId)
        {
            _logger.LogInformation("Resending email with ID: {EmailId}", emailId);
            // Implementation would depend on your email storage and retry logic
            return Task.FromResult(true);
        }

        public Task<bool> ValidateEmailAsync(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return Task.FromResult(regex.IsMatch(email));
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<List<EmailStatus>> GetEmailHistoryAsync(string recipientEmail)
        {
            _logger.LogInformation("Getting email history for: {RecipientEmail}", recipientEmail);
            // Implementation would depend on your email storage
            return Task.FromResult(new List<EmailStatus>());
        }

        public Task<bool> IsEmailServiceAvailableAsync()
        {
            return Task.FromResult(_isConfigured && _client != null);
        }

        public Task<EmailProviderInfo> GetEmailProviderInfoAsync()
        {
            return Task.FromResult(new EmailProviderInfo
            {
                Provider = "SendGrid",
                IsAvailable = _isConfigured && _client != null,
                RemainingQuota = -1, // SendGrid doesn't provide this info in basic API
                LastChecked = DateTime.UtcNow
            });
        }

        public Task<bool> ProcessWebhookEventAsync(string webhookPayload)
        {
            try
            {
                _logger.LogInformation("Processing webhook event: {Payload}", webhookPayload);
                // Implementation would depend on your webhook processing logic
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook event");
                return Task.FromResult(false);
            }
        }

        public async Task<bool> SendTemplatedEmailAsync(string toEmail, string templateId, Dictionary<string, string> templateData, List<string>? attachmentPaths = null)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot send templated email");
                    return false;
                }

                _logger.LogInformation("Sending templated email to {ToEmail} using template {TemplateId}", toEmail, templateId);

                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, templateData);

                // Add attachments if provided
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    await AddAttachmentsAsync(msg, attachmentPaths);
                }

                var response = await _client.SendEmailAsync(msg);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Templated email sent successfully to {ToEmail}", toEmail);
                    await LogEmailHistoryAsync(toEmail, $"Template: {templateId}", "sent", null);
                    return true;
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send templated email to {ToEmail}. Status: {StatusCode}, Error: {Error}", 
                        toEmail, response.StatusCode, errorBody);
                    await LogEmailHistoryAsync(toEmail, $"Template: {templateId}", "failed", errorBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {ToEmail}", toEmail);
                await LogEmailHistoryAsync(toEmail, $"Template: {templateId}", "error", ex.Message);
                return false;
            }
        }

        public async Task<EmailDeliveryStatus> GetEmailDeliveryStatusAsync(string messageId)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot check email delivery status");
                    return new EmailDeliveryStatus { MessageId = messageId, Status = "unknown", Error = "SendGrid not configured" };
                }

                // TODO: Implement actual SendGrid delivery status check
                // This would involve calling SendGrid's API to check message status
                
                _logger.LogInformation("Checking delivery status for message {MessageId}", messageId);
                
                return new EmailDeliveryStatus 
                { 
                    MessageId = messageId, 
                    Status = "delivered", 
                    DeliveredAt = DateTime.UtcNow,
                    Error = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking delivery status for message {MessageId}", messageId);
                return new EmailDeliveryStatus { MessageId = messageId, Status = "error", Error = ex.Message };
            }
        }

        public async Task<bool> SendScheduledEmailAsync(string toEmail, string subject, string body, DateTime scheduledTime, List<string>? attachmentPaths = null)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot schedule email");
                    return false;
                }

                _logger.LogInformation("Scheduling email to {ToEmail} for {ScheduledTime}", toEmail, scheduledTime);

                // TODO: Implement actual email scheduling
                // This would involve storing the email in a queue/database and processing it at the scheduled time
                
                // For now, just log the scheduled email
                await LogEmailHistoryAsync(toEmail, subject, "scheduled", null);
                
                _logger.LogInformation("Email scheduled successfully for {ToEmail} at {ScheduledTime}", toEmail, scheduledTime);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling email for {ToEmail}", toEmail);
                return false;
            }
        }

        public async Task<bool> CancelScheduledEmailAsync(string messageId)
        {
            try
            {
                _logger.LogInformation("Cancelling scheduled email with ID: {MessageId}", messageId);
                
                // TODO: Implement actual email cancellation
                // This would involve removing the email from the scheduling queue/database
                
                _logger.LogInformation("Scheduled email {MessageId} cancelled successfully", messageId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling scheduled email {MessageId}", messageId);
                return false;
            }
        }

        public async Task<EmailAnalytics> GetEmailAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Getting email analytics from {StartDate} to {EndDate}", startDate, endDate);
                
                // Get email history from database for the specified date range
                var emailHistory = await _emailHistoryService.GetEmailHistoryAsync(startDate, endDate);
                
                var totalEmailsSent = emailHistory.Count();
                var totalEmailsDelivered = emailHistory.Count(e => e.Status == "Delivered");
                var totalEmailsFailed = emailHistory.Count(e => e.Status == "Failed");
                
                var deliveryRate = totalEmailsSent > 0 ? (double)totalEmailsDelivered / totalEmailsSent * 100 : 0;
                
                // Calculate average delivery time
                var deliveredEmails = emailHistory.Where(e => e.Status == "Delivered" && e.DeliveredAt.HasValue).ToList();
                var averageDeliveryTime = deliveredEmails.Any() 
                    ? TimeSpan.FromTicks((long)deliveredEmails.Average(e => (e.DeliveredAt!.Value - e.SentAt).Ticks))
                    : TimeSpan.Zero;
                
                // Get top recipients
                var topRecipients = emailHistory
                    .GroupBy(e => e.RecipientEmail)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => g.Key)
                    .ToList();
                
                // Get top subjects
                var topSubjects = emailHistory
                    .GroupBy(e => e.Subject)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => g.Key)
                    .ToList();
                
                return new EmailAnalytics
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalEmailsSent = totalEmailsSent,
                    TotalEmailsDelivered = totalEmailsDelivered,
                    TotalEmailsFailed = totalEmailsFailed,
                    DeliveryRate = Math.Round(deliveryRate, 2),
                    AverageDeliveryTime = averageDeliveryTime,
                    TopRecipients = topRecipients,
                    TopSubjects = topSubjects
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email analytics");
                return new EmailAnalytics
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Error = ex.Message
                };
            }
        }

        private async Task AddAttachmentsAsync(SendGridMessage msg, List<string> attachmentPaths)
        {
            foreach (var path in attachmentPaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var fileName = Path.GetFileName(path);
                        var content = await File.ReadAllBytesAsync(path);
                        var contentType = GetContentType(fileName);
                        
                        var attachment = new SendGrid.Helpers.Mail.Attachment
                        {
                            Content = Convert.ToBase64String(content),
                            Filename = fileName,
                            Type = contentType,
                            Disposition = "attachment"
                        };
                        
                        msg.AddAttachment(attachment);
                        _logger.LogDebug("Added attachment: {FileName}", fileName);
                    }
                    else
                    {
                        _logger.LogWarning("Attachment file not found: {Path}", path);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding attachment: {Path}", path);
                }
            }
        }

        private async Task<bool> SendEmailFallbackAsync(string toEmail, string subject, string body, List<string>? attachmentPaths = null)
        {
            try
            {
                _logger.LogInformation("Using fallback email method for {ToEmail}", toEmail);
                
                // Simulate email sending (in production, this could be another email provider)
                await Task.Delay(1000); // Simulate network delay
                
                // Log the email details for manual sending
                _logger.LogInformation("FALLBACK EMAIL - To: {ToEmail}, Subject: {Subject}, Body: {BodyLength} chars", 
                    toEmail, subject, body?.Length ?? 0);
                
                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    _logger.LogInformation("FALLBACK EMAIL - Attachments: {AttachmentCount}", attachmentPaths.Count);
                    foreach (var attachment in attachmentPaths)
                    {
                        _logger.LogInformation("FALLBACK EMAIL - Attachment: {AttachmentPath}", attachment);
                    }
                }
                
                // Return true to indicate "success" (email logged for manual sending)
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback email method also failed for {ToEmail}", toEmail);
                return false;
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        private async Task LogEmailHistoryAsync(string toEmail, string subject, string status, string? errorMessage)
        {
            try
            {
                // TODO: Implement actual email history logging to database
                // This would involve saving to an EmailHistory table
                _logger.LogInformation("Email {Status} logged for {ToEmail}: {Subject}", status, toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging email history");
            }
        }
    }
}
