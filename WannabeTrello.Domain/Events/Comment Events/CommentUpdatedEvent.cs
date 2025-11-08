namespace WannabeTrello.Domain.Events.Comment_Events;

public class CommentUpdatedEvent(long commentId, long taskId, string? oldContent, string? newContent, long modifyingUserId): DomainEvent
{
    public long CommentId => commentId;
    public long TaskId => taskId;
    public string? OldContent => oldContent;
    public string? NewContent => newContent;
    public long ModifyingUserId => modifyingUserId;
}