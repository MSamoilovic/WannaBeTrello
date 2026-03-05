using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR.Contracts;
using WannabeTrello.Infrastructure.SignalR.Hubs;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class UserNotificationService(
    IHubContext<NotificationHub, INotificationHubClient> notificationHub) : IUserNotificationService
{
    public async Task NotifyUserProfileUpdated(
        long userId,
        IReadOnlyDictionary<string, object?> oldValues,
        IReadOnlyDictionary<string, object?> newValues,
        long modifyingUserId,
        CancellationToken cancellationToken)
    {
        await notificationHub.Clients
            .Group($"User:{userId}")
            .UserProfileUpdated(new UserProfileUpdatedNotification
            {
                UserId = userId,
                ModifiedBy = modifyingUserId
            });
    }

    public async Task NotifyUserDeactivated(
        long userId,
        long deactivatedByUserId,
        CancellationToken cancellationToken)
    {
        await notificationHub.Clients
            .Group($"User:{userId}")
            .UserDeactivated(new UserDeactivatedNotification
            {
                UserId = userId,
                DeactivatedBy = deactivatedByUserId
            });
    }

    public async Task NotifyUserReactivated(
        long userId,
        long reactivatedByUserId,
        CancellationToken cancellationToken)
    {
        await notificationHub.Clients
            .Group($"User:{userId}")
            .UserReactivated(new UserReactivatedNotification
            {
                UserId = userId,
                ReactivatedBy = reactivatedByUserId
            });
    }
}

