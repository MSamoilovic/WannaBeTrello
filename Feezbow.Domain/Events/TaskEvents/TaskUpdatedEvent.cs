namespace WannabeTrello.Domain.Events.TaskEvents;

public class TaskUpdatedEvent(long taskId, string? taskTitle, long modifierUserId, Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues) : DomainEvent
{
    public long TaskId { get; } = taskId;
    public long ModifierUserId { get; } = modifierUserId;
    public string? TaskName { get; } = taskTitle;
    public Dictionary<string, object?> OldValues { get; } = oldValues;
    public Dictionary<string, object?> NewValues { get; } = newValues;
    
}