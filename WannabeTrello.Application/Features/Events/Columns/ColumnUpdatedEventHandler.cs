using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Column_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnUpdatedEventHandler(
    IColumnNotificationService columnNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<ColumnUpdatedEvent>
{
    public async Task Handle(ColumnUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnUpdated(
            notification.ColumnId,
            notification.OldName,
            notification.NewName,
            notification.BoardId,
            notification.ModifierUserId);

        var activity = ActivityTracker.Create(
            type: ActivityType.ColumnUpdated,
            description: $"Column '{notification.OldName}' was renamed to '{notification.NewName}'.",
            userId: notification.ModifierUserId,
            relatedEntityId: notification.ColumnId,
            relatedEntityType: nameof(Column));

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}
