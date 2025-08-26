using Microsoft.AspNetCore.SignalR;
using DocHub.Application.DTOs;

namespace DocHub.API.Hubs;

public class EmailStatusHub : Hub
{
    private readonly ILogger<EmailStatusHub> _logger;

    public EmailStatusHub(ILogger<EmailStatusHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
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
