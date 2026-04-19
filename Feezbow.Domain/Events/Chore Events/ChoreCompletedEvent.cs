namespace Feezbow.Domain.Events.Chore_Events;

public class ChoreCompletedEvent(long choreId, long projectId, long completedBy) : DomainEvent
{
    public long ChoreId => choreId;
    public long ProjectId => projectId;
    public long CompletedBy => completedBy;
}
