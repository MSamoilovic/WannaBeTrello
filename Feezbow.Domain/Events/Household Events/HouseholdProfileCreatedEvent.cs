namespace Feezbow.Domain.Events.Household_Events;

public class HouseholdProfileCreatedEvent(long householdId, long projectId, long createdBy) : DomainEvent
{
    public long HouseholdId => householdId;
    public long ProjectId => projectId;
    public long CreatedBy => createdBy;
}
