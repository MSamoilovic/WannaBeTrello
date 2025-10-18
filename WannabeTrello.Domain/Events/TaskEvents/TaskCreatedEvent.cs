namespace WannabeTrello.Domain.Events.TaskEvents;

public class TaskCreatedEvent(long taskId, string taskTitle, long creatorUserId, long assigneeId) : DomainEvent
{
    public long TaskId => taskId;
    public string TaskTitle => taskTitle;
    public long CreatorUserId => creatorUserId;
    public long AssigneeId => assigneeId;
}