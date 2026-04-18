using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Events.Household_Events;

public class HouseholdMemberRoleAssignedEvent(
    long projectId,
    long memberId,
    HouseholdRole role,
    HouseholdRole? previousRole,
    long assignedBy) : DomainEvent
{
    public long ProjectId => projectId;
    public long MemberId => memberId;
    public HouseholdRole Role => role;
    public HouseholdRole? PreviousRole => previousRole;
    public long AssignedBy => assignedBy;
}
