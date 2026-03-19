namespace Feezbow.Infrastructure.SignalR.Contracts;

/// <summary>
/// Strongly-typed client contract for personal/system notifications.
/// Each user subscribes to their own notification channel.
/// </summary>
public interface INotificationHubClient
{
    Task UserProfileUpdated(UserProfileUpdatedNotification notification);
    Task UserDeactivated(UserDeactivatedNotification notification);
    Task UserReactivated(UserReactivatedNotification notification);
    Task TaskAssigned(TaskAssignedNotification notification);
}
