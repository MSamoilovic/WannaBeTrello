namespace Feezbow.Domain.Events.Household_Events;

public class HouseholdProfileUpdatedEvent(
    long householdId,
    long projectId,
    long updatedBy,
    Dictionary<string, object?> oldValues,
    Dictionary<string, object?> newValues) : DomainEvent
{
    public long HouseholdId => householdId;
    public long ProjectId => projectId;
    public long UpdatedBy => updatedBy;
    public Dictionary<string, object?> OldValues => oldValues;
    public Dictionary<string, object?> NewValues => newValues;
}
