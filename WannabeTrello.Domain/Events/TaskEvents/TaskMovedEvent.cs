namespace WannabeTrello.Domain.Events.TaskEvents;

public class TaskMovedEvent(long taskId, long originalColumnId, long newColumnId, long? performedByUserId)
    : DomainEvent
{
    public long TaskId { get; } = taskId;
    public long OriginalColumnId { get; } = originalColumnId;
    public long NewColumnId { get; } = newColumnId;
    public long? PerformedByUserId { get; } = performedByUserId;
}