using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Column_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnWipLimitChangedEventHandler(
    IColumnNotificationService columnNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<ColumnWipLimitChangedEvent>
{
    public async Task Handle(ColumnWipLimitChangedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnWipLimitChanged(
            notification.ColumnId,
            notification.BoardId,
            notification.OldWipLimit,
            notification.NewWipLimit,
            notification.ModifierUserId);

        var oldValue = notification.OldWipLimit.HasValue ? notification.OldWipLimit.Value.ToString() : "unlimited";
        var newValue = notification.NewWipLimit.HasValue ? notification.NewWipLimit.Value.ToString() : "unlimited";
        
        var activity = ActivityTracker.Create(
            type: ActivityType.ColumnWipLimitChanged,
            description: $"Column WIP limit changed from {oldValue} to {newValue}.",
            userId: notification.ModifierUserId,
            relatedEntityId: notification.ColumnId,
            relatedEntityType: nameof(Column));

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}
