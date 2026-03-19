namespace Feezbow.Infrastructure.SignalR.Contracts;

/// <summary>
/// Strongly-typed client contract for user presence tracking.
/// </summary>
public interface IPresenceHubClient
{
    Task UserOnline(UserOnlineNotification notification);
    Task UserOffline(UserOfflineNotification notification);
}
