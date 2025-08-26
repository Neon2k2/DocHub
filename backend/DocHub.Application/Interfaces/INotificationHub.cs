using DocHub.Application.Interfaces;

namespace DocHub.Application.Interfaces;

public interface INotificationHub
{
    Task SendToUserAsync(string userId, string method, object message);
    Task SendToGroupAsync(string groupName, string method, object message);
    Task SendToAllAsync(string method, object message);
    Task SendToClientAsync(string connectionId, string method, object message);
}
