using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.UserEvents;

namespace Feezbow.Application.Features.Events.Users;

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

