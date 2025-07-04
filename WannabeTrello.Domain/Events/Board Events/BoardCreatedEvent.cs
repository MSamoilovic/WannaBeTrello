namespace WannabeTrello.Domain.Events.Board_Events;

public class BoardCreatedEvent(long boardId, string boardName, long creatorUserId) : DomainEvent
{
    public long BoardId { get; } = boardId;
    public string BoardName { get; } = boardName;
    public long CreatorUserId { get; } = creatorUserId;
}