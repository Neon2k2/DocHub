using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsSuperAdmin => Role?.ToLower() == "superadmin";
    public List<string> Permissions => GetPermissionsFromRole(Role);
    
    private List<string> GetPermissionsFromRole(string? role)
    {
        return role?.ToLower() switch
        {
            "superadmin" => new List<string> { "read", "write", "delete", "admin", "manage_users", "manage_templates", "manage_signatures" },
            "admin" => new List<string> { "read", "write", "delete", "admin", "manage_templates", "manage_signatures" },
            "manager" => new List<string> { "read", "write", "manage_templates" },
            "user" => new List<string> { "read", "write" },
            _ => new List<string> { "read" }
        };
    }
}

public class CreateUserDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? Role { get; set; }
}

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}
