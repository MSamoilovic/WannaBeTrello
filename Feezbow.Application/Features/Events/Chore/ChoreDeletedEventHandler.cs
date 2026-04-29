using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Chore_Events;

namespace Feezbow.Application.Features.Events.Chore;

public class ChoreDeletedEventHandler(IChoreNotificationService notifications)
    : INotificationHandler<ChoreDeletedEvent>
{
    public Task Handle(ChoreDeletedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyChoreDeleted(
            notification.ChoreId,
            notification.ProjectId,
            notification.DeletedBy,
            cancellationToken);
}
