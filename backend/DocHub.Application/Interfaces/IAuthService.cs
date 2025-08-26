using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(string username, string password);
    Task<AuthResponseDto> RefreshTokenAsync(string token);
    Task<bool> LogoutAsync(string userId);
    Task<UserDto?> GetCurrentUserAsync(string userId);
    Task<bool> ValidateTokenAsync(string token);
    
    // Password reset functionality
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    
    // Admin management methods
    Task<IEnumerable<UserDto>> GetAllAdminsAsync();
    Task<UserDto?> GetAdminByIdAsync(string id);
    Task<UserDto> CreateAdminAsync(CreateAdminRequest request);
    Task<UserDto?> UpdateAdminAsync(string id, UpdateAdminRequest request);
    Task<bool> DeleteAdminAsync(string id);
    Task<UserDto?> ToggleAdminStatusAsync(string id);
    Task<bool> ChangeAdminPasswordAsync(string id, ChangePasswordRequest request);
    Task<AdminPermissionsDto?> GetAdminPermissionsAsync(string id);
    Task<bool> UpdateAdminPermissionsAsync(string id, UpdatePermissionsRequest request);
    Task<IEnumerable<AdminActivityDto>> GetAdminActivityAsync(string id, ActivityFilter filter);
    Task<AdminStatsDto> GetAdminStatsAsync();
}
