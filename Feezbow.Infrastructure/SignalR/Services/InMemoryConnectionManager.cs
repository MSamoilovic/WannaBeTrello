using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Feezbow.Infrastructure.SignalR.Services;


public sealed class InMemoryConnectionManager(ILogger<InMemoryConnectionManager> logger)
    : IConnectionManager
{
    // userId → set of connectionIds
    private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

    // connectionId → userId  (reverse lookup)
    private readonly ConcurrentDictionary<string, string> _connectionUser = new();

    private readonly object _lock = new();

    public Task AddConnectionAsync(string userId, string connectionId)
    {
        lock (_lock)
        {
            _userConnections
                .GetOrAdd(userId, _ => new HashSet<string>())
                .Add(connectionId);

            _connectionUser[connectionId] = userId;
        }

        logger.LogDebug(
            "Connection {ConnectionId} registered for user {UserId}",
            connectionId, userId);

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(string connectionId)
    {
        lock (_lock)
        {
            if (!_connectionUser.TryRemove(connectionId, out var userId))
                return Task.CompletedTask;

            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                    _userConnections.TryRemove(userId, out _);
            }

            logger.LogDebug(
                "Connection {ConnectionId} removed for user {UserId}",
                connectionId, userId);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> GetConnectionsForUserAsync(string userId)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
                return Task.FromResult<IReadOnlyCollection<string>>(connections.ToArray());
        }

        return Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());
    }

    public Task<bool> IsUserOnlineAsync(string userId) =>
        Task.FromResult(_userConnections.ContainsKey(userId));

    public Task<int> GetOnlineUserCountAsync() =>
        Task.FromResult(_userConnections.Count);
}
