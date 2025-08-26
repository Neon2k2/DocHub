using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class PasswordReset : BaseEntity
{
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public DateTime ExpiresAt { get; set; }
    
    public bool IsUsed { get; set; }
    
    public DateTime? UsedAt { get; set; }
    
    public string? UsedByIp { get; set; }
}
