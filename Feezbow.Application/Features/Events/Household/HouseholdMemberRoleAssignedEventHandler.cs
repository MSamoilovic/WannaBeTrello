using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Household_Events;

namespace Feezbow.Application.Features.Events.Household;

public class HouseholdMemberRoleAssignedEventHandler(IHouseholdNotificationService notifications)
    : INotificationHandler<HouseholdMemberRoleAssignedEvent>
{
    public Task Handle(HouseholdMemberRoleAssignedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyMemberRoleAssigned(
            notification.ProjectId,
            notification.MemberId,
            notification.Role,
            notification.PreviousRole,
            notification.AssignedBy,
            cancellationToken);
}
