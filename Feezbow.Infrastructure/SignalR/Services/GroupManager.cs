using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace WannabeTrello.Infrastructure.SignalR.Services;

/// <summary>
/// In-memory implementation of <see cref="IHubGroupManager"/>.
/// Maintains a bidirectional index: connection → groups and group → connections.
/// </summary>
public sealed class GroupManager(ILogger<GroupManager> logger) : IHubGroupManager
{
    // connectionId → groups the connection belongs to
    private readonly ConcurrentDictionary<string, HashSet<string>> _connectionGroups = new();

    // groupName → connections in that group
    private readonly ConcurrentDictionary<string, HashSet<string>> _groupConnections = new();

    private readonly object _lock = new();

    public Task TrackGroupMembershipAsync(string connectionId, string groupName)
    {
        lock (_lock)
        {
            _connectionGroups
                .GetOrAdd(connectionId, _ => new HashSet<string>())
                .Add(groupName);

            _groupConnections
                .GetOrAdd(groupName, _ => new HashSet<string>())
                .Add(connectionId);
        }

        logger.LogDebug(
            "Connection {ConnectionId} tracked in group {Group}",
            connectionId, groupName);

        return Task.CompletedTask;
    }

    public Task UntrackGroupMembershipAsync(string connectionId, string groupName)
    {
        lock (_lock)
        {
            RemoveFromSet(_connectionGroups, connectionId, groupName);
            RemoveFromSet(_groupConnections, groupName, connectionId);
        }

        return Task.CompletedTask;
    }

    public Task ClearGroupMembershipsAsync(string connectionId)
    {
        lock (_lock)
        {
            if (!_connectionGroups.TryRemove(connectionId, out var groups))
                return Task.CompletedTask;

            foreach (var group in groups)
                RemoveFromSet(_groupConnections, group, connectionId);

            logger.LogDebug(
                "Cleared {Count} group memberships for connection {ConnectionId}",
                groups.Count, connectionId);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> GetGroupsForConnectionAsync(string connectionId)
    {
        lock (_lock)
        {
            if (_connectionGroups.TryGetValue(connectionId, out var groups))
                return Task.FromResult<IReadOnlyCollection<string>>(groups.ToArray());
        }

        return Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());
    }

    public Task<int> GetConnectionCountInGroupAsync(string groupName)
    {
        lock (_lock)
        {
            if (_groupConnections.TryGetValue(groupName, out var connections))
                return Task.FromResult(connections.Count);
        }

        return Task.FromResult(0);
    }

   
    private static void RemoveFromSet(
        ConcurrentDictionary<string, HashSet<string>> dict,
        string key,
        string value)
    {
        if (!dict.TryGetValue(key, out var set)) return;
        set.Remove(value);
        if (set.Count == 0) dict.TryRemove(key, out _);
    }
}
