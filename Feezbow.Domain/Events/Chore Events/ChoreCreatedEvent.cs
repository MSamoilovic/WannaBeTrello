namespace Feezbow.Domain.Events.Chore_Events;

public class ChoreCreatedEvent(long choreId, long projectId, string title, long? assignedToUserId, long createdBy) : DomainEvent
{
    public long ChoreId => choreId;
    public long ProjectId => projectId;
    public string Title => title;
    public long? AssignedToUserId => assignedToUserId;
    public long CreatedBy => createdBy;
}
