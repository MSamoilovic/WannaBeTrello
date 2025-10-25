using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Column_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnOrderChangedEventHandler(
    IColumnNotificationService columnNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<ColumnOrderChangedEvent>
{
    public async Task Handle(ColumnOrderChangedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnOrderChanged(
            notification.ColumnId,
            notification.BoardId,
            notification.OldOrder,
            notification.NewOrder,
            notification.ModifierUserId);

        var activity = ActivityTracker.Create(
            type: ActivityType.ColumnOrderChanged,
            description: $"Column order changed from {notification.OldOrder} to {notification.NewOrder}.",
            userId: notification.ModifierUserId,
            relatedEntityId: notification.ColumnId,
            relatedEntityType: nameof(Column));

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}
