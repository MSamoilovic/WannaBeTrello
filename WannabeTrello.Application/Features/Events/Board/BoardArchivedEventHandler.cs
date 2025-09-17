using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardArchivedEventHandler(IBoardNotificationService boardNotificationService, IActivityTrackerService activityTrackerService): INotificationHandler<BoardArchivedEvent>
{
    public async  Task Handle(BoardArchivedEvent notification, CancellationToken cancellationToken)
    {   
        await boardNotificationService.NotifyBoardArchived(notification.BoardId, notification.ModifierUserId);
        
        var activity = ActivityTracker.Create(
            type: ActivityType.BoardArchived,
            description: $"Board '{notification.BoardId}' is archived.",
            userId: notification.ModifierUserId,
            relatedEntityId: notification.BoardId,
            relatedEntityType: nameof(Domain.Entities.Board));
        
        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}