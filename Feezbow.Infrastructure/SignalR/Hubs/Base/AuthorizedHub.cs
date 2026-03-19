using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Feezbow.Infrastructure.SignalR.Services;

namespace Feezbow.Infrastructure.SignalR.Hubs.Base;

/// <summary>
/// Base class for all application hubs. Enforces authentication,
/// provides structured logging, and wires connection/group/presence tracking.
/// </summary>
[Authorize]
public abstract class AuthorizedHub<TClient>(
    ILogger logger,
    IConnectionManager connectionManager,
    IHubGroupManager groupManager,
    IPresenceTracker presenceTracker)
    : Hub<TClient>
    where TClient : class
{
    protected IConnectionManager ConnectionManager { get; } = connectionManager;
    protected IHubGroupManager GroupManager { get; } = groupManager;
    protected IPresenceTracker PresenceTracker { get; } = presenceTracker;

    protected string CorrelationId { get; private set; } = string.Empty;

    public override async Task OnConnectedAsync()
    {
        CorrelationId = Guid.NewGuid().ToString();

        var userId = Context.UserIdentifier ?? "anonymous";

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = CorrelationId,
            ["HubName"] = GetType().Name,
            ["ConnectionId"] = Context.ConnectionId,
            ["UserId"] = userId
        }))
        {
            logger.LogInformation("Client connected to {HubName}", GetType().Name);
        }

        if (Context.UserIdentifier is not null)
        {
            await connectionManager.AddConnectionAsync(Context.UserIdentifier, Context.ConnectionId);
            await presenceTracker.UserConnectedAsync(Context.UserIdentifier, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            logger.LogWarning(exception,
                "Client {ConnectionId} disconnected from {HubName} with error",
                Context.ConnectionId, GetType().Name);
        }
        else
        {
            logger.LogInformation(
                "Client {ConnectionId} disconnected from {HubName}",
                Context.ConnectionId, GetType().Name);
        }

        await groupManager.ClearGroupMembershipsAsync(Context.ConnectionId);

        if (Context.UserIdentifier is not null)
        {
            await connectionManager.RemoveConnectionAsync(Context.ConnectionId);
            await presenceTracker.UserDisconnectedAsync(Context.UserIdentifier, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Returns the current user's ID, throwing HubException if not authenticated.
    /// </summary>
    protected long GetCurrentUserId()
    {
        if (string.IsNullOrEmpty(Context.UserIdentifier) ||
            !long.TryParse(Context.UserIdentifier, out long userId))
        {
            throw new HubException("User is not authenticated.");
        }

        return userId;
    }

    /// <summary>
    /// Tries to parse the current user's ID. Returns false if not authenticated.
    /// </summary>
    protected bool TryGetCurrentUserId(out long userId)
    {
        userId = 0;
        return !string.IsNullOrEmpty(Context.UserIdentifier) &&
               long.TryParse(Context.UserIdentifier, out userId);
    }

    /// <summary>Returns the SignalR group name for a board.</summary>
    protected static string BoardGroup(long boardId) => $"Board:{boardId}";

    /// <summary>Returns the SignalR group name for a project.</summary>
    protected static string ProjectGroup(long projectId) => $"Project:{projectId}";

    /// <summary>Returns the SignalR group name for a user (personal notifications).</summary>
    protected static string UserGroup(long userId) => $"User:{userId}";
}
