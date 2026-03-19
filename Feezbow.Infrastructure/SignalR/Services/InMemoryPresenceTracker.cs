using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Feezbow.Infrastructure.SignalR.Services;

/// <summary>
/// In-memory implementation of <see cref="IPresenceTracker"/>.
/// Retains last-seen timestamps even after a user goes offline.
/// </summary>
public sealed class InMemoryPresenceTracker(ILogger<InMemoryPresenceTracker> logger)
    : IPresenceTracker
{
    private sealed class UserState
    {
        public HashSet<string> ConnectionIds { get; } = [];
        public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;
        public bool IsOnline => ConnectionIds.Count > 0;
    }

    private readonly ConcurrentDictionary<string, UserState> _presence = new();
    private readonly object _lock = new();

    public Task UserConnectedAsync(string userId, string connectionId)
    {
        lock (_lock)
        {
            var state = _presence.GetOrAdd(userId, _ => new UserState());
            state.ConnectionIds.Add(connectionId);
            state.LastSeen = DateTimeOffset.UtcNow;
        }

        logger.LogInformation(
            "User {UserId} connected ({ConnectionId}) – now online",
            userId, connectionId);

        return Task.CompletedTask;
    }

    public Task UserDisconnectedAsync(string userId, string connectionId)
    {
        bool wentOffline = false;

        lock (_lock)
        {
            if (_presence.TryGetValue(userId, out var state))
            {
                state.ConnectionIds.Remove(connectionId);
                state.LastSeen = DateTimeOffset.UtcNow;
                wentOffline = !state.IsOnline;
            }
        }

        if (wentOffline)
        {
            logger.LogInformation("User {UserId} has no more connections – now offline", userId);
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsOnlineAsync(string userId) =>
        Task.FromResult(_presence.TryGetValue(userId, out var s) && s.IsOnline);

    public Task<DateTimeOffset?> GetLastSeenAsync(string userId)
    {
        DateTimeOffset? result = _presence.TryGetValue(userId, out var s)
            ? s.LastSeen
            : null;

        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<string>> GetOnlineUsersAsync()
    {
        lock (_lock)
        {
            var online = _presence
                .Where(kv => kv.Value.IsOnline)
                .Select(kv => kv.Key)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<string>>(online);
        }
    }
}
