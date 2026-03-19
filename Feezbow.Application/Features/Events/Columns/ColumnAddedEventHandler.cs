using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Column_Events;

namespace Feezbow.Application.Features.Events.Columns;

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