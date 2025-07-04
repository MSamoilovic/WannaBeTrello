namespace WannabeTrello.Domain.Events.TaskEvents;

public class TaskCreatedEvent(long taskId, string taskTitle, long creatorUserId) : DomainEvent
{
    public long TaskId { get; } = taskId;
    public string TaskTitle { get; } = taskTitle;
    public long CreatorUserId { get; } = creatorUserId;
}