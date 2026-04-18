using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Events.Household_Events;

public class HouseholdMemberRoleRemovedEvent(
    long projectId,
    long memberId,
    HouseholdRole previousRole,
    long removedBy) : DomainEvent
{
    public long ProjectId => projectId;
    public long MemberId => memberId;
    public HouseholdRole PreviousRole => previousRole;
    public long RemovedBy => removedBy;
}
