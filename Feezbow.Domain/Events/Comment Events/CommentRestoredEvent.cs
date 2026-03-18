namespace WannabeTrello.Domain.Events.Comment_Events;

public class CommentRestoredEvent(long commentId, long taskId, long modifyingUserId): DomainEvent
{
    public long CommentId => commentId;
    public long TaskId => taskId;
    public long ModifyingUserId => modifyingUserId;
}