using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DocHub.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<Admin> _adminRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IGenericRepository<Admin> adminRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _adminRepository = adminRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(string username, string password)
    {
        try
        {
            var admin = await _adminRepository.FirstOrDefaultAsync(a =>
                a.Username == username && a.IsActive);

            if (admin == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            if (!VerifyPassword(password, admin.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Update last login info
            admin.LastLoginAt = DateTime.UtcNow;
            await _adminRepository.UpdateAsync(admin);

            // Generate JWT token
            var token = GenerateJwtToken(admin);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = admin.Id,
                    Username = admin.Username,
                    Email = admin.Email,
                    FirstName = admin.FullName.Split(' ').FirstOrDefault() ?? admin.FullName,
                    LastName = admin.FullName.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                    Role = admin.Role,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                    LastLoginAt = admin.LastLoginAt
                }
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Login failed: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string token)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid token"
                };
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid token"
                };
            }

            var admin = await _adminRepository.GetByIdAsync(userId);
            if (admin == null || !admin.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found or inactive"
                };
            }

            var newToken = GenerateJwtToken(admin);
            var newRefreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = newToken,
                RefreshToken = newRefreshToken
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Token refresh failed: {ex.Message}"
            };
        }
    }

    public Task<bool> LogoutAsync(string userId)
    {
        try
        {
            // In a real implementation, you might want to blacklist the token
            // For now, just return success
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        try
        {
            var admin = await _adminRepository.GetByIdAsync(userId);
            if (admin == null || !admin.IsActive)
                return null;

            return new UserDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                FirstName = admin.FullName.Split(' ').FirstOrDefault() ?? admin.FullName,
                LastName = admin.FullName.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                Role = admin.Role,
                IsActive = admin.IsActive,
                CreatedAt = admin.CreatedAt,
                LastLoginAt = admin.LastLoginAt
            };
        }
        catch
        {
            return null;
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        try
        {
            var admin = await _adminRepository.FirstOrDefaultAsync(a => a.Email == email && a.IsActive);
            if (admin == null)
            {
                // Don't reveal if email exists or not for security
                return true;
            }

            // TODO: Generate reset token and send email
            // For now, just log the request
            _logger.LogInformation("Password reset requested for email: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for email: {Email}", email);
            return false;
        }
    }

    public Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            // TODO: Validate reset token and update password
            // For now, just log the request
            _logger.LogInformation("Password reset attempted with token: {Token}", token);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password with token: {Token}", token);
            return Task.FromResult(false);
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            var admin = await _adminRepository.GetByIdAsync(userId);
            if (admin == null)
            {
                return false;
            }

            if (!VerifyPassword(currentPassword, admin.PasswordHash))
            {
                return false;
            }

            admin.PasswordHash = HashPassword(newPassword);
            await _adminRepository.UpdateAsync(admin);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        // Simple password verification - in production, use proper hashing
        return passwordHash == HashPassword(password);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private string GenerateJwtToken(Admin admin)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "default-secret-key-12345"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.GivenName, admin.FullName),
            new Claim("IsSuperAdmin", admin.IsSuperAdmin.ToString()),
            new Claim("Role", admin.Role ?? "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "DocHub",
            audience: _configuration["Jwt:Audience"] ?? "DocHub",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-12345")),
            ValidateLifetime = false // We want to get the principal even if token is expired
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    // Admin management methods
    public async Task<IEnumerable<UserDto>> GetAllAdminsAsync()
    {
        try
        {
            var admins = await _adminRepository.GetAllAsync();
            return admins.Select(MapAdminToUserDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all admins");
            throw;
        }
    }

    public async Task<UserDto?> GetAdminByIdAsync(string id)
    {
        try
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            return admin != null ? MapAdminToUserDto(admin) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin by id {Id}", id);
            throw;
        }
    }

    public async Task<UserDto> CreateAdminAsync(CreateAdminRequest request)
    {
        try
        {
            // Check if username already exists
            var existingAdmin = await _adminRepository.FirstOrDefaultAsync(a => a.Username == request.Username);
            if (existingAdmin != null)
            {
                throw new InvalidOperationException("Username already exists");
            }

            var admin = new Admin
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = HashPassword(request.Password),
                Role = request.Role ?? "Admin",
                IsSuperAdmin = request.IsSuperAdmin,
                IsActive = true,
                Permissions = request.Permissions != null ? JsonSerializer.Serialize(request.Permissions) : null
            };

            var createdAdmin = await _adminRepository.AddAsync(admin);
            return MapAdminToUserDto(createdAdmin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin");
            throw;
        }
    }

    public async Task<UserDto?> UpdateAdminAsync(string id, UpdateAdminRequest request)
    {
        try
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null)
                return null;

            if (!string.IsNullOrWhiteSpace(request.Email))
                admin.Email = request.Email;
            
            if (!string.IsNullOrWhiteSpace(request.FullName))
                admin.FullName = request.FullName;
            
            if (!string.IsNullOrWhiteSpace(request.Role))
                admin.Role = request.Role;
            
            if (request.IsActive.HasValue)
                admin.IsActive = request.IsActive.Value;
            
            if (request.Permissions != null)
                admin.Permissions = JsonSerializer.Serialize(request.Permissions);

            var updatedAdmin = await _adminRepository.UpdateAsync(admin);
            return updatedAdmin != null ? MapAdminToUserDto(updatedAdmin) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAdminAsync(string id)
    {
        try
        {
            return await _adminRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting admin {Id}", id);
            throw;
        }
    }

    public async Task<UserDto?> ToggleAdminStatusAsync(string id)
    {
        try
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null)
                return null;

            admin.IsActive = !admin.IsActive;
            var updatedAdmin = await _adminRepository.UpdateAsync(admin);
            return updatedAdmin != null ? MapAdminToUserDto(updatedAdmin) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling admin status {Id}", id);
            throw;
        }
    }

    public async Task<bool> ChangeAdminPasswordAsync(string id, ChangePasswordRequest request)
    {
        try
        {
            if (request.NewPassword != request.ConfirmPassword)
                return false;

            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null)
                return false;

            admin.PasswordHash = HashPassword(request.NewPassword);
            await _adminRepository.UpdateAsync(admin);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing admin password {Id}", id);
            throw;
        }
    }

    public async Task<AdminPermissionsDto?> GetAdminPermissionsAsync(string id)
    {
        try
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null)
                return null;

            var permissions = !string.IsNullOrEmpty(admin.Permissions) 
                ? JsonSerializer.Deserialize<List<string>>(admin.Permissions) ?? new List<string>()
                : new List<string>();

            return new AdminPermissionsDto
            {
                AdminId = admin.Id.ToString(),
                Username = admin.Username,
                Role = admin.Role ?? "Admin",
                IsSuperAdmin = admin.IsSuperAdmin,
                Permissions = permissions,
                LastUpdated = admin.UpdatedAt ?? admin.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin permissions {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateAdminPermissionsAsync(string id, UpdatePermissionsRequest request)
    {
        try
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null)
                return false;

            if (request.Permissions != null)
                admin.Permissions = JsonSerializer.Serialize(request.Permissions);
            
            if (!string.IsNullOrWhiteSpace(request.Role))
                admin.Role = request.Role;
            
            if (request.IsSuperAdmin.HasValue)
                admin.IsSuperAdmin = request.IsSuperAdmin.Value;

            await _adminRepository.UpdateAsync(admin);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin permissions {Id}", id);
            throw;
        }
    }

    public Task<IEnumerable<AdminActivityDto>> GetAdminActivityAsync(string id, ActivityFilter filter)
    {
        try
        {
            // TODO: Implement actual activity logging
            // This would involve querying an activity log table
            return Task.FromResult<IEnumerable<AdminActivityDto>>(new List<AdminActivityDto>
            {
                new AdminActivityDto
                {
                    Id = Guid.NewGuid().ToString(),
                    AdminId = id,
                    Username = "Admin",
                    Action = "Login",
                    Description = "User logged in",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    IpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0",
                    Metadata = new Dictionary<string, object>()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin activity {Id}", id);
            throw;
        }
    }

    public async Task<AdminStatsDto> GetAdminStatsAsync()
    {
        try
        {
            var allAdmins = await _adminRepository.GetAllAsync();
            
            return new AdminStatsDto
            {
                TotalAdmins = allAdmins.Count(),
                ActiveAdmins = allAdmins.Count(a => a.IsActive),
                SuperAdmins = allAdmins.Count(a => a.IsSuperAdmin),
                RegularAdmins = allAdmins.Count(a => !a.IsSuperAdmin),
                LastAdminCreated = allAdmins.Any() ? allAdmins.Max(a => a.CreatedAt) : DateTime.UtcNow,
                LastAdminLogin = allAdmins.Any() ? allAdmins.Max(a => a.LastLoginAt ?? DateTime.MinValue) : DateTime.UtcNow,
                AdminsByRole = allAdmins.GroupBy(a => a.Role ?? "Admin").ToDictionary(g => g.Key, g => g.Count()),
                RecentActivity = allAdmins.Take(5).Select(a => new RecentAdminActivity
                {
                    AdminId = a.Id.ToString(),
                    Username = a.Username,
                    Action = "Created",
                    Timestamp = a.CreatedAt
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin stats");
            throw;
        }
    }

    private UserDto MapAdminToUserDto(Admin admin)
    {
        return new UserDto
        {
            Id = admin.Id,
            Username = admin.Username,
            Email = admin.Email,
            FirstName = admin.FullName.Split(' ').FirstOrDefault() ?? admin.FullName,
            LastName = admin.FullName.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
            Role = admin.Role ?? "Admin",
            IsActive = admin.IsActive,
            CreatedAt = admin.CreatedAt,
            LastLoginAt = admin.LastLoginAt
        };
    }
}
