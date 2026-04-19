namespace Feezbow.Domain.Events.Chore_Events;

public class ChoreUpdatedEvent(long choreId, long projectId, long updatedBy) : DomainEvent
{
    public long ChoreId => choreId;
    public long ProjectId => projectId;
    public long UpdatedBy => updatedBy;
}
