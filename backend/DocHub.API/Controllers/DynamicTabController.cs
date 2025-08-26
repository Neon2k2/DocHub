using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DocHub.Application.Interfaces;
using DocHub.Application.DTOs;
using DocHub.Core.Entities;
using Microsoft.Extensions.Logging;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DynamicTabController : ControllerBase
{
    private readonly IDynamicTabService _dynamicTabService;
    private readonly ILogger<DynamicTabController> _logger;

    public DynamicTabController(IDynamicTabService dynamicTabService, ILogger<DynamicTabController> logger)
    {
        _dynamicTabService = dynamicTabService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DynamicTabDto>>> GetAllTabs()
    {
        try
        {
            var tabs = await _dynamicTabService.GetAllActiveTabsAsync();
            return Ok(tabs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tabs");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DynamicTabDto>> GetTabById(string id)
    {
        try
        {
            var tab = await _dynamicTabService.GetTabByIdAsync(id);
            if (tab == null)
                return NotFound();

            return Ok(tab);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tab by id: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("name/{name}")]
    public async Task<ActionResult<DynamicTabDto>> GetTabByName(string name)
    {
        try
        {
            var tab = await _dynamicTabService.GetTabByNameAsync(name);
            if (tab == null)
                return NotFound();

            return Ok(tab);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tab by name: {Name}", name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<DynamicTabDto>> CreateTab([FromBody] CreateDynamicTabDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTab = await _dynamicTabService.CreateTabAsync(createDto);
            return CreatedAtAction(nameof(GetTabById), new { id = createdTab.Id }, createdTab);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tab");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DynamicTabDto>> UpdateTab(string id, [FromBody] UpdateDynamicTabDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedTab = await _dynamicTabService.UpdateTabAsync(id, updateDto);
            return Ok(updatedTab);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tab: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTab(string id)
    {
        try
        {
            var deleted = await _dynamicTabService.DeleteTabAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tab: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<bool>> ToggleTabStatus(string id)
    {
        try
        {
            var newStatus = await _dynamicTabService.ToggleTabStatusAsync(id);
            return Ok(newStatus);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling tab status: {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("datasource/{dataSource}")]
    public async Task<ActionResult<IEnumerable<DynamicTabDto>>> GetTabsByDataSource(string dataSource)
    {
        try
        {
            var tabs = await _dynamicTabService.GetTabsByDataSourceAsync(dataSource);
            return Ok(tabs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tabs by data source: {DataSource}", dataSource);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("reorder")]
    public async Task<ActionResult<bool>> ReorderTabs([FromBody] List<TabReorderDto> reorderDtos)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reordered = await _dynamicTabService.ReorderTabsAsync(reorderDtos);
            return Ok(reordered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering tabs");
            return StatusCode(500, "Internal server error");
        }
    }
}
