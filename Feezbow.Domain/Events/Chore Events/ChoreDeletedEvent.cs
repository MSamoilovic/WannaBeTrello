namespace Feezbow.Domain.Events.Chore_Events;

public class ChoreDeletedEvent(long choreId, long projectId, long deletedBy) : DomainEvent
{
    public long ChoreId => choreId;
    public long ProjectId => projectId;
    public long DeletedBy => deletedBy;
}
