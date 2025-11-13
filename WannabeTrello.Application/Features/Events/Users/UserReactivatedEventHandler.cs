using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.UserEvents;

namespace WannabeTrello.Application.Features.Events.Users;

public class UserReactivatedEventHandler(IUserNotificationService userNotificationService)
    : INotificationHandler<UserReactivatedEvent>
{
    public async Task Handle(UserReactivatedEvent notification, CancellationToken cancellationToken)
    {
        await userNotificationService.NotifyUserReactivated(
            notification.UserId,
            notification.ReactivatedByUserId,
            cancellationToken);
    }
}

