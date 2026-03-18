using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.UserEvents;

namespace WannabeTrello.Application.Features.Events.Users;

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

