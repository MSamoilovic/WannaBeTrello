namespace WannabeTrello.Domain.Events;

public class BoardMemberRemovedEvent(long boardId, long userId, long removerUserId) : DomainEvent
{
    public long BoardId { get; } = boardId;
    public long UserId { get; } = userId;
    public long RemoverUserId { get; } = removerUserId;
}