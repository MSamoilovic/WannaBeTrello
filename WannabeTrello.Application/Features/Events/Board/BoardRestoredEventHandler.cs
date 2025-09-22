using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardRestoredEventHandler(
    IBoardNotificationService boardNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<BoardRestoredEvent>
{
    public async Task Handle(BoardRestoredEvent notification, CancellationToken cancellationToken)
    {
        await boardNotificationService.NotifyBoardRestored(notification.BoardId, notification.ModifierUserId);
        
        var activity = ActivityTracker.Create(
            type: ActivityType.BoardRestored,
            description: $"Board '{notification.BoardId}' is restored.",
            userId: notification.ModifierUserId,
            relatedEntityId: notification.BoardId,
            relatedEntityType: nameof(Domain.Entities.Board));
        
        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}