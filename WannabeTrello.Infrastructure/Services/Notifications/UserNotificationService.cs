using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR.Contracts;
using WannabeTrello.Infrastructure.SignalR.Hubs;
using WannabeTrello.Infrastructure.SignalR.Resilience;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class UserNotificationService(
    IHubContext<NotificationHub, INotificationHubClient> notificationHub,
    ResiliencePipeline pipeline,
    ILogger<UserNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IUserNotificationService
{
    public async Task NotifyUserProfileUpdated(
        long userId,
        IReadOnlyDictionary<string, object?> oldValues,
        IReadOnlyDictionary<string, object?> newValues,
        long modifyingUserId,
        CancellationToken cancellationToken)
    {
        await SendAsync(_ => new ValueTask(notificationHub.Clients
            .Group($"User:{userId}")
            .UserProfileUpdated(new UserProfileUpdatedNotification
            {
                UserId = userId,
                ModifiedBy = modifyingUserId
            })), cancellationToken);
    }

    public async Task NotifyUserDeactivated(
        long userId,
        long deactivatedByUserId,
        CancellationToken cancellationToken)
    {
        await SendAsync(_ => new ValueTask(notificationHub.Clients
            .Group($"User:{userId}")
            .UserDeactivated(new UserDeactivatedNotification
            {
                UserId = userId,
                DeactivatedBy = deactivatedByUserId
            })), cancellationToken);
    }

    public async Task NotifyUserReactivated(
        long userId,
        long reactivatedByUserId,
        CancellationToken cancellationToken)
    {
        await SendAsync(_ => new ValueTask(notificationHub.Clients
            .Group($"User:{userId}")
            .UserReactivated(new UserReactivatedNotification
            {
                UserId = userId,
                ReactivatedBy = reactivatedByUserId
            })), cancellationToken);
    }
}
