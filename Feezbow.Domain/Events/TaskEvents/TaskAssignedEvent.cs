namespace WannabeTrello.Domain.Events.TaskEvents;

public class TaskAssignedEvent(long taskId, long? oldAssigneeId, long? newAssigneeId, long assignedByUserId) : DomainEvent
{
    public long TaskId => taskId;
    public long? OldAssigneeId => oldAssigneeId;
    public long? NewAssigneeId => newAssigneeId;
    public long AssignedByUserId => assignedByUserId;
}