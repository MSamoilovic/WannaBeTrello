using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardCreatedEventHandler(
    IBoardNotificationService notificationService,
    IActivityTrackerService activityTrackerService)
    : INotificationHandler<BoardCreatedEvent>
{
    public async Task Handle(BoardCreatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.NotifyBoardCreated(notification.BoardId, notification.BoardName,
            notification.CreatorUserId);
        
        var initialValues = new Dictionary<string, object?>
        {
            { "Name", notification.BoardName },
            { "Description", notification.Description },
            { "OwnerId", notification.CreatorUserId },
            { "IsArchived", false }
        };
        
        var activity = ActivityTracker.Create(
            type: ActivityType.BoardCreated,
            description: $"Project '{notification.BoardName}' is created.",
            userId: notification.CreatorUserId,
            relatedEntityId: notification.BoardId,
            relatedEntityType: nameof(WannabeTrello.Domain.Entities.Project),
            newValue: initialValues
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}