using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Column_Events;

namespace Feezbow.Application.Features.Events.Columns;

public class ColumnWipLimitChangedEventHandler(
    IColumnNotificationService columnNotificationService) : INotificationHandler<ColumnWipLimitChangedEvent>
{
    public async Task Handle(ColumnWipLimitChangedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnWipLimitChanged(
            notification.ColumnId,
            notification.BoardId,
            notification.OldWipLimit,
            notification.NewWipLimit,
            notification.ModifierUserId);
    }
}
