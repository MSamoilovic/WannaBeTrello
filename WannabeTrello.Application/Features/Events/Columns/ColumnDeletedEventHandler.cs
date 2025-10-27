using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Column_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnDeletedEventHandler(
    IColumnNotificationService columnNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<ColumnDeletedEvent>
{
    public async Task Handle(ColumnDeletedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnDeletedEvent(notification.ColumnId, notification.BoardId,
            notification.ModifierUserId);
        
        var activity = ActivityTracker.Create(
            type: ActivityType.ColumnDeleted,
            description: $"Column '{notification.ColumnId}' was deleted.",
            userId: notification.ModifierUserId,
            relatedEntityId: notification.ColumnId,
            relatedEntityType: nameof(Column));
        
        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}