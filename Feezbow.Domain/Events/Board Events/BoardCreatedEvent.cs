namespace Feezbow.Domain.Events.Board_Events;

public class BoardCreatedEvent(long boardId, string? boardName, string? description, long creatorUserId, long projectId) : DomainEvent
{
    public long BoardId { get; } = boardId;
    public string? BoardName { get; } = boardName;
    public string? Description { get; } = description;
    public long CreatorUserId { get; } = creatorUserId;
    public long ProjectId { get; } = projectId;
}