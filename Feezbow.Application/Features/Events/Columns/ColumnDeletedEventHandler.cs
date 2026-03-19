using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Column_Events;

namespace Feezbow.Application.Features.Events.Columns;

public class ColumnDeletedEventHandler(
    IColumnNotificationService columnNotificationService) : INotificationHandler<ColumnDeletedEvent>
{
    public async Task Handle(ColumnDeletedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnDeletedEvent(notification.ColumnId, notification.BoardId,
            notification.ModifierUserId);
    }
}