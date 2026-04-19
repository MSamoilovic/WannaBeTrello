namespace Feezbow.Domain.Events.Chore_Events;

public class ChoreAssignedEvent(
    long choreId, long projectId, long? assignedToUserId, long? previousUserId, long assignedBy) : DomainEvent
{
    public long ChoreId => choreId;
    public long ProjectId => projectId;
    public long? AssignedToUserId => assignedToUserId;
    public long? PreviousUserId => previousUserId;
    public long AssignedBy => assignedBy;
}
