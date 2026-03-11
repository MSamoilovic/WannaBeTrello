using Microsoft.Extensions.Logging;
using WannabeTrello.Infrastructure.SignalR.Contracts;
using WannabeTrello.Infrastructure.SignalR.Hubs.Base;
using WannabeTrello.Infrastructure.SignalR.Services;

namespace WannabeTrello.Infrastructure.SignalR.Hubs;

/// <summary>
/// Hub for personal real-time notifications.
/// Each authenticated user is automatically subscribed to their own
/// <c>User:{userId}</c> group on connect, so notification services can
/// target individual users without the client calling any extra methods.
/// </summary>
public class NotificationHub(
    ILogger<NotificationHub> logger,
    IConnectionManager connectionManager,
    IHubGroupManager groupManager,
    IPresenceTracker presenceTracker)
    : AuthorizedHub<INotificationHubClient>(logger, connectionManager, groupManager, presenceTracker)
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var userId = GetCurrentUserId();
        var group = UserGroup(userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        await GroupManager.TrackGroupMembershipAsync(Context.ConnectionId, group);

        logger.LogInformation("User {UserId} auto-subscribed to {Group}", userId, group);
    }
}
