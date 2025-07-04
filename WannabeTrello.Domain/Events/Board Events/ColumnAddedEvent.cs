namespace WannabeTrello.Domain.Events.Board_Events;

public class ColumnAddedEvent(long boardId, long columnId, string columnName, long creatorUserId)
    : DomainEvent
{
    public long BoardId { get; } = boardId;
    public long ColumnId { get; } = columnId;
    public string ColumnName { get; } = columnName;
    public long CreatorUserId { get; } = creatorUserId;
}