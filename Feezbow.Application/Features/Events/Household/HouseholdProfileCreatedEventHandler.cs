using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Household_Events;

namespace Feezbow.Application.Features.Events.Household;

public class HouseholdProfileCreatedEventHandler(IHouseholdNotificationService notifications)
    : INotificationHandler<HouseholdProfileCreatedEvent>
{
    public Task Handle(HouseholdProfileCreatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyProfileCreated(
            notification.HouseholdId,
            notification.ProjectId,
            notification.CreatedBy,
            cancellationToken);
}
