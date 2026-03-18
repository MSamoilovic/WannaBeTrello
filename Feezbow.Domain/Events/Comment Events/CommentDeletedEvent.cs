namespace WannabeTrello.Domain.Events.Comment_Events;

public class CommentDeletedEvent(long commentId, long taskId, long modifierUserId): DomainEvent
{
    public long CommentId => commentId;
    public long TaskId => taskId;
    public long ModifyingUserId => modifierUserId;
}