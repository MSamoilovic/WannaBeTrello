using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Column_Events;

namespace WannabeTrello.Application.Features.Events.Columns;

public class ColumnAddedEventHandler(
    IColumnNotificationService columnNotificationService) : INotificationHandler<ColumnAddedEvent>
{
    public async  Task Handle(ColumnAddedEvent notification, CancellationToken cancellationToken)
    {
        await columnNotificationService.NotifyColumnCreated(
            notification.ColumnId,
            notification.ColumnName,
            notification.BoardId,
            notification.CreatorUserId);
    }
}