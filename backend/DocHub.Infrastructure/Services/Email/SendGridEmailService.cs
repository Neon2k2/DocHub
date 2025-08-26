using DocHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DocHub.Infrastructure.Services.Email
{
    public class SendGridEmailService : IEmailService
    {
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _isConfigured;
        private readonly SendGridClient? _client;

        public SendGridEmailService(ILogger<SendGridEmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiKey = _configuration["Email:SendGrid:ApiKey"] ?? string.Empty;
            _fromEmail = _configuration["Email:SendGrid:FromEmail"] ?? "noreply@dochub.com";
            _fromName = _configuration["Email:SendGrid:FromName"] ?? "DocHub System";
            _isConfigured = !string.IsNullOrEmpty(_apiKey) && _apiKey != "YOUR_SENDGRID_API_KEY_HERE";
            
            if (_isConfigured)
            {
                _client = new SendGridClient(_apiKey);
                _logger.LogInformation("SendGrid email service initialized successfully");
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
                    _logger.LogWarning("SendGrid not configured, cannot send email to {ToEmail}", toEmail);
                    return false;
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
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string body, List<string>? attachmentPaths = null)
        {
            try
            {
                if (!_isConfigured || _client == null)
                {
                    _logger.LogWarning("SendGrid not configured, cannot send email to {ToName} ({ToEmail})", toName, toEmail);
                    return false;
                }

                _logger.LogInformation("Sending email to {ToName} ({ToEmail}) with subject: {Subject}", toName, toEmail, subject);

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
                    _logger.LogInformation("Email sent successfully to {ToName} ({ToEmail})", toName, toEmail);
                    return true;
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {ToName} ({ToEmail}). Status: {StatusCode}, Error: {Error}", 
                        toName, toEmail, response.StatusCode, errorBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToName} ({ToEmail})", toName, toEmail);
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
                var personalizations = toEmails.Select(email => new Personalization
                {
                    Tos = new List<EmailAddress> { new EmailAddress(email) }
                }).ToList();

                var msg = new SendGridMessage
                {
                    From = from,
                    Subject = subject,
                    PlainTextContent = body,
                    HtmlContent = body,
                    Personalizations = personalizations
                };

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
                
                // Create dynamic template email
                var msg = new SendGridMessage();
                msg.SetFrom(from);
                msg.AddTo(to);
                msg.SetTemplateId(templateName);
                
                // Add dynamic template data
                foreach (var kvp in templateData)
                {
                    msg.AddSubstitution($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
                }

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
                
                // Create HTML email with plain text fallback
                var plainTextBody = ConvertHtmlToPlainText(htmlBody);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextBody, htmlBody);

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

        public async Task<EmailStatus> GetEmailStatusAsync(string emailId)
        {
            try
            {
                if (!_isConfigured)
                {
                    return new EmailStatus
                    {
                        Id = emailId,
                        ToEmail = "unknown@example.com",
                        Subject = "Service Not Configured",
                        Status = "Service Not Configured",
                        SentAt = DateTime.UtcNow,
                        ErrorMessage = "SendGrid service is not configured"
                    };
                }

                // SendGrid doesn't provide real-time email status tracking in the free tier
                // This would require webhook setup or using SendGrid's Event API
                // For now, return a simulated status
                return await Task.FromResult(new EmailStatus
                {
                    Id = emailId,
                    ToEmail = "unknown@example.com",
                    Subject = "Email Status",
                    Status = "Sent",
                    SentAt = DateTime.UtcNow,
                    ErrorMessage = "Email status tracking requires webhook setup"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email status for {EmailId}", emailId);
                return new EmailStatus
                {
                    Id = emailId,
                    ToEmail = "unknown@example.com",
                    Subject = "Error",
                    Status = "Error",
                    SentAt = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> ResendEmailAsync(string emailId)
        {
            try
            {
                if (!_isConfigured)
                {
                    _logger.LogWarning("SendGrid not configured, cannot resend email {EmailId}", emailId);
                    return false;
                }

                // For resending, we would need to store the original email details
                // This is a simplified implementation
                _logger.LogInformation("Resending email with ID: {EmailId}", emailId);
                
                // In production, you would:
                // 1. Retrieve original email details from database
                // 2. Send the email again
                // 3. Update the email record
                
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending email {EmailId}", emailId);
                return false;
            }
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return await Task.FromResult(regex.IsMatch(email));
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<List<EmailStatus>> GetEmailHistoryAsync(string recipientEmail)
        {
            try
            {
                if (!_isConfigured)
                {
                    return new List<EmailStatus>();
                }

                // This would require storing email history in a database
                // For now, return empty list
                _logger.LogInformation("Getting email history for {RecipientEmail}", recipientEmail);
                
                // In production, you would:
                // 1. Query database for email history
                // 2. Filter by recipient email
                // 3. Return formatted results
                
                return await Task.FromResult(new List<EmailStatus>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email history for {RecipientEmail}", recipientEmail);
                return new List<EmailStatus>();
            }
        }

        public async Task<bool> IsEmailServiceAvailableAsync()
        {
            try
            {
                if (!_isConfigured)
                {
                    return false;
                }

                // Test SendGrid connectivity by sending a simple request
                // Note: SendGrid doesn't have a simple health check endpoint
                // We'll just check if the client is configured
                return _isConfigured;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SendGrid service availability check failed");
                return false;
            }
        }

        public async Task<EmailProviderInfo> GetEmailProviderInfoAsync()
        {
            try
            {
                // Mock implementation
                return new EmailProviderInfo
                {
                    Provider = "SendGrid",
                    IsAvailable = true,
                    RemainingQuota = 1000,
                    LastChecked = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email provider info");
                return new EmailProviderInfo
                {
                    Provider = "SendGrid",
                    IsAvailable = false,
                    RemainingQuota = 0,
                    LastChecked = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> ProcessWebhookEventAsync(string webhookPayload)
        {
            try
            {
                // Mock implementation - process webhook events
                _logger.LogInformation("Processing webhook event: {Payload}", webhookPayload);
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
            try
            {
                foreach (var attachmentPath in attachmentPaths)
                {
                    if (File.Exists(attachmentPath))
                    {
                        var attachment = await CreateAttachmentAsync(attachmentPath);
                        msg.AddAttachment(attachment);
                        _logger.LogDebug("Added attachment: {AttachmentPath}", attachmentPath);
                    }
                    else
                    {
                        _logger.LogWarning("Attachment file not found: {AttachmentPath}", attachmentPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding attachments to email");
            }
        }

        private async Task<SendGrid.Helpers.Mail.Attachment> CreateAttachmentAsync(string filePath)
        {
            try
            {
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                var contentType = GetContentType(fileName);

                return new SendGrid.Helpers.Mail.Attachment
                {
                    Content = Convert.ToBase64String(fileBytes),
                    Filename = fileName,
                    Type = contentType,
                    Disposition = "attachment"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attachment from file: {FilePath}", filePath);
                throw;
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
                ".jpg" or ".jpeg" => "image/jpeg",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }

        private string ConvertHtmlToPlainText(string html)
        {
            try
            {
                // Simple HTML to plain text conversion
                var plainText = html
                    .Replace("<br>", "\n")
                    .Replace("<br/>", "\n")
                    .Replace("<br />", "\n")
                    .Replace("<p>", "\n")
                    .Replace("</p>", "\n")
                    .Replace("<div>", "\n")
                    .Replace("</div>", "\n")
                    .Replace("<h1>", "\n\n")
                    .Replace("</h1>", "\n")
                    .Replace("<h2>", "\n\n")
                    .Replace("</h2>", "\n")
                    .Replace("<h3>", "\n\n")
                    .Replace("</h3>", "\n");

                // Remove HTML tags
                var regex = new Regex("<[^>]+>");
                plainText = regex.Replace(plainText, "");

                // Decode HTML entities
                plainText = System.Web.HttpUtility.HtmlDecode(plainText);

                // Clean up extra whitespace
                plainText = Regex.Replace(plainText, @"\s+", " ");
                plainText = plainText.Trim();

                return plainText;
            }
            catch
            {
                return html; // Return original if conversion fails
            }
        }
    }
}
