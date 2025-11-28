using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Column_Events;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnDeletedEventHandler(
    IColumnNotificationService columnNotificationService) : INotificationHandler<ColumnDeletedEvent>
{
    public async Task Handle(ColumnDeletedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnDeletedEvent(notification.ColumnId, notification.BoardId,
            notification.ModifierUserId);
    }
}