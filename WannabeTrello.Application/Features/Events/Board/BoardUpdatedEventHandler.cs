using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardUpdatedEventHandler(
    IBoardNotificationService notificationService,
    IActivityTrackerService activityTrackerService)
    : INotificationHandler<BoardUpdatedEvent>
{
    public async Task Handle(BoardUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.NotifyBoardUpdated(notification.BoardId, notification.ModifierUserId);

        var activity = ActivityTracker.Create(
            type: ActivityType.BoardUpdated,
            description: $"Board '{notification.BoardId}' has been updated.",
            userId: notification.ModifierUserId,
            relatedEntityId: notification.BoardId,
            relatedEntityType: "Board",
            oldValue: notification.OldValue,
            newValue: notification.NewValue
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}