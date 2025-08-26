using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BulkOperationsController : ControllerBase
{
    private readonly IGeneratedLetterService _generatedLetterService;
    private readonly ILetterPreviewService _letterPreviewService;
    private readonly IEmailService _emailService;
    private readonly ILogger<BulkOperationsController> _logger;

    public BulkOperationsController(
        IGeneratedLetterService generatedLetterService,
        ILetterPreviewService letterPreviewService,
        IEmailService emailService,
        ILogger<BulkOperationsController> logger)
    {
        _generatedLetterService = generatedLetterService;
        _letterPreviewService = letterPreviewService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Generate letters for multiple employees
    /// </summary>
    [HttpPost("generate-letters")]
    public async Task<ActionResult<ApiResponse<BulkLetterGenerationResult>>> GenerateBulkLetters([FromBody] BulkLetterGenerationRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _generatedLetterService.GenerateBulkLettersAsync(request);
            return Ok(ApiResponse<BulkLetterGenerationResult>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk letters");
            return StatusCode(500, ApiResponse<BulkLetterGenerationResult>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Generate letter previews for multiple employees
    /// </summary>
    [HttpPost("generate-previews")]
    public async Task<ActionResult<ApiResponse<BulkPreviewGenerationResult>>> GenerateBulkPreviews([FromBody] BulkPreviewGenerationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _letterPreviewService.GenerateBulkPreviewsAsync(request);
            return Ok(ApiResponse<BulkPreviewGenerationResult>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk previews");
            return StatusCode(500, ApiResponse<BulkPreviewGenerationResult>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Send emails for multiple generated letters
    /// </summary>
    [HttpPost("send-emails")]
    public async Task<ActionResult<ApiResponse<BulkEmailSendingResult>>> SendBulkEmails([FromBody] BulkEmailSendingRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _generatedLetterService.SendBulkEmailsAsync(request);
            return Ok(ApiResponse<BulkEmailSendingResult>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk emails");
            return StatusCode(500, ApiResponse<BulkEmailSendingResult>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get bulk operation status
    /// </summary>
    [HttpGet("status/{operationId}")]
    public async Task<ActionResult<BulkOperationStatusDto>> GetBulkOperationStatus(string operationId)
    {
        try
        {
            var status = await _generatedLetterService.GetBulkOperationStatusAsync(operationId);
            if (status == null)
                return NotFound();

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operation status {OperationId}", operationId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cancel a bulk operation
    /// </summary>
    [HttpPost("cancel/{operationId}")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelBulkOperation(string operationId)
    {
        try
        {
            var result = await _generatedLetterService.CancelBulkOperationAsync(operationId);
            return Ok(ApiResponse<bool>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling bulk operation {OperationId}", operationId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Retry failed items in a bulk operation
    /// </summary>
    [HttpPost("retry/{operationId}")]
    public async Task<ActionResult<ApiResponse<BulkOperationRetryResult>>> RetryBulkOperation(string operationId, [FromBody] RetryBulkOperationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _generatedLetterService.RetryBulkOperationAsync(operationId, request);
            return Ok(ApiResponse<BulkOperationRetryResult>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying bulk operation {OperationId}", operationId);
            return StatusCode(500, ApiResponse<BulkOperationRetryResult>.ErrorResult("Internal server error"));
        }
    }

    /// <summary>
    /// Get bulk operation history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<BulkOperationHistoryDto>>> GetBulkOperationHistory([FromQuery] BulkOperationHistoryFilter filter)
    {
        try
        {
            var history = await _generatedLetterService.GetBulkOperationHistoryAsync(filter);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operation history");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get bulk operation statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<BulkOperationStatsDto>> GetBulkOperationStats()
    {
        try
        {
            var stats = await _generatedLetterService.GetBulkOperationStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk operation stats");
            return StatusCode(500, "Internal server error");
        }
    }
}
