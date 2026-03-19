using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Column_Events;

namespace Feezbow.Application.Features.Events.Columns;

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
