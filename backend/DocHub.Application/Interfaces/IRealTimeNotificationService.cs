using DocHub.Core.Entities;

namespace DocHub.Application.Interfaces;

public interface IRealTimeNotificationService
{
    Task NotifyUserAsync(string userId, NotificationMessage message);
    Task NotifyGroupAsync(string groupName, NotificationMessage message);
    Task NotifyAllUsersAsync(NotificationMessage message);
    Task AddUserToGroupAsync(string userId, string groupName);
    Task RemoveUserFromGroupAsync(string userId, string groupName);
    Task RegisterUserConnectionAsync(string userId, string connectionId);
    Task RemoveUserConnectionAsync(string userId);
    Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync(string userId);
    Task MarkNotificationAsDeliveredAsync(string notificationId);
    
    // Advanced notification features
    Task<IEnumerable<string>> GetUserGroupsAsync(string userId);
    Task<bool> IsUserOnlineAsync(string userId);
    Task<int> GetOnlineUserCountAsync();
    Task<IEnumerable<string>> GetOnlineUsersAsync();
    Task UpdateUserNotificationPreferencesAsync(string userId, NotificationPreferences preferences);
    Task<NotificationPreferences> GetUserNotificationPreferencesAsync(string userId);
    Task<bool> IsNotificationEnabledAsync(string userId, string notificationType);
}

public class NotificationMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public bool IsRead { get; set; } = false;
    public string? SenderId { get; set; }
    public string? RecipientId { get; set; }
    public string? GroupName { get; set; }
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}


