using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Chore_Events;

namespace Feezbow.Application.Features.Events.Chore;

public class ChoreAssignedEventHandler(IChoreNotificationService notifications)
    : INotificationHandler<ChoreAssignedEvent>
{
    public Task Handle(ChoreAssignedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyChoreAssigned(
            notification.ChoreId,
            notification.ProjectId,
            notification.AssignedToUserId,
            notification.PreviousUserId,
            notification.AssignedBy,
            cancellationToken);
}
