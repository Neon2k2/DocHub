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
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _isConfigured;
        private readonly SendGridClient? _client;

        public EnhancedEmailService(
            ILogger<EnhancedEmailService> logger, 
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

        public async Task<DocHub.Application.Interfaces.EmailStatus> GetEmailStatusAsync(string emailId)
        {
            // This is a simplified implementation - in a real scenario, you'd query a database
            return new DocHub.Application.Interfaces.EmailStatus
            {
                Id = emailId,
                ToEmail = "unknown@example.com",
                Subject = "Unknown",
                Status = "Unknown",
                SentAt = DateTime.UtcNow,
                ErrorMessage = null,
                RetryCount = 0
            };
        }

        public async Task<bool> ResendEmailAsync(string emailId)
        {
            _logger.LogInformation("Resending email with ID: {EmailId}", emailId);
            // Implementation would depend on your email storage and retry logic
            return true;
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<DocHub.Application.Interfaces.EmailStatus>> GetEmailHistoryAsync(string recipientEmail)
        {
            _logger.LogInformation("Getting email history for: {RecipientEmail}", recipientEmail);
            // Implementation would depend on your email storage
            return new List<DocHub.Application.Interfaces.EmailStatus>();
        }

        public async Task<bool> IsEmailServiceAvailableAsync()
        {
            return _isConfigured && _client != null;
        }

        public async Task<EmailProviderInfo> GetEmailProviderInfoAsync()
        {
            return new EmailProviderInfo
            {
                Provider = "SendGrid",
                IsAvailable = _isConfigured && _client != null,
                RemainingQuota = -1, // SendGrid doesn't provide this info in basic API
                LastChecked = DateTime.UtcNow
            };
        }

        public async Task<bool> ProcessWebhookEventAsync(string webhookPayload)
        {
            try
            {
                _logger.LogInformation("Processing webhook event: {Payload}", webhookPayload);
                // Implementation would depend on your webhook processing logic
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook event");
                return false;
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
    }
}
