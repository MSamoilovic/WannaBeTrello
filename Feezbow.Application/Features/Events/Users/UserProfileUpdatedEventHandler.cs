using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.UserEvents;

namespace Feezbow.Application.Features.Events.Users;

public class UserProfileUpdatedEventHandler(IUserNotificationService userNotificationService)
    : INotificationHandler<UserProfileUpdatedEvent>
{
    public async Task Handle(UserProfileUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await userNotificationService.NotifyUserProfileUpdated(
            notification.UserId,
            notification.OldValues,
            notification.NewValues,
            notification.ModifyingUserId,
            cancellationToken);
    }
}

