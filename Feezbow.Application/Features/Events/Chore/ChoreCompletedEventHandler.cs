using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Chore_Events;

namespace Feezbow.Application.Features.Events.Chore;

public class ChoreCompletedEventHandler(IChoreNotificationService notifications)
    : INotificationHandler<ChoreCompletedEvent>
{
    public Task Handle(ChoreCompletedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyChoreCompleted(
            notification.ChoreId,
            notification.ProjectId,
            notification.CompletedBy,
            cancellationToken);
}
