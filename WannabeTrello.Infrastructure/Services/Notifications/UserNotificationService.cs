using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class UserNotificationService(
    IHubContext<TrellyHub, ITrellyHub> hubContext) : IUserNotificationService
{
    public async Task NotifyUserProfileUpdated(
        long userId,
        IReadOnlyDictionary<string, object?> oldValues,
        IReadOnlyDictionary<string, object?> newValues,
        long modifyingUserId,
        CancellationToken cancellationToken)
    {
        
        await hubContext.Clients.All.UserProfileUpdated(userId, modifyingUserId);

    }

    public async Task NotifyUserDeactivated(
        long userId,
        long deactivatedByUserId,
        CancellationToken cancellationToken)
    {
        
        await hubContext.Clients.All.UserDeactivated(userId, deactivatedByUserId);
    }

    public async Task NotifyUserReactivated(
        long userId,
        long reactivatedByUserId,
        CancellationToken cancellationToken)
    {
        
        await hubContext.Clients.All.UserReactivated(userId, reactivatedByUserId);
    }
}

