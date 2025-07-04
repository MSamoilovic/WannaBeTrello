namespace WannabeTrello.Domain.Events.TaskEvents;

public class TaskCommentedEvent(long taskId, long commentId, long commentAuthorId, long boardId)
    : DomainEvent
{
    public long TaskId { get; } = taskId;
    public long CommentId { get; } = commentId;
    public long CommentAuthorId { get; } = commentAuthorId;
    public long BoardId { get; } = boardId;
}