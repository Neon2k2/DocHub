using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IGenericRepository<Admin> _adminRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IGenericRepository<Admin> adminRepository,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _adminRepository = adminRepository;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var admin = await _adminRepository.GetFirstOrDefaultAsync(a =>
                a.Username == loginRequest.Username && a.IsActive);

            if (admin == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            if (!VerifyPassword(loginRequest.Password, admin.PasswordHash))
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            // Update last login info
            admin.LastLoginAt = DateTime.UtcNow;
            admin.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _adminRepository.UpdateAsync(admin);

            // Generate JWT token
            var token = GenerateJwtToken(admin);
            var refreshToken = GenerateRefreshToken();

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = admin.Id.ToString(),
                    Username = admin.Username,
                    Email = admin.Email,
                    FirstName = admin.FullName.Split(' ').FirstOrDefault() ?? "",
                    LastName = admin.FullName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    Role = admin.Role,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                    LastLoginAt = admin.LastLoginAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", loginRequest.Username);
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(request.RefreshToken);
            if (principal == null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid token"
                });
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid token"
                });
            }

            var admin = await _adminRepository.GetByIdAsync(userId);
            if (admin == null || !admin.IsActive)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found or inactive"
                });
            }

            var newToken = GenerateJwtToken(admin);
            var newRefreshToken = GenerateRefreshToken();

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = newToken,
                RefreshToken = newRefreshToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("request-password-reset")]
    public async Task<ActionResult<PasswordResetResponseDto>> RequestPasswordReset([FromBody] PasswordResetRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var admin = await _adminRepository.GetFirstOrDefaultAsync(a => a.Email == request.Email && a.IsActive);
            if (admin == null)
            {
                // Don't reveal if email exists or not for security
                return Ok(new PasswordResetResponseDto
                {
                    Success = true,
                    Message = "If the email exists, a password reset link has been sent."
                });
            }

            // Generate reset token
            var resetToken = GenerateResetToken();
            var expiresAt = DateTime.UtcNow.AddHours(24);

            // Save reset token to database
            var passwordReset = new PasswordReset
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Token = resetToken,
                ExpiresAt = expiresAt,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = admin.Id.ToString()
            };

            // TODO: Save to PasswordReset table
            // await _passwordResetRepository.AddAsync(passwordReset);

            // TODO: Send email with reset link
            // await _emailService.SendPasswordResetEmailAsync(request.Email, resetToken);

            return Ok(new PasswordResetResponseDto
            {
                Success = true,
                Message = "Password reset link has been sent to your email.",
                ResetToken = resetToken, // Remove this in production
                ExpiresAt = expiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for email: {Email}", request.Email);
            return StatusCode(500, new PasswordResetResponseDto
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<PasswordResetResponseDto>> ResetPassword([FromBody] PasswordResetDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "New password and confirmation password do not match."
                });
            }

            // TODO: Validate reset token from database
            // var passwordReset = await _passwordResetRepository.GetFirstOrDefaultAsync(pr => 
            //     pr.Token == request.Token && 
            //     !pr.IsUsed && 
            //     pr.ExpiresAt > DateTime.UtcNow);

            // if (passwordReset == null)
            // {
            //     return BadRequest(new PasswordResetResponseDto
            //     {
            //         Success = false,
            //         Message = "Invalid or expired reset token."
            //     });
            // }

            // Update admin password
            // var admin = await _adminRepository.GetFirstOrDefaultAsync(a => a.Email == passwordReset.Email);
            // if (admin != null)
            // {
            //     admin.PasswordHash = HashPassword(request.NewPassword);
            //     await _adminRepository.UpdateAsync(admin);
            // }

            // Mark reset token as used
            // passwordReset.IsUsed = true;
            // passwordReset.UsedAt = DateTime.UtcNow;
            // passwordReset.UsedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            // await _passwordResetRepository.UpdateAsync(passwordReset);

            return Ok(new PasswordResetResponseDto
            {
                Success = true,
                Message = "Password has been reset successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new PasswordResetResponseDto
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<PasswordResetResponseDto>> ChangePassword([FromBody] ChangePasswordDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "New password and confirmation password do not match."
                });
            }

            // Get current user from JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "User not authenticated."
                });
            }

            var admin = await _adminRepository.GetByIdAsync(userId);
            if (admin == null)
            {
                return NotFound(new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            // Verify current password
            if (!VerifyPassword(request.CurrentPassword, admin.PasswordHash))
            {
                return BadRequest(new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "Current password is incorrect."
                });
            }

            // Update password
            admin.PasswordHash = HashPassword(request.NewPassword);
            await _adminRepository.UpdateAsync(admin);

            return Ok(new PasswordResetResponseDto
            {
                Success = true,
                Message = "Password changed successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new PasswordResetResponseDto
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        // In a real implementation, you might want to blacklist the token
        // For now, just return success
        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Logged out successfully",
            Data = true
        });
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        try
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Invalid token"
                });
            }

            var userId = userIdClaim.Value;
            var admin = await _adminRepository.GetByIdAsync(userId);
            if (admin == null || !admin.IsActive)
            {
                return Unauthorized(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found or inactive"
                });
            }

            var userDto = new UserDto
            {
                Id = admin.Id.ToString(),
                Username = admin.Username,
                Email = admin.Email,
                FirstName = admin.FullName.Split(' ').FirstOrDefault() ?? "",
                LastName = admin.FullName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                Role = admin.Role,
                IsActive = admin.IsActive,
                CreatedAt = admin.CreatedAt,
                LastLoginAt = admin.LastLoginAt
            };

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Internal server error"
            });
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

    private string GenerateResetToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Substring(0, 22);
    }
}
