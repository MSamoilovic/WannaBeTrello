using Microsoft.Extensions.Logging;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs.Base;
using Feezbow.Infrastructure.SignalR.Services;

namespace Feezbow.Infrastructure.SignalR.Hubs;

/// <summary>
/// Hub for user presence tracking.
/// Broadcasts <see cref="UserOnlineNotification"/> when a user's first connection
/// arrives and <see cref="UserOfflineNotification"/> when their last connection drops,
/// so clients only receive one transition event per logical online/offline change.
/// </summary>
public class PresenceHub(
    ILogger<PresenceHub> logger,
    IConnectionManager connectionManager,
    IHubGroupManager groupManager,
    IPresenceTracker presenceTracker)
    : AuthorizedHub<IPresenceHubClient>(logger, connectionManager, groupManager, presenceTracker)
{
    public override async Task OnConnectedAsync()
    {
        // Capture before base updates presence so we know whether the user
        // was already online (i.e. had at least one other connection).
        var wasOnline = Context.UserIdentifier is not null
            && await PresenceTracker.IsOnlineAsync(Context.UserIdentifier);

        await base.OnConnectedAsync();

        if (!wasOnline && Context.UserIdentifier is not null)
        {
            await Clients.Others.UserOnline(new UserOnlineNotification
            {
                UserId = Context.UserIdentifier
            });

            logger.LogInformation("User {UserId} came online", Context.UserIdentifier);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Capture before base removes the connection from the presence tracker.
        var userId = Context.UserIdentifier;

        await base.OnDisconnectedAsync(exception);

        // IsOnlineAsync returns false once the user has no remaining connections.
        if (userId is not null && !await PresenceTracker.IsOnlineAsync(userId))
        {
            await Clients.Others.UserOffline(new UserOfflineNotification { UserId = userId });

            logger.LogInformation("User {UserId} went offline", userId);
        }
    }

    /// <summary>
    /// Returns the IDs of all users who are currently online.
    /// </summary>
    [HubMethod(Description = "Get all currently online user IDs")]
    public Task<IReadOnlyCollection<string>> GetOnlineUsersAsync()
        => PresenceTracker.GetOnlineUsersAsync();
}
