using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class Notification : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? Data { get; set; }
    
    public bool IsDelivered { get; set; } = false;
    
    public DateTime? DeliveredAt { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTime? ReadAt { get; set; }
    
    [StringLength(50)]
    public string Priority { get; set; } = "Normal";
    
    public string? SenderId { get; set; }
    
    public string? GroupName { get; set; }
    
    public string? RecipientId { get; set; }
}
