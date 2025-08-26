using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class CreateAdminRequest
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Role { get; set; } = "Admin";
    
    public bool IsSuperAdmin { get; set; } = false;
    
    public List<string> Permissions { get; set; } = new();
}

public class UpdateAdminRequest
{
    [StringLength(100)]
    public string? FullName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [StringLength(100)]
    public string? Role { get; set; }
    
    public bool? IsActive { get; set; }
    
    public List<string>? Permissions { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AdminPermissionsDto
{
    public string AdminId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public List<string> Permissions { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class UpdatePermissionsRequest
{
    [Required]
    public List<string> Permissions { get; set; } = new();
    
    public string? Role { get; set; }
    
    public bool? IsSuperAdmin { get; set; }
}

public class AdminActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string AdminId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ActivityFilter
{
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminStatsDto
{
    public int TotalAdmins { get; set; }
    public int ActiveAdmins { get; set; }
    public int SuperAdmins { get; set; }
    public int RegularAdmins { get; set; }
    public DateTime LastAdminCreated { get; set; }
    public DateTime LastAdminLogin { get; set; }
    public Dictionary<string, int> AdminsByRole { get; set; } = new();
    public List<RecentAdminActivity> RecentActivity { get; set; } = new();
}

public class RecentAdminActivity
{
    public string AdminId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
