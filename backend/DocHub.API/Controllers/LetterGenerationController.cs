using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;
using Microsoft.Extensions.Logging;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LetterGenerationController : ControllerBase
{
    private readonly IGeneratedLetterService _letterService;
    private readonly IEmailService _emailService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<LetterGenerationController> _logger;

    public LetterGenerationController(
        IGeneratedLetterService letterService,
        IEmailService emailService,
        IEmployeeService employeeService,
        ILogger<LetterGenerationController> logger)
    {
        _letterService = letterService;
        _emailService = emailService;
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a letter for a single employee
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<GeneratedLetter>>> GenerateLetter([FromBody] GenerateLetterRequest request)
    {
        try
        {
            _logger.LogInformation("Generating letter for employee {EmployeeId} using template {TemplateId}", 
                request.EmployeeId, request.LetterTemplateId);

            // Validate request
            if (string.IsNullOrEmpty(request.LetterTemplateId))
                return BadRequest(ApiResponse<GeneratedLetter>.ValidationErrorResult("Template ID is required", new List<string> { "Template ID is required" }));

            if (string.IsNullOrEmpty(request.EmployeeId))
                return BadRequest(ApiResponse<GeneratedLetter>.ValidationErrorResult("Employee ID is required", new List<string> { "Employee ID is required" }));

            // Generate the letter
            var generatedLetter = await _letterService.GenerateLetterAsync(request);

            _logger.LogInformation("Letter generated successfully: {LetterNumber}", generatedLetter.LetterNumber);

            return Ok(ApiResponse<GeneratedLetter>.SuccessResult(generatedLetter, "Letter generated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for letter generation");
            return BadRequest(ApiResponse<GeneratedLetter>.ValidationErrorResult("Invalid request", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating letter");
            return StatusCode(500, ApiResponse<GeneratedLetter>.ErrorResult("Error generating letter", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Generate letters for multiple employees
    /// </summary>
    [HttpPost("generate-bulk")]
    public async Task<ActionResult<ApiResponse<List<GeneratedLetter>>>> GenerateBulkLetters([FromBody] GenerateBulkLettersRequest request)
    {
        try
        {
            _logger.LogInformation("Generating bulk letters for {Count} employees", request.EmployeeIds.Count);

            if (string.IsNullOrEmpty(request.LetterTemplateId))
                return BadRequest(ApiResponse<List<GeneratedLetter>>.ValidationErrorResult("Template ID is required", new List<string> { "Template ID is required" }));

            if (request.EmployeeIds == null || !request.EmployeeIds.Any())
                return BadRequest(ApiResponse<List<GeneratedLetter>>.ValidationErrorResult("Employee IDs are required", new List<string> { "At least one employee ID is required" }));

            var generatedLetters = new List<GeneratedLetter>();

            foreach (var employeeId in request.EmployeeIds)
            {
                try
                {
                    var generateRequest = new GenerateLetterRequest
                    {
                        LetterTemplateId = request.LetterTemplateId,
                        EmployeeId = employeeId,
                        DigitalSignatureId = request.DigitalSignatureId,
                        FieldValues = request.FieldValues,
                        AttachmentPaths = request.AttachmentPaths ?? new List<string>()
                    };
                    
                    var letter = await _letterService.GenerateLetterAsync(generateRequest);
                    generatedLetters.Add(letter);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate letter for employee {EmployeeId}", employeeId);
                    // Continue with other employees
                }
            }

            _logger.LogInformation("Bulk letter generation completed: {SuccessCount}/{TotalCount}", 
                generatedLetters.Count, request.EmployeeIds.Count);

            return Ok(ApiResponse<List<GeneratedLetter>>.SuccessResult(generatedLetters, 
                $"Generated {generatedLetters.Count} out of {request.EmployeeIds.Count} letters"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk letter generation");
            return StatusCode(500, ApiResponse<List<GeneratedLetter>>.ErrorResult("Error in bulk letter generation", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Send email for a generated letter
    /// </summary>
    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<bool>>> SendLetter([FromBody] SendLetterRequest request)
    {
        try
        {
            _logger.LogInformation("Sending letter {LetterId} via email", request.LetterId);

            if (string.IsNullOrEmpty(request.LetterId))
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Letter ID is required", new List<string> { "Letter ID is required" }));

            if (string.IsNullOrEmpty(request.EmailHistoryId))
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Email History ID is required", new List<string> { "Email History ID is required" }));

            // Send the letter
            var emailSent = await _letterService.SendEmailAsync(
                request.LetterId,
                request.EmailHistoryId,
                request.AdditionalAttachments
            );

            if (emailSent)
            {
                _logger.LogInformation("Letter sent successfully: {LetterId}", request.LetterId);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Letter sent successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to send letter: {LetterId}", request.LetterId);
                return BadRequest(ApiResponse<bool>.ErrorResult("Failed to send letter", new List<string> { "Email service failed" }));
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for sending letter");
            return BadRequest(ApiResponse<bool>.ValidationErrorResult("Invalid request", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending letter");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error sending letter", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Send emails for multiple letters
    /// </summary>
    [HttpPost("send-bulk-emails")]
    public async Task<ActionResult<ApiResponse<string>>> SendBulkEmails([FromBody] BulkEmailRequest request)
    {
        try
        {
            if (request.LetterIds == null || !request.LetterIds.Any())
            {
                return BadRequest(ApiResponse<string>.ValidationErrorResult("Letter IDs are required", new List<string> { "At least one letter ID is required" }));
            }

            if (request.ToEmails == null || !request.ToEmails.Any())
            {
                return BadRequest(ApiResponse<string>.ValidationErrorResult("Recipient emails are required", new List<string> { "At least one recipient email is required" }));
            }

            var messageIds = await _letterService.SendBulkEmailsAsync(request);

            _logger.LogInformation("Bulk emails sent successfully for {Count} letters", request.LetterIds.Count);

            return Ok(ApiResponse<string>.SuccessResult(messageIds, "Bulk emails sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk emails");
            return StatusCode(500, ApiResponse<string>.ErrorResult("Error sending bulk emails", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Resend email for a letter
    /// </summary>
    [HttpPost("resend-email/{id}")]
    public async Task<ActionResult<ApiResponse<string>>> ResendEmail(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<string>.ValidationErrorResult("Letter ID is required", new List<string> { "Letter ID is required" }));
            }

            var success = await _letterService.ResendEmailAsync(id);

            if (success)
            {
                _logger.LogInformation("Email resent successfully for letter {LetterId}", id);
                return Ok(ApiResponse<string>.SuccessResult("Email resent successfully", "Email resent successfully"));
            }
            else
            {
                return BadRequest(ApiResponse<string>.ErrorResult("Failed to resend email", new List<string> { "Resend operation returned false" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email for letter {LetterId}", id);
            return StatusCode(500, ApiResponse<string>.ErrorResult("Error resending email", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get letter generation statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<LetterGenerationStats>>> GetGenerationStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var stats = await _letterService.GetGenerationStatsAsync(fromDate, toDate);

            return Ok(ApiResponse<LetterGenerationStats>.SuccessResult(stats, "Statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving letter generation statistics");
            return StatusCode(500, ApiResponse<LetterGenerationStats>.ErrorResult("Error retrieving statistics", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update email status for a letter
    /// </summary>
    [HttpPut("email-status/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateEmailStatus(
        string id,
        [FromBody] UpdateEmailStatusRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Letter ID is required", new List<string> { "Letter ID is required" }));
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Status is required", new List<string> { "Status is required" }));
            }

            var success = await _letterService.UpdateEmailStatusAsync(id, request.Status, null);

            if (success)
            {
                _logger.LogInformation("Email status updated successfully for letter {LetterId} to {Status}", id, request.Status);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Email status updated successfully"));
            }
            else
            {
                return NotFound(ApiResponse<bool>.ErrorResult("Letter not found", new List<string> { "Letter ID not found" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email status for letter {LetterId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error updating email status", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get all generated letters with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<GeneratedLetterDto>>>> GetGeneratedLetters(
        [FromQuery] string? status = null,
        [FromQuery] string? emailStatus = null,
        [FromQuery] string? employeeId = null,
        [FromQuery] string? templateId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            IEnumerable<GeneratedLetter> letters;

            if (!string.IsNullOrEmpty(status))
            {
                // Filter by status (this would need to be implemented in the service)
                letters = await _letterService.GetLettersByStatusAsync(status);
            }
            else if (!string.IsNullOrEmpty(emailStatus))
            {
                letters = await _letterService.GetLettersByStatusAsync(emailStatus);
            }
            else if (!string.IsNullOrEmpty(employeeId))
            {
                letters = await _letterService.GetLettersByEmployeeAsync(employeeId);
            }
            else if (!string.IsNullOrEmpty(templateId))
            {
                letters = await _letterService.GetLettersByTemplateAsync(templateId);
            }
            else
            {
                letters = await _letterService.GetAllGeneratedLettersAsync();
            }

            // Apply date filtering if provided
            if (fromDate.HasValue || toDate.HasValue)
            {
                letters = letters.Where(l => 
                    (!fromDate.HasValue || l.CreatedAt >= fromDate.Value) &&
                    (!toDate.HasValue || l.CreatedAt <= toDate.Value)
                );
            }

            // Convert entities to DTOs
            var letterDtos = letters.Select(l => new GeneratedLetterDto
            {
                Id = l.Id,
                LetterNumber = l.LetterNumber ?? "",
                LetterType = l.LetterType ?? "",
                LetterTemplateId = l.LetterTemplateId ?? "",
                EmployeeId = l.EmployeeId ?? "",
                DigitalSignatureId = l.DigitalSignatureId ?? "",
                LetterFilePath = l.LetterFilePath,
                Status = l.Status ?? "Generated",
                GeneratedAt = l.GeneratedAt,
                SentAt = l.SentAt,
                DeliveredAt = l.DeliveredAt,
                EmailId = l.EmailId,
                ErrorMessage = l.ErrorMessage,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy ?? "",
                UpdatedAt = l.UpdatedAt
            });

            return Ok(ApiResponse<IEnumerable<GeneratedLetterDto>>.SuccessResult(letterDtos, "Letters retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving generated letters");
            return StatusCode(500, ApiResponse<IEnumerable<GeneratedLetterDto>>.ErrorResult("Error retrieving letters", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get a specific generated letter by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GeneratedLetterDto>>> GetGeneratedLetter(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<GeneratedLetterDto>.ValidationErrorResult("Letter ID is required", new List<string> { "Letter ID is required" }));
            }

            var letter = await _letterService.GetGeneratedLetterByIdAsync(id);

            if (letter == null)
            {
                return NotFound(ApiResponse<GeneratedLetterDto>.ErrorResult("Letter not found", new List<string> { "Letter ID not found" }));
            }

            // Convert entity to DTO
            var letterDto = new GeneratedLetterDto
            {
                Id = letter.Id,
                LetterNumber = letter.LetterNumber ?? "",
                LetterType = letter.LetterType ?? "",
                LetterTemplateId = letter.LetterTemplateId ?? "",
                EmployeeId = letter.EmployeeId ?? "",
                DigitalSignatureId = letter.DigitalSignatureId ?? "",
                LetterFilePath = letter.LetterFilePath,
                Status = letter.Status ?? "Generated",
                GeneratedAt = letter.GeneratedAt,
                SentAt = letter.SentAt,
                DeliveredAt = letter.DeliveredAt,
                EmailId = letter.EmailId,
                ErrorMessage = letter.ErrorMessage,
                CreatedAt = letter.CreatedAt,
                CreatedBy = letter.CreatedBy ?? "",
                UpdatedAt = letter.UpdatedAt
            };

            return Ok(ApiResponse<GeneratedLetterDto>.SuccessResult(letterDto, "Letter retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving letter {LetterId}", id);
            return StatusCode(500, ApiResponse<GeneratedLetterDto>.ErrorResult("Error retrieving letter", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a generated letter
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteGeneratedLetter(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult("Letter ID is required", new List<string> { "Letter ID is required" }));
            }

            var success = await _letterService.DeleteGeneratedLetterAsync(id);

            if (success)
            {
                _logger.LogInformation("Letter deleted successfully: {LetterId}", id);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Letter deleted successfully"));
            }
            else
            {
                return NotFound(ApiResponse<bool>.ErrorResult("Letter not found", new List<string> { "Letter ID not found" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting letter {LetterId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Error deleting letter", new List<string> { ex.Message }));
        }
    }

    [HttpGet("preview/{templateId}")]
    public async Task<ActionResult<ApiResponse<LetterPreview>>> PreviewLetter(string templateId, [FromQuery] string employeeId, [FromQuery] string signatureId = null)
    {
        try
        {
            _logger.LogInformation("Generating letter preview for template {TemplateId}", templateId);

            if (string.IsNullOrEmpty(employeeId))
                return BadRequest(ApiResponse<LetterPreview>.ValidationErrorResult("Employee ID is required", new List<string> { "Employee ID is required" }));

            // For now, return a basic preview structure
            // TODO: Implement actual preview generation logic
            var preview = new LetterPreview
            {
                LetterType = "Preview",
                LetterTemplateId = templateId,
                EmployeeId = employeeId,
                DigitalSignatureId = signatureId,
                PreviewContent = "Letter preview content will be generated here",
                LastGeneratedAt = DateTime.UtcNow,
                IsActive = true
            };

            _logger.LogInformation("Letter preview generated successfully for template {TemplateId}", templateId);

            return Ok(ApiResponse<LetterPreview>.SuccessResult(preview, "Letter preview generated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating letter preview");
            return StatusCode(500, ApiResponse<LetterPreview>.ErrorResult("Error generating letter preview", new List<string> { ex.Message }));
        }
    }
}

// Request models
public class UpdateEmailStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public DateTime? StatusDate { get; set; }
}

