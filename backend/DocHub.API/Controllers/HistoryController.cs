using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly IGeneratedLetterService _letterService;
    private readonly IEmailService _emailService;
    private readonly ILogger<HistoryController> _logger;

    public HistoryController(
        IGeneratedLetterService letterService,
        IEmailService emailService,
        ILogger<HistoryController> logger)
    {
        _letterService = letterService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GeneratedLetter>>> GetLetterHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null,
        [FromQuery] string? templateId = null,
        [FromQuery] string? employeeId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            IEnumerable<GeneratedLetter> letters;

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                letters = await _letterService.GetByStatusAsync(status);
            }
            else if (!string.IsNullOrEmpty(templateId))
            {
                letters = await _letterService.GetByTemplateAsync(templateId);
            }
            else if (!string.IsNullOrEmpty(employeeId))
            {
                letters = await _letterService.GetByEmployeeAsync(employeeId);
            }
            else if (startDate.HasValue && endDate.HasValue)
            {
                letters = await _letterService.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                letters = await _letterService.GetPagedAsync(page, pageSize);
            }

            // Apply pagination if not already filtered
            if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(templateId) && 
                string.IsNullOrEmpty(employeeId) && !startDate.HasValue && !endDate.HasValue)
            {
                var totalCount = await _letterService.GetTotalCountAsync();
                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", page.ToString());
                Response.Headers.Add("X-PageSize", pageSize.ToString());
            }

            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letter history");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<GeneratedLetter>>> SearchHistory([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search term is required");
            }

            var letters = await _letterService.SearchAsync(q);
            return Ok(letters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching letter history with term: {SearchTerm}", q);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<HistoryStats>> GetHistoryStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var allLetters = await _letterService.GetAllAsync();
            
            // Apply date filter if provided
            if (startDate.HasValue && endDate.HasValue)
            {
                allLetters = allLetters.Where(l => l.CreatedAt >= startDate.Value && l.CreatedAt <= endDate.Value);
            }

            var stats = new HistoryStats
            {
                TotalLetters = allLetters.Count(),
                GeneratedCount = allLetters.Count(l => l.Status == "Generated"),
                SentCount = allLetters.Count(l => l.Status == "Sent"),
                FailedCount = allLetters.Count(l => l.Status == "Failed"),
                PendingCount = allLetters.Count(l => l.Status == "Pending"),
                DateRange = new DateRange
                {
                    StartDate = startDate ?? allLetters.Min(l => l.CreatedAt),
                    EndDate = endDate ?? allLetters.Max(l => l.CreatedAt)
                }
            };

            if (stats.TotalLetters > 0)
            {
                stats.SuccessRate = Math.Round((double)stats.SentCount / stats.TotalLetters * 100, 2);
                stats.AverageProcessingTime = allLetters
                    .Where(l => l.SentAt.HasValue)
                    .Select(l => (l.SentAt.Value - l.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average();
            }

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history stats");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("email-status")]
    public async Task<ActionResult<EmailStatusSummary>> GetEmailStatusSummary()
    {
        try
        {
            var allLetters = await _letterService.GetAllAsync();
            
            var totalEmails = allLetters.Count();
            var sentEmails = allLetters.Count(l => l.Status == "Sent");
            var failedEmails = allLetters.Count(l => l.Status == "Failed");
            var pendingEmails = allLetters.Count(l => l.Status == "Generated");
            var deliveredEmails = allLetters.Count(l => l.Status == "Delivered");
            var openedEmails = allLetters.Count(l => l.Status == "Opened");
            var bouncedEmails = allLetters.Count(l => l.Status == "Bounced");

            var statusSummary = new EmailStatusSummary
            {
                Status = "Summary",
                Count = totalEmails,
                Percentage = 100.0,
                SentEmails = sentEmails,
                FailedEmails = failedEmails,
                PendingEmails = pendingEmails,
                DeliveredEmails = deliveredEmails,
                OpenedEmails = openedEmails,
                BouncedEmails = bouncedEmails,
                DeliveryRate = totalEmails > 0 ? Math.Round((double)deliveredEmails / totalEmails * 100, 2) : 0,
                OpenRate = deliveredEmails > 0 ? Math.Round((double)openedEmails / deliveredEmails * 100, 2) : 0
            };

            return Ok(statusSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email status summary");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GeneratedLetter>> GetLetterById(string id)
    {
        try
        {
            var letter = await _letterService.GetByIdAsync(id);
            if (letter == null)
            {
                return NotFound();
            }
            return Ok(letter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letter by id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/resend")]
    public async Task<ActionResult<ResendResult>> ResendEmail(string id)
    {
        try
        {
            var letter = await _letterService.GetByIdAsync(id);
            if (letter == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(letter.EmailId))
            {
                return BadRequest("No email ID associated with this letter");
            }

            var result = await _letterService.ResendEmailAsync(id);
            
            var resendResult = new ResendResult
            {
                LetterId = id,
                Success = result,
                ResentAt = DateTime.UtcNow,
                Message = result ? "Email resent successfully" : "Failed to resend email"
            };

            if (result)
            {
                return Ok(resendResult);
            }
            else
            {
                return BadRequest(resendResult);
            }
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ResendResult
            {
                LetterId = id,
                Success = false,
                ResentAt = DateTime.UtcNow,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email for letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("bulk-resend")]
    public async Task<ActionResult<BulkResendResult>> BulkResendEmails([FromBody] BulkResendRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = new BulkResendResult
            {
                TotalLetters = request.LetterIds.Count,
                SuccessfullyResent = 0,
                FailedCount = 0,
                Results = new List<ResendResult>()
            };

            foreach (var letterId in request.LetterIds)
            {
                try
                {
                    var resendResult = await _letterService.ResendEmailAsync(letterId);
                    
                    var resultItem = new ResendResult
                    {
                        LetterId = letterId,
                        Success = resendResult,
                        ResentAt = DateTime.UtcNow,
                        Message = resendResult ? "Email resent successfully" : "Failed to resend email"
                    };

                    result.Results.Add(resultItem);

                    if (resendResult)
                    {
                        result.SuccessfullyResent++;
                    }
                    else
                    {
                        result.FailedCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Results.Add(new ResendResult
                    {
                        LetterId = letterId,
                        Success = false,
                        ResentAt = DateTime.UtcNow,
                        Message = $"Error: {ex.Message}"
                    });
                    result.FailedCount++;
                }
            }

            result.ProcessedAt = DateTime.UtcNow;

            if (result.SuccessfullyResent > 0)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk resend");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/update-status")]
    public async Task<ActionResult> UpdateLetterStatus(
        string id, 
        [FromBody] UpdateStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _letterService.UpdateStatusAsync(id, request.Status, request.ErrorMessage);
            
            if (result)
            {
                return Ok(new { message = "Status updated successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to update status" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLetter(string id)
    {
        try
        {
            var result = await _letterService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting letter: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

// Request and response models
public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class BulkResendRequest
{
    public List<string> LetterIds { get; set; } = new();
}

public class HistoryStats
{
    public int TotalLetters { get; set; }
    public int GeneratedCount { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageProcessingTime { get; set; }
    public DateRange DateRange { get; set; } = new();
}

public class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ResendResult
{
    public string LetterId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateTime ResentAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BulkResendResult
{
    public int TotalLetters { get; set; }
    public int SuccessfullyResent { get; set; }
    public int FailedCount { get; set; }
    public List<ResendResult> Results { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}
