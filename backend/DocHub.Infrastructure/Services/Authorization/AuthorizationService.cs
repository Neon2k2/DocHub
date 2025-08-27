using DocHub.Core.Entities.Authorization;
using DocHub.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocHub.Infrastructure.Services.Authorization;

public interface IAuthorizationService
{
    Task<bool> HasPermissionAsync(string userId, string userType, string permissionName);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, string userType);
    Task<bool> AssignRoleToUserAsync(string userId, string userType, string roleId);
    Task<bool> RemoveRoleFromUserAsync(string userId, string roleId);
    Task<bool> AddPermissionToRoleAsync(string roleId, string permissionId);
    Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);
}

public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILoggingService _loggingService;

    public AuthorizationService(
        IUserRoleRepository userRoleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository,
        ILoggingService loggingService)
    {
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
        _loggingService = loggingService;
    }

    public async Task<bool> HasPermissionAsync(string userId, string userType, string permissionName)
    {
        try
        {
            var userRoles = await _userRoleRepository
                .GetQueryable()
                .Where(ur => ur.UserId == userId && ur.UserType == userType)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!userRoles.Any())
                return false;

            var permission = await _permissionRepository
                .GetQueryable()
                .FirstOrDefaultAsync(p => p.Name == permissionName);

            if (permission == null)
                return false;

            var hasPermission = await _rolePermissionRepository
                .GetQueryable()
                .AnyAsync(rp => userRoles.Contains(rp.RoleId) && rp.PermissionId == permission.Id);

            return hasPermission;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "HasPermission", ex.Message, new { userId, userType, permissionName });
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, string userType)
    {
        try
        {
            var permissions = await _userRoleRepository
                .GetQueryable()
                .Where(ur => ur.UserId == userId && ur.UserType == userType)
                .Join(_rolePermissionRepository.GetQueryable(),
                    ur => ur.RoleId,
                    rp => rp.RoleId,
                    (ur, rp) => rp.PermissionId)
                .Join(_permissionRepository.GetQueryable(),
                    permId => permId,
                    perm => perm.Id,
                    (permId, perm) => perm.Name)
                .Distinct()
                .ToListAsync();

            return permissions;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "GetUserPermissions", ex.Message, new { userId, userType });
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string userType, string roleId)
    {
        try
        {
            var existingRole = await _roleRepository.GetByIdAsync(roleId);
            if (existingRole == null)
                return false;

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                UserType = userType
            };

            await _userRoleRepository.AddAsync(userRole);
            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "AssignRoleToUser", ex.Message, new { userId, userType, roleId });
            return false;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleId)
    {
        try
        {
            var userRole = await _userRoleRepository
                .GetQueryable()
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
                return false;

            await _userRoleRepository.DeleteAsync(userRole);
            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "RemoveRoleFromUser", ex.Message, new { userId, roleId });
            return false;
        }
    }

    public async Task<bool> AddPermissionToRoleAsync(string roleId, string permissionId)
    {
        try
        {
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            await _rolePermissionRepository.AddAsync(rolePermission);
            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "AddPermissionToRole", ex.Message, new { roleId, permissionId });
            return false;
        }
    }

    public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
    {
        try
        {
            var rolePermission = await _rolePermissionRepository
                .GetQueryable()
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
                return false;

            await _rolePermissionRepository.DeleteAsync(rolePermission);
            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Authorization", "RemovePermissionFromRole", ex.Message, new { roleId, permissionId });
            return false;
        }
    }
}
