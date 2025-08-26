using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailHistoryController : ControllerBase
{
    private readonly IEmailHistoryService _emailHistoryService;
    private readonly ILogger<EmailHistoryController> _logger;

    public EmailHistoryController(IEmailHistoryService emailHistoryService, ILogger<EmailHistoryController> logger)
    {
        _emailHistoryService = emailHistoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get email history by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmailHistoryDto>> GetEmailHistory(string id)
    {
        try
        {
            var emailHistory = await _emailHistoryService.GetEmailHistoryByIdAsync(id);
            if (emailHistory == null)
                return NotFound();

            return Ok(emailHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get email history by employee
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<EmailHistoryDto>>> GetEmailHistoryByEmployee(string employeeId)
    {
        try
        {
            var emailHistory = await _emailHistoryService.GetEmailHistoryByEmployeeAsync(employeeId);
            return Ok(emailHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history for employee: {EmployeeId}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get email history by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<EmailHistoryDto>>> GetEmailHistoryByStatus(string status)
    {
        try
        {
            var emailHistory = await _emailHistoryService.GetEmailHistoryByStatusAsync(status);
            return Ok(emailHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history by status: {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get email history by date range
    /// </summary>
    [HttpGet("daterange")]
    public async Task<ActionResult<IEnumerable<EmailHistoryDto>>> GetEmailHistoryByDateRange(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var emailHistory = await _emailHistoryService.GetEmailHistoryByDateRangeAsync(startDate, endDate);
            return Ok(emailHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history by date range: {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get failed emails
    /// </summary>
    [HttpGet("failed")]
    public async Task<ActionResult<IEnumerable<EmailHistoryDto>>> GetFailedEmails()
    {
        try
        {
            var failedEmails = await _emailHistoryService.GetFailedEmailsAsync();
            return Ok(failedEmails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed emails");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search email history
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<EmailHistoryDto>>> SearchEmailHistory([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            var results = await _emailHistoryService.SearchEmailHistoryAsync(searchTerm);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching email history with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Resend email
    /// </summary>
    [HttpPost("{id}/resend")]
    public async Task<ActionResult<bool>> ResendEmail(string id, [FromBody] ResendEmailRequest request)
    {
        try
        {
            var result = await _emailHistoryService.ResendEmailAsync(id, request);
            if (result)
            {
                _logger.LogInformation("Email resent successfully for ID: {Id}", id);
                return Ok(new { success = true, message = "Email resent successfully" });
            }
            
            return BadRequest("Failed to resend email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retry failed email
    /// </summary>
    [HttpPost("{id}/retry")]
    public async Task<ActionResult<bool>> RetryFailedEmail(string id)
    {
        try
        {
            var result = await _emailHistoryService.RetryFailedEmailAsync(id);
            if (result)
            {
                _logger.LogInformation("Failed email retried successfully for ID: {Id}", id);
                return Ok(new { success = true, message = "Email retry initiated" });
            }
            
            return BadRequest("Failed to retry email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed email with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Add attachment to email history
    /// </summary>
    [HttpPost("{id}/attachments")]
    public async Task<ActionResult<EmailHistoryDto>> AddAttachment(string id, [FromBody] AddEmailAttachmentRequest request)
    {
        try
        {
            var emailHistory = await _emailHistoryService.AddAttachmentAsync(id, request);
            return Ok(emailHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding attachment to email history with ID: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Remove attachment from email history
    /// </summary>
    [HttpDelete("{id}/attachments/{attachmentId}")]
    public async Task<ActionResult<bool>> RemoveAttachment(string id, string attachmentId)
    {
        try
        {
            var result = await _emailHistoryService.RemoveAttachmentAsync(id, attachmentId);
            if (result)
            {
                return Ok(new { success = true, message = "Attachment removed successfully" });
            }
            
            return BadRequest("Failed to remove attachment");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing attachment {AttachmentId} from email history {Id}", attachmentId, id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get email history by email ID (from email provider)
    /// </summary>
    [HttpGet("provider/{emailId}")]
    public async Task<ActionResult<EmailHistoryDto>> GetEmailHistoryByEmailId(string emailId)
    {
        try
        {
            var emailHistory = await _emailHistoryService.GetEmailHistoryByEmailIdAsync(emailId);
            if (emailHistory == null)
                return NotFound();

            return Ok(emailHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email history by email ID: {EmailId}", emailId);
            return StatusCode(500, "Internal server error");
        }
    }
}
