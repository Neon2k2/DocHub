using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class Admin : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Role { get; set; }
    
    public bool IsSuperAdmin { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(1000)]
    public string? Permissions { get; set; } // JSON string of permissions
    
    public DateTime? LastLoginAt { get; set; }
    
    public string? LastLoginIp { get; set; }
    
    // Navigation properties
    public virtual ICollection<DynamicTab> CreatedTabs { get; set; } = new List<DynamicTab>();
}
