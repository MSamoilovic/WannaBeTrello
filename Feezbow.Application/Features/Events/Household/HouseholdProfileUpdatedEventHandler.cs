using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Household_Events;

namespace Feezbow.Application.Features.Events.Household;

public class HouseholdProfileUpdatedEventHandler(IHouseholdNotificationService notifications)
    : INotificationHandler<HouseholdProfileUpdatedEvent>
{
    public Task Handle(HouseholdProfileUpdatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyProfileUpdated(
            notification.HouseholdId,
            notification.ProjectId,
            notification.UpdatedBy,
            notification.NewValues,
            cancellationToken);
}
