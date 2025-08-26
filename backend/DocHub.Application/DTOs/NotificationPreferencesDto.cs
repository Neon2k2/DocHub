namespace DocHub.Application.DTOs;

public class NotificationPreferencesDto
{
    public string UserId { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool InAppNotifications { get; set; } = true;
    public List<string> EnabledTypes { get; set; } = new();
    public List<string> DisabledTypes { get; set; } = new();
    public TimeSpan QuietHoursStart { get; set; } = TimeSpan.FromHours(22);
    public TimeSpan QuietHoursEnd { get; set; } = TimeSpan.FromHours(8);
    public bool QuietHoursEnabled { get; set; } = false;
}
