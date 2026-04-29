using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Chore_Events;

namespace Feezbow.Application.Features.Events.Chore;

public class ChoreCreatedEventHandler(IChoreNotificationService notifications)
    : INotificationHandler<ChoreCreatedEvent>
{
    public Task Handle(ChoreCreatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyChoreCreated(
            notification.ChoreId,
            notification.ProjectId,
            notification.Title,
            notification.AssignedToUserId,
            notification.CreatedBy,
            cancellationToken);
}
