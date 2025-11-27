using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.ColumnEvents;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnUpdatedEventHandler(
    IColumnNotificationService columnNotificationService) : INotificationHandler<ColumnUpdatedEvent>
{
    public async Task Handle(ColumnUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnUpdated(
            notification.ColumnId,
            notification.OldName,
            notification.NewName,
            notification.BoardId,
            notification.ModifierUserId);
    }
}
