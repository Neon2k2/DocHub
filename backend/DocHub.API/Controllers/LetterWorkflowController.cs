using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LetterWorkflowController : ControllerBase
{
    private readonly ILetterWorkflowService _workflowService;
    private readonly ILetterStatusService _statusService;
    private readonly IGeneratedLetterService _letterService;
    private readonly ILogger<LetterWorkflowController> _logger;

    public LetterWorkflowController(
        ILetterWorkflowService workflowService,
        ILetterStatusService statusService,
        IGeneratedLetterService letterService,
        ILogger<LetterWorkflowController> logger)
    {
        _workflowService = workflowService;
        _statusService = statusService;
        _letterService = letterService;
        _logger = logger;
    }

    /// <summary>
    /// Process a single letter workflow
    /// </summary>
    [HttpPost("process")]
    public async Task<ActionResult<ApiResponse<LetterWorkflowResult>>> ProcessLetterWorkflow(
        [FromBody] LetterWorkflowRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.LetterId))
            {
                return BadRequest(ApiResponse<LetterWorkflowResult>.ValidationErrorResult(
                    "Letter ID is required", new List<string> { "Letter ID is required" }));
            }

            var result = await _workflowService.ProcessLetterWorkflowAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Letter workflow completed successfully: {LetterId}", request.LetterId);
                return Ok(ApiResponse<LetterWorkflowResult>.SuccessResult(result, "Letter workflow completed successfully"));
            }
            else
            {
                _logger.LogWarning("Letter workflow failed: {LetterId} - {Error}", request.LetterId, result.ErrorMessage);
                return BadRequest(ApiResponse<LetterWorkflowResult>.ErrorResult(
                    "Letter workflow failed", new List<string> { result.ErrorMessage ?? "Unknown error" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing letter workflow for {LetterId}", request?.LetterId);
            return StatusCode(500, ApiResponse<LetterWorkflowResult>.ErrorResult(
                "Error processing letter workflow", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Process bulk letter workflows
    /// </summary>
    [HttpPost("process/bulk")]
    public async Task<ActionResult<ApiResponse<BulkWorkflowResult>>> ProcessBulkWorkflow(
        [FromBody] BulkWorkflowRequest request)
    {
        try
        {
            if (request.LetterIds == null || !request.LetterIds.Any())
            {
                return BadRequest(ApiResponse<BulkWorkflowResult>.ValidationErrorResult(
                    "Letter IDs are required", new List<string> { "At least one letter ID is required" }));
            }

            var result = await _workflowService.ProcessBulkWorkflowAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Bulk workflow completed: {Successful}/{Total} successful", 
                    result.SuccessfulCount, request.LetterIds.Count);
                return Ok(ApiResponse<BulkWorkflowResult>.SuccessResult(result, 
                    $"Bulk workflow completed: {result.SuccessfulCount}/{request.LetterIds.Count} successful"));
            }
            else
            {
                _logger.LogWarning("Bulk workflow failed: {Error}", result.ErrorMessage);
                return BadRequest(ApiResponse<BulkWorkflowResult>.ErrorResult(
                    "Bulk workflow failed", new List<string> { result.ErrorMessage ?? "Unknown error" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk workflow");
            return StatusCode(500, ApiResponse<BulkWorkflowResult>.ErrorResult(
                "Error processing bulk workflow", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get workflow status for a letter
    /// </summary>
    [HttpGet("status/{letterId}")]
    public async Task<ActionResult<ApiResponse<LetterWorkflowStatus>>> GetWorkflowStatus(string letterId)
    {
        try
        {
            var status = await _workflowService.GetWorkflowStatusAsync(letterId);

            if (status.Status == "NotFound")
            {
                return NotFound(ApiResponse<LetterWorkflowStatus>.ErrorResult(
                    "Letter not found", new List<string> { "Letter ID not found" }));
            }

            _logger.LogInformation("Workflow status retrieved for letter: {LetterId}", letterId);
            return Ok(ApiResponse<LetterWorkflowStatus>.SuccessResult(status, "Workflow status retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow status for {LetterId}", letterId);
            return StatusCode(500, ApiResponse<LetterWorkflowStatus>.ErrorResult(
                "Error getting workflow status", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update letter status
    /// </summary>
    [HttpPut("status/{letterId}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateLetterStatus(
        string letterId,
        [FromBody] UpdateLetterStatusRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.NewStatus))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult(
                    "New status is required", new List<string> { "New status is required" }));
            }

            var success = await _statusService.UpdateLetterStatusAsync(letterId, request.NewStatus, request.Notes);

            if (success)
            {
                _logger.LogInformation("Letter status updated: {LetterId} -> {Status}", letterId, request.NewStatus);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Letter status updated successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to update letter status: {LetterId}", letterId);
                return BadRequest(ApiResponse<bool>.ErrorResult(
                    "Failed to update letter status", new List<string> { "Letter not found or update failed" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating letter status for {LetterId}", letterId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Error updating letter status", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update letter statuses in batch
    /// </summary>
    [HttpPut("status/batch")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateLetterStatusBatch(
        [FromBody] BatchUpdateLetterStatusRequest request)
    {
        try
        {
            if (request.LetterIds == null || !request.LetterIds.Any())
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult(
                    "Letter IDs are required", new List<string> { "At least one letter ID is required" }));
            }

            if (string.IsNullOrEmpty(request.NewStatus))
            {
                return BadRequest(ApiResponse<bool>.ValidationErrorResult(
                    "New status is required", new List<string> { "New status is required" }));
            }

            var success = await _statusService.UpdateLetterStatusBatchAsync(
                request.LetterIds, request.NewStatus, request.Notes);

            if (success)
            {
                _logger.LogInformation("Batch status update completed: {Count} letters updated to {Status}", 
                    request.LetterIds.Count, request.NewStatus);
                return Ok(ApiResponse<bool>.SuccessResult(true, 
                    $"Batch status update completed: {request.LetterIds.Count} letters updated"));
            }
            else
            {
                _logger.LogWarning("Failed to update letter statuses in batch");
                return BadRequest(ApiResponse<bool>.ErrorResult(
                    "Failed to update letter statuses", new List<string> { "Batch update failed" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating letter statuses in batch");
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Error updating letter statuses", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get letter status history
    /// </summary>
    [HttpGet("status/{letterId}/history")]
    public async Task<ActionResult<ApiResponse<List<LetterStatusHistory>>>> GetLetterStatusHistory(string letterId)
    {
        try
        {
            var history = await _statusService.GetLetterStatusHistoryAsync(letterId);

            _logger.LogInformation("Status history retrieved for letter: {LetterId}", letterId);
            return Ok(ApiResponse<List<LetterStatusHistory>>.SuccessResult(history, "Status history retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status history for {LetterId}", letterId);
            return StatusCode(500, ApiResponse<List<LetterStatusHistory>>.ErrorResult(
                "Error getting status history", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get letter status summary
    /// </summary>
    [HttpGet("status/summary")]
    public async Task<ActionResult<ApiResponse<LetterStatusSummary>>> GetLetterStatusSummary()
    {
        try
        {
            var summary = await _statusService.GetLetterStatusSummaryAsync();

            _logger.LogInformation("Letter status summary retrieved");
            return Ok(ApiResponse<LetterStatusSummary>.SuccessResult(summary, "Status summary retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letter status summary");
            return StatusCode(500, ApiResponse<LetterStatusSummary>.ErrorResult(
                "Error getting status summary", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get letters by status
    /// </summary>
    [HttpGet("status/{status}/letters")]
    public async Task<ActionResult<ApiResponse<List<GeneratedLetter>>>> GetLettersByStatus(
        string status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var letters = await _statusService.GetLettersByStatusAsync(status, page, pageSize);
            var totalCount = await _statusService.GetLetterCountByStatusAsync(status);

            var response = new
            {
                Letters = letters,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            _logger.LogInformation("Letters retrieved by status: {Status}, page {Page}", status, page);
            return Ok(ApiResponse<object>.SuccessResult(response, "Letters retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by status: {Status}", status);
            return StatusCode(500, ApiResponse<List<GeneratedLetter>>.ErrorResult(
                "Error getting letters by status", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Mark letter as sent
    /// </summary>
    [HttpPut("status/{letterId}/sent")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkLetterAsSent(
        string letterId,
        [FromBody] MarkLetterAsSentRequest request)
    {
        try
        {
            var success = await _statusService.MarkLetterAsSentAsync(letterId, request.EmailId);

            if (success)
            {
                _logger.LogInformation("Letter marked as sent: {LetterId}", letterId);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Letter marked as sent successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to mark letter as sent: {LetterId}", letterId);
                return BadRequest(ApiResponse<bool>.ErrorResult(
                    "Failed to mark letter as sent", new List<string> { "Letter not found or update failed" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking letter as sent: {LetterId}", letterId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Error marking letter as sent", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Mark letter as delivered
    /// </summary>
    [HttpPut("status/{letterId}/delivered")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkLetterAsDelivered(string letterId)
    {
        try
        {
            var success = await _statusService.MarkLetterAsDeliveredAsync(letterId);

            if (success)
            {
                _logger.LogInformation("Letter marked as delivered: {LetterId}", letterId);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Letter marked as delivered successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to mark letter as delivered: {LetterId}", letterId);
                return BadRequest(ApiResponse<bool>.ErrorResult(
                    "Failed to mark letter as delivered", new List<string> { "Letter not found or update failed" }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking letter as delivered: {LetterId}", letterId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Error marking letter as delivered", new List<string> { ex.Message }));
        }
    }
}

// Request models
public class UpdateLetterStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class BatchUpdateLetterStatusRequest
{
    public List<string> LetterIds { get; set; } = new();
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class MarkLetterAsSentRequest
{
    public string EmailId { get; set; } = string.Empty;
}
