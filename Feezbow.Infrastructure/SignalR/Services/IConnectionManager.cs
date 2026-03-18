namespace WannabeTrello.Infrastructure.SignalR.Services;

public interface IConnectionManager
{
    Task AddConnectionAsync(string userId, string connectionId);
    Task RemoveConnectionAsync(string connectionId);
    Task<IReadOnlyCollection<string>> GetConnectionsForUserAsync(string userId);
    Task<bool> IsUserOnlineAsync(string userId);
    Task<int> GetOnlineUserCountAsync();
}
