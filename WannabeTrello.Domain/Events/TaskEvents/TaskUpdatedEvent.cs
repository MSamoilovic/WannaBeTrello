namespace WannabeTrello.Domain.Events.TaskEvents;

public class TaskUpdatedEvent(long taskId, long modifierUserId) : DomainEvent
{
    public long TaskId { get; } = taskId;
    public long ModifierUserId { get; } = modifierUserId;
}