namespace WannabeTrello.Domain.Events;

public class BoardUpdatedEvent(long boardId, long modifierUserId) : DomainEvent
{
    public long BoardId { get; } = boardId;
    public long ModifierUserId { get; } = modifierUserId;
}