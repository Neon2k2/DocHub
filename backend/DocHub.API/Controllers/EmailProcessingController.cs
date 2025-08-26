using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailProcessingController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailHistoryService _emailHistoryService;
    private readonly ILogger<EmailProcessingController> _logger;

    public EmailProcessingController(
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        IEmailHistoryService emailHistoryService,
        ILogger<EmailProcessingController> logger)
    {
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _emailHistoryService = emailHistoryService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] CreateEmailHistoryRequest request)
    {
        try
        {
            // Input validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<EmailHistoryDto>.ValidationErrorResult("Invalid request data", errors));
            }

            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                return BadRequest(ApiResponse<EmailHistoryDto>.ErrorResult("Recipient email is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                return BadRequest(ApiResponse<EmailHistoryDto>.ErrorResult("Email subject is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return BadRequest(ApiResponse<EmailHistoryDto>.ErrorResult("Email body is required"));
            }

            var result = await _emailService.SendEmailAsync(
                request.ToEmail, 
                request.Subject, 
                request.Body, 
                request.Attachments?.Select(a => a.FilePath).ToList());

            if (result)
            {
                // Create email history record
                var emailHistoryRequest = new CreateEmailHistoryRequest
                {
                    Subject = request.Subject,
                    ToEmail = request.ToEmail,
                    CcEmail = request.CcEmail,
                    BccEmail = request.BccEmail,
                    Body = request.Body,
                    Status = "sent",
                    SentAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "system"
                };

                // Persist email history to database
                var savedEmailHistory = await _emailHistoryService.CreateEmailHistoryAsync(emailHistoryRequest);

                return Ok(ApiResponse<EmailHistoryDto>.SuccessResult(savedEmailHistory, "Email sent successfully"));
            }

            return BadRequest(ApiResponse<EmailHistoryDto>.ErrorResult("Failed to send email"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return StatusCode(500, ApiResponse<EmailHistoryDto>.ErrorResult("Internal server error"));
        }
    }

    [HttpPost("send-bulk")]
    public async Task<IActionResult> SendBulkEmails([FromBody] SendBulkEmailsRequest request)
    {
        try
        {
            var result = await _emailService.SendBulkEmailsAsync(
                request.ToEmails, 
                request.Subject, 
                request.Body, 
                request.AttachmentPaths);

            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "Bulk emails sent successfully"));
            }

            return BadRequest(ApiResponse<bool>.ErrorResult("Failed to send bulk emails"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk emails");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    [HttpPost("send-templated")]
    public async Task<IActionResult> SendTemplatedEmail([FromBody] SendTemplatedEmailRequest request)
    {
        try
        {
            var result = await _emailService.SendTemplatedEmailAsync(
                request.ToEmail, 
                request.TemplateId, 
                request.TemplateData, 
                request.AttachmentPaths);

            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "Templated email sent successfully"));
            }

            return BadRequest(ApiResponse<bool>.ErrorResult("Failed to send templated email"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending templated email");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleEmail([FromBody] CreateScheduledEmailRequest request)
    {
        try
        {
            var result = await _emailService.SendScheduledEmailAsync(
                request.ToEmail, 
                request.Subject, 
                request.Body, 
                request.ScheduledTime, 
                request.AttachmentPaths);

            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "Email scheduled successfully"));
            }

            return BadRequest(ApiResponse<bool>.ErrorResult("Failed to schedule email"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling email");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    [HttpDelete("schedule/{messageId}")]
    public async Task<IActionResult> CancelScheduledEmail(string messageId)
    {
        try
        {
            var result = await _emailService.CancelScheduledEmailAsync(messageId);

            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "Scheduled email cancelled successfully"));
            }

            return BadRequest(ApiResponse<bool>.ErrorResult("Failed to cancel scheduled email"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling scheduled email");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    [HttpGet("delivery-status/{messageId}")]
    public async Task<IActionResult> GetDeliveryStatus(string messageId)
    {
        try
        {
            var status = await _emailService.GetEmailDeliveryStatusAsync(messageId);
            return Ok(ApiResponse<EmailDeliveryStatus>.SuccessResult(status, "Delivery status retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery status");
            return StatusCode(500, ApiResponse<EmailDeliveryStatus>.ErrorResult("Internal server error"));
        }
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetEmailAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var analytics = await _emailService.GetEmailAnalyticsAsync(startDate, endDate);
            return Ok(ApiResponse<EmailAnalytics>.SuccessResult(analytics, "Email analytics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email analytics");
            return StatusCode(500, ApiResponse<EmailAnalytics>.ErrorResult("Internal server error"));
        }
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetEmailTemplates()
    {
        try
        {
            var templates = await _emailTemplateService.GetAvailableTemplatesAsync();
            return Ok(ApiResponse<IEnumerable<EmailTemplate>>.SuccessResult(templates, "Email templates retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email templates");
            return StatusCode(500, ApiResponse<IEnumerable<EmailTemplate>>.ErrorResult("Internal server error"));
        }
    }

    [HttpGet("templates/{templateName}")]
    public async Task<IActionResult> GetEmailTemplate(string templateName)
    {
        try
        {
            var template = await _emailTemplateService.GetTemplateAsync(templateName);
            if (template != null)
            {
                return Ok(ApiResponse<EmailTemplate>.SuccessResult(template, "Email template retrieved successfully"));
            }

            return NotFound(ApiResponse<EmailTemplate>.NotFoundResult("Template not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email template");
            return StatusCode(500, ApiResponse<EmailTemplate>.ErrorResult("Internal server error"));
        }
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CreateEmailTemplate([FromBody] CreateEmailTemplateRequest request)
    {
        try
        {
            var result = await _emailTemplateService.SaveTemplateAsync(
                request.Name, 
                request.Content, 
                request.Description);

            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "Email template created successfully"));
            }

            return BadRequest(ApiResponse<bool>.ErrorResult("Failed to create email template"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email template");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    [HttpPost("templates/{templateName}/preview")]
    public async Task<IActionResult> PreviewEmailTemplate(string templateName, [FromBody] Dictionary<string, string> placeholders)
    {
        try
        {
            var preview = await _emailTemplateService.PreviewTemplateAsync(templateName, placeholders);
            return Ok(ApiResponse<EmailPreview>.SuccessResult(preview, "Email template preview generated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing email template");
            return StatusCode(500, ApiResponse<EmailPreview>.ErrorResult("Internal server error"));
        }
    }

    [HttpPost("templates/{templateName}/validate")]
    public async Task<IActionResult> ValidateEmailTemplate(string templateName, [FromBody] ValidateTemplateRequest request)
    {
        try
        {
            var isValid = await _emailTemplateService.ValidateTemplateAsync(request.Content);
            return Ok(ApiResponse<bool>.SuccessResult(isValid, "Email template validation completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating email template");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetEmailHistory([FromQuery] string? recipientEmail, [FromQuery] string? status, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            IEnumerable<EmailHistoryDto> history;
            
            if (!string.IsNullOrEmpty(recipientEmail))
            {
                // Search by email
                history = await _emailHistoryService.SearchEmailHistoryAsync(recipientEmail);
            }
            else if (!string.IsNullOrEmpty(status))
            {
                history = await _emailHistoryService.GetEmailHistoryByStatusAsync(status);
            }
            else if (startDate.HasValue && endDate.HasValue)
            {
                history = await _emailHistoryService.GetEmailHistoryByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                // Get all history - return empty for now since there's no method for this
                history = new List<EmailHistoryDto>();
            }
            
            return Ok(ApiResponse<IEnumerable<EmailHistoryDto>>.SuccessResult(history, "Email history retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history");
            return StatusCode(500, ApiResponse<IEnumerable<EmailHistoryDto>>.ErrorResult("Internal server error"));
        }
    }

    [HttpGet("provider-info")]
    public IActionResult GetEmailProviderInfo()
    {
        try
        {
            var providerInfo = _emailService.GetEmailProviderInfoAsync().Result;
            return Ok(ApiResponse<EmailProviderInfo>.SuccessResult(providerInfo, "Email provider information retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email provider info");
            return StatusCode(500, ApiResponse<EmailProviderInfo>.ErrorResult("Internal server error"));
        }
    }

    [HttpPost("resend/{emailId}")]
    public async Task<IActionResult> ResendEmail(string emailId, [FromBody] ResendEmailRequest? request = null)
    {
        try
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(emailId))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Email ID is required"));
            }

            var result = await _emailService.ResendEmailAsync(emailId);
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "Email resent successfully"));
            }

            return BadRequest(ApiResponse<bool>.ErrorResult("Failed to resend email"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }
}

// Request DTOs
public class SendBulkEmailsRequest
{
    public List<string> ToEmails { get; set; } = new List<string>();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string>? AttachmentPaths { get; set; }
}

public class SendTemplatedEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, string> TemplateData { get; set; } = new Dictionary<string, string>();
    public List<string>? AttachmentPaths { get; set; }
}

public class CreateEmailTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ValidateTemplateRequest
{
    public string Content { get; set; } = string.Empty;
}
