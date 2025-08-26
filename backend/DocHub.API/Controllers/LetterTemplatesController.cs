using Microsoft.AspNetCore.Mvc;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Application.DTOs;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LetterTemplatesController : ControllerBase
{
    private readonly ILetterTemplateService _templateService;
    private readonly ILogger<LetterTemplatesController> _logger;

    public LetterTemplatesController(
        ILetterTemplateService templateService,
        ILogger<LetterTemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LetterTemplate>>> GetAll()
    {
        try
        {
            var templates = await _templateService.GetAllAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all templates");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<LetterTemplate>>> GetActive()
    {
        try
        {
            var templates = await _templateService.GetActiveTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active templates");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("datasource/{dataSource}")]
    public async Task<ActionResult<IEnumerable<LetterTemplate>>> GetByDataSource(string dataSource)
    {
        try
        {
            var templates = await _templateService.GetByDataSourceAsync(dataSource);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates by data source: {DataSource}", dataSource);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LetterTemplate>> GetById(string id)
    {
        try
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template by id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("name/{name}")]
    public async Task<ActionResult<LetterTemplate>> GetByName(string name)
    {
        try
        {
            var template = await _templateService.GetByNameAsync(name);
            if (template == null)
            {
                return NotFound();
            }
            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template by name: {Name}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<LetterTemplate>> Create([FromBody] LetterTemplate template)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTemplate = await _templateService.CreateAsync(template);
            return CreatedAtAction(nameof(GetById), new { id = createdTemplate.Id }, createdTemplate);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LetterTemplate>> Update(string id, [FromBody] LetterTemplate template)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedTemplate = await _templateService.UpdateAsync(id, template);
            return Ok(updatedTemplate);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var result = await _templateService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStatus(string id)
    {
        try
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            template.IsActive = !template.IsActive;
            var updatedTemplate = await _templateService.UpdateAsync(id, template);
            return Ok(updatedTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling template status: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/reorder")]
    public async Task<ActionResult> Reorder(string id, [FromBody] int newSortOrder)
    {
        try
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            template.SortOrder = newSortOrder;
            var updatedTemplate = await _templateService.UpdateAsync(id, template);
            return Ok(updatedTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering template: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}/datasource")]
    public async Task<ActionResult<LetterTemplate>> ToggleDataSource(string id, [FromBody] DataSourceToggleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Validate data source value
            if (request.DataSource != "Upload" && request.DataSource != "Database")
            {
                return BadRequest("Data source must be either 'Upload' or 'Database'");
            }

            template.DataSource = request.DataSource;
            var updatedTemplate = await _templateService.UpdateAsync(id, template);
            return Ok(updatedTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling data source for template: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class DataSourceToggleRequest
{
    public string DataSource { get; set; } = string.Empty; // "Upload" or "Database"
}
