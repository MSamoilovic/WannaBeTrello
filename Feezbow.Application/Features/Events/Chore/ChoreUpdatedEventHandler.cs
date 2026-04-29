using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Chore_Events;

namespace Feezbow.Application.Features.Events.Chore;

public class ChoreUpdatedEventHandler(IChoreNotificationService notifications)
    : INotificationHandler<ChoreUpdatedEvent>
{
    public Task Handle(ChoreUpdatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyChoreUpdated(
            notification.ChoreId,
            notification.ProjectId,
            notification.UpdatedBy,
            cancellationToken);
}
