using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AdminManagementController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AdminManagementController> _logger;

    public AdminManagementController(
        IAuthService authService,
        ILogger<AdminManagementController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Get all admin users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllAdmins()
    {
        try
        {
            var admins = await _authService.GetAllAdminsAsync();
            return Ok(admins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all admins");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get admin by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetAdminById(string id)
    {
        try
        {
            var admin = await _authService.GetAdminByIdAsync(id);
            if (admin == null)
                return NotFound();

            return Ok(admin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new admin user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdAdmin = await _authService.CreateAdminAsync(request);
            return CreatedAtAction(nameof(GetAdminById), new { id = createdAdmin.Id }, createdAdmin);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update admin user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateAdmin(string id, [FromBody] UpdateAdminRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedAdmin = await _authService.UpdateAdminAsync(id, request);
            if (updatedAdmin == null)
                return NotFound();

            return Ok(updatedAdmin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete admin user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAdmin(string id)
    {
        try
        {
            var result = await _authService.DeleteAdminAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting admin {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Toggle admin status
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<UserDto>> ToggleAdminStatus(string id)
    {
        try
        {
            var admin = await _authService.ToggleAdminStatusAsync(id);
            if (admin == null)
                return NotFound();

            return Ok(admin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling admin status {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Change admin password
    /// </summary>
    [HttpPost("{id}/change-password")]
    public async Task<ActionResult> ChangeAdminPassword(string id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ChangeAdminPasswordAsync(id, request);
            if (!result)
                return NotFound();

            return Ok("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing admin password {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get admin permissions
    /// </summary>
    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<AdminPermissionsDto>> GetAdminPermissions(string id)
    {
        try
        {
            var permissions = await _authService.GetAdminPermissionsAsync(id);
            if (permissions == null)
                return NotFound();

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin permissions {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update admin permissions
    /// </summary>
    [HttpPut("{id}/permissions")]
    public async Task<ActionResult> UpdateAdminPermissions(string id, [FromBody] UpdatePermissionsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.UpdateAdminPermissionsAsync(id, request);
            if (!result)
                return NotFound();

            return Ok("Permissions updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin permissions {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get admin activity log
    /// </summary>
    [HttpGet("{id}/activity")]
    public async Task<ActionResult<IEnumerable<AdminActivityDto>>> GetAdminActivity(string id, [FromQuery] ActivityFilter filter)
    {
        try
        {
            var activities = await _authService.GetAdminActivityAsync(id, filter);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin activity {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get admin statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetAdminStats()
    {
        try
        {
            var stats = await _authService.GetAdminStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin stats");
            return StatusCode(500, "Internal server error");
        }
    }
}
