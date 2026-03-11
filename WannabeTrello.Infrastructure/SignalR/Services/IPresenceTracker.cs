namespace WannabeTrello.Infrastructure.SignalR.Services;

public interface IPresenceTracker
{
    Task UserConnectedAsync(string userId, string connectionId);
    Task UserDisconnectedAsync(string userId, string connectionId);
    Task<bool> IsOnlineAsync(string userId);
    Task<DateTimeOffset?> GetLastSeenAsync(string userId);
    Task<IReadOnlyCollection<string>> GetOnlineUsersAsync();
}
