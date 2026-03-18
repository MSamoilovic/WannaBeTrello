namespace WannabeTrello.Domain.Events.Comment_Events;

public class CommentUpdatedEvent(long commentId, long taskId, Dictionary<string, object?> oldContent, Dictionary<string, object?> newContent, long modifyingUserId): DomainEvent
{
    public long CommentId => commentId;
    public long TaskId => taskId;
    public Dictionary<string, object?> OldContent => oldContent;
    public Dictionary<string, object?> NewContent => newContent;
    public long ModifyingUserId => modifyingUserId;
}