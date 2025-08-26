using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class LetterTemplateFieldsController : ControllerBase
{
    private readonly ILetterTemplateService _letterTemplateService;
    private readonly ILogger<LetterTemplateFieldsController> _logger;

    public LetterTemplateFieldsController(
        ILetterTemplateService letterTemplateService,
        ILogger<LetterTemplateFieldsController> logger)
    {
        _letterTemplateService = letterTemplateService;
        _logger = logger;
    }

    /// <summary>
    /// Get all fields for a specific letter template
    /// </summary>
    [HttpGet("template/{templateId}")]
    public async Task<ActionResult<IEnumerable<LetterTemplateFieldDto>>> GetFieldsByTemplate(string templateId)
    {
        try
        {
            var fields = await _letterTemplateService.GetTemplateFieldsAsync(templateId);
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fields for template {TemplateId}", templateId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific field by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LetterTemplateFieldDto>> GetField(string id)
    {
        try
        {
            var field = await _letterTemplateService.GetTemplateFieldAsync(id);
            if (field == null)
                return NotFound();

            return Ok(field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new field for a letter template
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LetterTemplateFieldDto>> CreateField([FromBody] CreateLetterTemplateFieldDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdField = await _letterTemplateService.CreateTemplateFieldAsync(createDto);
            return CreatedAtAction(nameof(GetField), new { id = createdField.Id }, createdField);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template field");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing field
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<LetterTemplateFieldDto>> UpdateField(string id, [FromBody] UpdateLetterTemplateFieldDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedField = await _letterTemplateService.UpdateTemplateFieldAsync(id, updateDto);
            if (updatedField == null)
                return NotFound();

            return Ok(updatedField);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating field {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a field
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteField(string id)
    {
        try
        {
            var result = await _letterTemplateService.DeleteTemplateFieldAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting field {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Reorder fields for a template
    /// </summary>
    [HttpPost("reorder")]
    public async Task<ActionResult> ReorderFields([FromBody] List<FieldReorderDto> reorderDtos)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _letterTemplateService.ReorderTemplateFieldsAsync(reorderDtos);
            if (!result)
                return BadRequest("Failed to reorder fields");

            return Ok("Fields reordered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering template fields");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get field validation rules
    /// </summary>
    [HttpGet("validation-rules")]
    public ActionResult<IEnumerable<string>> GetValidationRules()
    {
        var rules = new[]
        {
            "required",
            "email",
            "phone",
            "date",
            "number",
            "min_length",
            "max_length",
            "pattern"
        };

        return Ok(rules);
    }
}
