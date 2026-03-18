using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Column_Events;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnOrderChangedEventHandler(
    IColumnNotificationService columnNotificationService) : INotificationHandler<ColumnOrderChangedEvent>
{
    public async Task Handle(ColumnOrderChangedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnOrderChanged(
            notification.ColumnId,
            notification.BoardId,
            notification.OldOrder,
            notification.NewOrder,
            notification.ModifierUserId);
    }
}
