namespace Feezbow.Infrastructure.SignalR.Services;

/// <summary>
/// Tracks which SignalR groups each connection belongs to.
/// Complements SignalR's built-in group management with queryable membership data,
/// enabling features like "how many users are watching Board:42?" and
/// automatic cleanup when a connection disconnects.
/// </summary>
public interface IHubGroupManager
{
    /// <summary>Records that a connection has joined a group.</summary>
    Task TrackGroupMembershipAsync(string connectionId, string groupName);

    /// <summary>Records that a connection has left a group.</summary>
    Task UntrackGroupMembershipAsync(string connectionId, string groupName);

    /// <summary>
    /// Removes a connection from all tracked groups.
    /// Call this in OnDisconnectedAsync before SignalR's own cleanup.
    /// </summary>
    Task ClearGroupMembershipsAsync(string connectionId);

    /// <summary>Returns all group names the connection is currently tracked in.</summary>
    Task<IReadOnlyCollection<string>> GetGroupsForConnectionAsync(string connectionId);

    /// <summary>Returns the number of connections currently tracked in a group.</summary>
    Task<int> GetConnectionCountInGroupAsync(string groupName);
}
