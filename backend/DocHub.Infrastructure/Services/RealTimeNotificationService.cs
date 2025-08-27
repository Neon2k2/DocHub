using DocHub.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using DocHub.Infrastructure.Data;
using DocHub.Core.Entities;
using DocHub.Application.DTOs;
using Microsoft.EntityFrameworkCore;


namespace DocHub.Infrastructure.Services;

public class RealTimeNotificationService : IRealTimeNotificationService
{
    private readonly ILogger<RealTimeNotificationService> _logger;
    private readonly DocHubDbContext _context;
    private readonly INotificationHub _notificationHub;
    private readonly Dictionary<string, HashSet<string>> _userGroups;
    private readonly Dictionary<string, string> _userConnections;

    public RealTimeNotificationService(
        ILogger<RealTimeNotificationService> logger,
        DocHubDbContext context,
        INotificationHub notificationHub)
    {
        _logger = logger;
        _context = context;
        _notificationHub = notificationHub;
        _userGroups = new Dictionary<string, HashSet<string>>();
        _userConnections = new Dictionary<string, string>();
    }

    public async Task NotifyUserAsync(string userId, NotificationMessage message)
    {
        try
        {
            if (_userConnections.TryGetValue(userId, out var connectionId))
            {
                // Send real-time notification via SignalR
                await _notificationHub.SendToClientAsync(connectionId, "ReceiveNotification", message);
                _logger.LogInformation("Notification sent to user {UserId} via connection {ConnectionId}", userId, connectionId);
            }
            else
            {
                _logger.LogWarning("User {UserId} is not online, storing notification for later delivery", userId);
                // Store notification in database for offline users
                await StoreNotificationForOfflineUserAsync(userId, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
        }
    }

    public async Task NotifyGroupAsync(string groupName, NotificationMessage message)
    {
        try
        {
            var groupMembers = _userGroups.Values
                .Where(groups => groups.Contains(groupName))
                .SelectMany(groups => groups)
                .Distinct()
                .ToList();

            if (groupMembers.Any())
            {
                // Send to all group members
                await _notificationHub.SendToGroupAsync(groupName, "ReceiveGroupNotification", message);
                _logger.LogInformation("Group notification sent to group {GroupName} with {Count} members", groupName, groupMembers.Count);
            }
            else
            {
                _logger.LogWarning("No online members found for group {GroupName}", groupName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending group notification to group {GroupName}", groupName);
        }
    }

    public async Task NotifyAllUsersAsync(NotificationMessage message)
    {
        try
        {
            // Send to all connected users
            await _notificationHub.SendToAllAsync("ReceiveGlobalNotification", message);
            _logger.LogInformation("Global notification sent to all users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending global notification");
        }
    }

    public Task AddUserToGroupAsync(string userId, string groupName)
    {
        try
        {
            if (!_userGroups.ContainsKey(userId))
            {
                _userGroups[userId] = new HashSet<string>();
            }

            _userGroups[userId].Add(groupName);
            _logger.LogInformation("User {UserId} added to group {GroupName}", userId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to group {GroupName}", userId, groupName);
        }
        
        return Task.CompletedTask;
    }

    public Task RemoveUserFromGroupAsync(string userId, string groupName)
    {
        try
        {
            if (_userGroups.ContainsKey(userId))
            {
                _userGroups[userId].Remove(groupName);
            }

            _logger.LogInformation("User {UserId} removed from group {GroupName}", userId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from group {GroupName}", userId, groupName);
        }
        
        return Task.CompletedTask;
    }

    public Task RegisterUserConnectionAsync(string userId, string connectionId)
    {
        try
        {
            _userConnections[userId] = connectionId;
            _logger.LogInformation("User {UserId} registered with connection {ConnectionId}", userId, connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user connection {UserId} -> {ConnectionId}", userId, connectionId);
        }
        
        return Task.CompletedTask;
    }

    public Task RemoveUserConnectionAsync(string userId)
    {
        try
        {
            if (_userConnections.Remove(userId))
            {
                _logger.LogInformation("User {UserId} connection removed", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user connection {UserId}", userId);
        }
        
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync(string userId)
    {
        try
        {
            // Get pending notifications from database
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsDelivered)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationMessage
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                Data = n.Data,
                CreatedAt = n.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending notifications for user {UserId}", userId);
            return Enumerable.Empty<NotificationMessage>();
        }
    }

    public async Task MarkNotificationAsDeliveredAsync(string notificationId)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsDelivered = true;
                notification.DeliveredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Notification {NotificationId} marked as delivered", notificationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as delivered", notificationId);
        }
    }

    // Advanced notification features
    public Task<IEnumerable<string>> GetUserGroupsAsync(string userId)
    {
        try
        {
            if (_userGroups.TryGetValue(userId, out var groups))
            {
                return Task.FromResult<IEnumerable<string>>(groups);
            }
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups for {UserId}", userId);
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
    }

    public Task<bool> IsUserOnlineAsync(string userId)
    {
        try
        {
            return Task.FromResult(_userConnections.ContainsKey(userId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking online status for {UserId}", userId);
            return Task.FromResult(false);
        }
    }

    public Task<int> GetOnlineUserCountAsync()
    {
        try
        {
            return Task.FromResult(_userConnections.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online user count");
            return Task.FromResult(0);
        }
    }

    public Task<IEnumerable<string>> GetOnlineUsersAsync()
    {
        try
        {
            return Task.FromResult<IEnumerable<string>>(_userConnections.Keys.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online users");
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
    }

    public async Task UpdateUserNotificationPreferencesAsync(string userId, NotificationPreferences preferences)
    {
        try
        {
            var existingPrefs = await _context.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingPrefs != null)
            {
                // Update existing preferences
                existingPrefs.EmailNotifications = preferences.EmailNotifications;
                existingPrefs.PushNotifications = preferences.PushNotifications;
                existingPrefs.InAppNotifications = preferences.InAppNotifications;
                existingPrefs.EnabledTypes = string.Join(",", preferences.EnabledTypes);
                existingPrefs.DisabledTypes = string.Join(",", preferences.DisabledTypes);
                existingPrefs.QuietHoursStart = preferences.QuietHoursStart;
                existingPrefs.QuietHoursEnd = preferences.QuietHoursEnd;
                existingPrefs.QuietHoursEnabled = preferences.QuietHoursEnabled;
                existingPrefs.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new preferences
                var newPrefs = new NotificationPreferences
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    EmailNotifications = preferences.EmailNotifications,
                    PushNotifications = preferences.PushNotifications,
                    InAppNotifications = preferences.InAppNotifications,
                    EnabledTypes = preferences.EnabledTypes != null ? string.Join(",", preferences.EnabledTypes) : "",
                    DisabledTypes = preferences.DisabledTypes != null ? string.Join(",", preferences.DisabledTypes) : "",
                    QuietHoursStart = preferences.QuietHoursStart,
                    QuietHoursEnd = preferences.QuietHoursEnd,
                    QuietHoursEnabled = preferences.QuietHoursEnabled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.NotificationPreferences.Add(newPrefs);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Notification preferences updated for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences for user {UserId}", userId);
        }
    }

    public async Task<NotificationPreferences> GetUserNotificationPreferencesAsync(string userId)
    {
        try
        {
            var prefs = await _context.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (prefs != null)
            {
                return prefs;
            }

            // Return default preferences
            return new NotificationPreferences
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                EmailNotifications = true,
                PushNotifications = true,
                InAppNotifications = true,
                EnabledTypes = "general,letter,email,system",
                DisabledTypes = "",
                QuietHoursStart = TimeSpan.FromHours(22),
                QuietHoursEnd = TimeSpan.FromHours(8),
                QuietHoursEnabled = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences for user {UserId}", userId);
            return new NotificationPreferences { UserId = userId };
        }
    }

    public async Task<bool> IsNotificationEnabledAsync(string userId, string notificationType)
    {
        try
        {
            var prefs = await GetUserNotificationPreferencesAsync(userId);
            
            // Check if notification type is disabled
            if (prefs.DisabledTypes.Contains(notificationType))
                return false;

            // Check if notification type is explicitly enabled
            if (prefs.EnabledTypes.Contains(notificationType))
                return true;

            // Check quiet hours
            if (prefs.QuietHoursEnabled)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                if (currentTime >= prefs.QuietHoursStart || currentTime <= prefs.QuietHoursEnd)
                    return false;
            }

            return prefs.InAppNotifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking notification status for user {UserId}, type {Type}", userId, notificationType);
            return true; // Default to enabled on error
        }
    }

    private async Task StoreNotificationForOfflineUserAsync(string userId, NotificationMessage message)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = message.Type,
                Title = message.Title,
                Message = message.Message,
                Data = message.Data?.ToString(),
                IsDelivered = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Notification stored for offline user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing notification for offline user {UserId}", userId);
        }
    }
}
