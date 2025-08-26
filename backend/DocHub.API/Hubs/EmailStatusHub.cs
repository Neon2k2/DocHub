using Microsoft.AspNetCore.SignalR;
using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DocHub.API.Hubs;

public class EmailStatusHub : Hub
{
    private readonly ILogger<EmailStatusHub> _logger;
    private readonly IRealTimeNotificationService _notificationService;

    public EmailStatusHub(
        ILogger<EmailStatusHub> logger,
        IRealTimeNotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        
        // Extract user ID from connection (you might want to implement JWT authentication here)
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;
        
        // Add user connection to notification service
        await _notificationService.AddUserConnectionAsync(userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        
        // Remove user connection from notification service
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;
        await _notificationService.RemoveUserConnectionAsync(userId);
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinEmailGroup(string emailId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"email_{emailId}");
        _logger.LogInformation("Client {ConnectionId} joined email group {EmailId}", Context.ConnectionId, emailId);
    }

    public async Task LeaveEmailGroup(string emailId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"email_{emailId}");
        _logger.LogInformation("Client {ConnectionId} left email group {EmailId}", Context.ConnectionId, emailId);
    }

    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        _logger.LogInformation("Client {ConnectionId} joined user group {UserId}", Context.ConnectionId, userId);
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        _logger.LogInformation("Client {ConnectionId} left user group {UserId}", Context.ConnectionId, userId);
    }

    public async Task JoinNotificationGroup(string groupName)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;
        await _notificationService.AddUserToGroupAsync(userId, groupName);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined notification group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveNotificationGroup(string groupName)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;
        await _notificationService.RemoveUserFromGroupAsync(userId, groupName);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left notification group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task SendNotification(string userId, string message)
    {
        var senderId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;
        var notification = new NotificationMessage
        {
            Type = "user-message",
            Title = "New Message",
            Message = message,
            SenderId = senderId,
            RecipientId = userId,
            Priority = NotificationPriority.Normal
        };

        await _notificationService.NotifyUserAsync(userId, notification);
        _logger.LogInformation("Notification sent from {SenderId} to {UserId}", senderId, userId);
    }

    public async Task SendGroupNotification(string groupName, string message)
    {
        var senderId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;
        var notification = new NotificationMessage
        {
            Type = "group-message",
            Title = "Group Message",
            Message = message,
            SenderId = senderId,
            GroupName = groupName,
            Priority = NotificationPriority.Normal
        };

        await _notificationService.NotifyGroupAsync(groupName, notification);
        _logger.LogInformation("Group notification sent to {GroupName} from {SenderId}", groupName, senderId);
    }
}

public static class EmailStatusHubExtensions
{
    public static async Task NotifyEmailStatusUpdate(this IHubContext<EmailStatusHub> hubContext, string emailId, EmailStatusUpdateDto statusUpdate)
    {
        await hubContext.Clients.Group($"email_{emailId}").SendAsync("EmailStatusUpdated", statusUpdate);
    }

    public static async Task NotifyUserEmailUpdate(this IHubContext<EmailStatusHub> hubContext, string userId, EmailStatusUpdateDto statusUpdate)
    {
        await hubContext.Clients.Group($"user_{userId}").SendAsync("UserEmailUpdated", statusUpdate);
    }

    public static async Task NotifyAllUsers(this IHubContext<EmailStatusHub> hubContext, string message, string type = "info")
    {
        await hubContext.Clients.All.SendAsync("GlobalNotification", new { message, type, timestamp = DateTime.UtcNow });
    }
}

public class EmailStatusUpdateDto
{
    public string EmailId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public object? Data { get; set; }
}
