using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.UserEvents;

namespace WannabeTrello.Application.Features.Events.Users;

public class UserDeactivatedEventHandler(IUserNotificationService userNotificationService)
    : INotificationHandler<UserDeactivatedEvent>
{
    public async Task Handle(UserDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        await userNotificationService.NotifyUserDeactivated(
            notification.UserId,
            notification.DeactivatedByUserId,
            cancellationToken);
    }
}

