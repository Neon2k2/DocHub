using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LetterPreviewController : ControllerBase
{
    private readonly ILetterPreviewService _letterPreviewService;
    private readonly ILogger<LetterPreviewController> _logger;

    public LetterPreviewController(ILetterPreviewService letterPreviewService, ILogger<LetterPreviewController> logger)
    {
        _letterPreviewService = letterPreviewService;
        _logger = logger;
    }

    /// <summary>
    /// Generate letter preview for an employee
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<LetterPreviewDto>> GeneratePreview([FromBody] GeneratePreviewRequest request)
    {
        try
        {
            var preview = await _letterPreviewService.GeneratePreviewAsync(
                request.LetterTemplateId, 
                request.EmployeeId, 
                request.DigitalSignatureId);
            
            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview for template {TemplateId} and employee {EmployeeId}", 
                request.LetterTemplateId, request.EmployeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get letter preview for an employee
    /// </summary>
    [HttpGet("template/{templateId}/employee/{employeeId}")]
    public async Task<ActionResult<LetterPreviewDto>> GetPreview(string templateId, string employeeId)
    {
        try
        {
            var preview = await _letterPreviewService.GetPreviewAsync(templateId, employeeId);
            if (preview == null)
                return NotFound();

            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preview for template {TemplateId} and employee {EmployeeId}", 
                templateId, employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get latest preview with latest signature
    /// </summary>
    [HttpGet("template/{templateId}/employee/{employeeId}/latest")]
    public async Task<ActionResult<LetterPreviewDto>> GetLatestPreview(string templateId, string employeeId)
    {
        try
        {
            var preview = await _letterPreviewService.GetLatestPreviewAsync(templateId, employeeId);
            if (preview == null)
                return NotFound();

            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest preview for template {TemplateId} and employee {EmployeeId}", 
                templateId, employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update letter preview
    /// </summary>
    [HttpPut("{previewId}")]
    public async Task<ActionResult<LetterPreviewDto>> UpdatePreview(string previewId, [FromBody] UpdatePreviewRequest request)
    {
        try
        {
            var preview = await _letterPreviewService.UpdatePreviewAsync(previewId, request);
            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preview {PreviewId}", previewId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete letter preview
    /// </summary>
    [HttpDelete("{previewId}")]
    public async Task<ActionResult<bool>> DeletePreview(string previewId)
    {
        try
        {
            var result = await _letterPreviewService.DeletePreviewAsync(previewId);
            if (result)
            {
                return Ok(new { success = true, message = "Preview deleted successfully" });
            }
            
            return BadRequest("Failed to delete preview");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting preview {PreviewId}", previewId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all previews for an employee
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<LetterPreviewDto>>> GetPreviewsByEmployee(string employeeId)
    {
        try
        {
            var previews = await _letterPreviewService.GetPreviewsByEmployeeAsync(employeeId);
            return Ok(previews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previews for employee {EmployeeId}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all previews for a letter type
    /// </summary>
    [HttpGet("type/{letterType}")]
    public async Task<ActionResult<IEnumerable<LetterPreviewDto>>> GetPreviewsByLetterType(string letterType)
    {
        try
        {
            var previews = await _letterPreviewService.GetPreviewsByLetterTypeAsync(letterType);
            return Ok(previews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previews for letter type {LetterType}", letterType);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Regenerate preview with latest signature
    /// </summary>
    [HttpPost("{previewId}/regenerate")]
    public async Task<ActionResult<bool>> RegeneratePreviewWithLatestSignature(string previewId)
    {
        try
        {
            var result = await _letterPreviewService.RegeneratePreviewWithLatestSignatureAsync(previewId);
            if (result)
            {
                return Ok(new { success = true, message = "Preview regenerated with latest signature" });
            }
            
            return BadRequest("Failed to regenerate preview");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating preview {PreviewId} with latest signature", previewId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get previews by date range
    /// </summary>
    [HttpGet("daterange")]
    public async Task<ActionResult<IEnumerable<LetterPreviewDto>>> GetPreviewsByDateRange(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var previews = await _letterPreviewService.GetPreviewsByDateRangeAsync(startDate, endDate);
            return Ok(previews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previews by date range: {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Clone preview for another employee
    /// </summary>
    [HttpPost("{previewId}/clone")]
    public async Task<ActionResult<LetterPreviewDto>> ClonePreview(string previewId, [FromBody] ClonePreviewRequest request)
    {
        try
        {
            var preview = await _letterPreviewService.ClonePreviewAsync(previewId, request.NewEmployeeId);
            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning preview {PreviewId} for employee {NewEmployeeId}", 
                previewId, request.NewEmployeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Bulk generate previews
    /// </summary>
    [HttpPost("bulk-generate")]
    public async Task<ActionResult<BulkPreviewResult>> BulkGeneratePreviews([FromBody] BulkPreviewRequest request)
    {
        try
        {
            var result = await _letterPreviewService.BulkGeneratePreviewsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk generating previews for template {TemplateId}", request.LetterTemplateId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get preview with attachments
    /// </summary>
    [HttpGet("{previewId}/with-attachments")]
    public async Task<ActionResult<LetterPreviewDto>> GetPreviewWithAttachments(string previewId)
    {
        try
        {
            var preview = await _letterPreviewService.GetPreviewWithAttachmentsAsync(previewId);
            if (preview == null)
                return NotFound();

            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preview with attachments {PreviewId}", previewId);
            return StatusCode(500, "Internal server error");
        }
    }
}


