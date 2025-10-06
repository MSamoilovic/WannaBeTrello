using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Column_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnAddedEventHandler(
    IColumnNotificationService columnNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<ColumnAddedEvent>
{
    public async  Task Handle(ColumnAddedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnCreated(
            notification.ColumnId,
            notification.ColumnName,
            notification.BoardId,
            notification.CreatorUserId);
        
        var activity = ActivityTracker.Create(
            type: ActivityType.ColumnAdded,
            description: $"Column '{notification.ColumnName}' is created.",
            userId: notification.CreatorUserId,
            relatedEntityId: notification.BoardId,
            relatedEntityType: nameof(WannabeTrello.Domain.Entities.Column)
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}