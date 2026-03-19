using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.UserEvents;

namespace Feezbow.Application.Features.Events.Users;

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

