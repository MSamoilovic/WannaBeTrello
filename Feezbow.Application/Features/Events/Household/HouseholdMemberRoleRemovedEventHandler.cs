using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Household_Events;

namespace Feezbow.Application.Features.Events.Household;

public class HouseholdMemberRoleRemovedEventHandler(IHouseholdNotificationService notifications)
    : INotificationHandler<HouseholdMemberRoleRemovedEvent>
{
    public Task Handle(HouseholdMemberRoleRemovedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyMemberRoleRemoved(
            notification.ProjectId,
            notification.MemberId,
            notification.PreviousRole,
            notification.RemovedBy,
            cancellationToken);
}
