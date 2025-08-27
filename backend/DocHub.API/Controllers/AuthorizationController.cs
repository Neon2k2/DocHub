using DocHub.Application.DTOs;
using DocHub.Infrastructure.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILoggingService _loggingService;

    public AuthorizationController(
        IAuthorizationService authorizationService,
        ILoggingService loggingService)
    {
        _authorizationService = authorizationService;
        _loggingService = loggingService;
    }

    [HttpGet("user/{userId}/permissions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetUserPermissions(string userId, [FromQuery] string userType)
    {
        try
        {
            var permissions = await _authorizationService.GetUserPermissionsAsync(userId, userType);
            return Ok(new ApiResponse<IEnumerable<string>>(permissions));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "GetUserPermissions", ex.Message, new { userId, userType });
            return StatusCode(500, new ApiResponse<IEnumerable<string>>(null, "Failed to retrieve user permissions"));
        }
    }

    [HttpPost("user/{userId}/roles/{roleId}")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignRoleToUser(string userId, string roleId, [FromQuery] string userType)
    {
        try
        {
            var result = await _authorizationService.AssignRoleToUserAsync(userId, userType, roleId);
            if (!result)
                return NotFound(new ApiResponse<bool>(false, "Role not found"));

            return Ok(new ApiResponse<bool>(true));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "AssignRoleToUser", ex.Message, new { userId, roleId, userType });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to assign role to user"));
        }
    }

    [HttpDelete("user/{userId}/roles/{roleId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveRoleFromUser(string userId, string roleId)
    {
        try
        {
            var result = await _authorizationService.RemoveRoleFromUserAsync(userId, roleId);
            if (!result)
                return NotFound(new ApiResponse<bool>(false, "User role not found"));

            return Ok(new ApiResponse<bool>(true));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "RemoveRoleFromUser", ex.Message, new { userId, roleId });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to remove role from user"));
        }
    }

    [HttpPost("roles/{roleId}/permissions/{permissionId}")]
    public async Task<ActionResult<ApiResponse<bool>>> AddPermissionToRole(string roleId, string permissionId)
    {
        try
        {
            var result = await _authorizationService.AddPermissionToRoleAsync(roleId, permissionId);
            return Ok(new ApiResponse<bool>(result));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "AddPermissionToRole", ex.Message, new { roleId, permissionId });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to add permission to role"));
        }
    }

    [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemovePermissionFromRole(string roleId, string permissionId)
    {
        try
        {
            var result = await _authorizationService.RemovePermissionFromRoleAsync(roleId, permissionId);
            if (!result)
                return NotFound(new ApiResponse<bool>(false, "Role permission not found"));

            return Ok(new ApiResponse<bool>(true));
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "RemovePermissionFromRole", ex.Message, new { roleId, permissionId });
            return StatusCode(500, new ApiResponse<bool>(false, "Failed to remove permission from role"));
        }
    }
}
