using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class NotificationPreferences : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public bool EmailNotifications { get; set; } = true;
    
    public bool PushNotifications { get; set; } = true;
    
    public bool InAppNotifications { get; set; } = true;
    
    public string? EnabledTypes { get; set; }
    
    public string? DisabledTypes { get; set; }
    
    public TimeSpan QuietHoursStart { get; set; } = TimeSpan.FromHours(22);
    
    public TimeSpan QuietHoursEnd { get; set; } = TimeSpan.FromHours(8);
    
    public bool QuietHoursEnabled { get; set; } = false;
}
